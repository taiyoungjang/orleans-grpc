using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes;
using game;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Server
{
    public class GrpcNetworkService : game.PlayerNetwork.PlayerNetworkBase
    {
        private readonly ILogger<GrpcNetworkService> _logger;
        private readonly Orleans.IClusterClient _clusterClient;
        private static game.RoomList s_emptyRoomList = new();
        private readonly static string s_region = "region";
        private readonly static string s_otp = "otp";
        public GrpcNetworkService(Orleans.IClusterClient clusterClient, ILogger<GrpcNetworkService> logger)
        {
            _clusterClient = clusterClient;
            _logger = logger;
        }
        async public override Task<AuthResponse> GetAuth(AuthRequest request, ServerCallContext context)
        {
            AuthResponse ret = new AuthResponse();
            AuthData authData;
            OtpData otpData;
            try
            {
                {
                    var authGrain = _clusterClient.GetGrain<IAuthGrain>(request.FirebaseId);
                    authData = await authGrain.GetAuthDataAsync();
                    if (authData.AccountGuid.Equals(System.Guid.Empty))
                    {
                        authData = await authGrain.SetNewAccountGuid();
                    }
                    if (authData.AccountGuid.Equals(System.Guid.Empty))
                    {
                        throw new System.Exception($"FirebaseId:{request.FirebaseId} accountUid:empty");
                    }
                }

                IOtpGrain otpGrain;
                Guid otpGuid = System.Guid.Empty;
                if (request.Otp != null)
                {
                    otpGuid = new System.Guid(request.Otp.Value.Span);
                }
                if(otpGuid.Equals(System.Guid.Empty))
                {
                    otpGrain = _clusterClient.GetGrain<IOtpGrain>(System.Guid.NewGuid());
                    otpData = await otpGrain.SetAccountGuidAsync(authData.AccountGuid);
                }
                else
                {
                    otpGrain = _clusterClient.GetGrain<IOtpGrain>(otpGuid);
                    otpData = await otpGrain.GetOtpAsync();
                }
                if (otpData.AccountGuid.Equals(System.Guid.Empty))
                {
                    throw new System.Exception($"FirebaseId:{request.FirebaseId} accountUid:empty");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            ret.Otp = new() { Value = Google.Protobuf.ByteString.CopyFrom(otpData.Otp.ToByteArray()) };
            return ret;
        }

        async private Task<(Guid accountGuid, Guid playerGuid, string playerName, long regionIndex)> GetContextName(ServerCallContext context)
        {
            var otpGrain = _clusterClient.GetGrain<IOtpGrain>(System.Guid.Parse(context.RequestHeaders.Get(s_otp).Value));
            OtpData otpData = await otpGrain.GetOtpAsync();
            long regionIndex = 0;
            if( !long.TryParse(context.RequestHeaders.Get(s_region).Value, out regionIndex))
            {

            }
            otpData = await otpGrain.GetOtpAsync();
            return (otpData.AccountGuid, otpData.PlayerGuid, otpData.PlayerName, regionIndex);
        }
        async private Task<IPlayerGrain> GetPlayer(ServerCallContext context)
        {
            var (accountGuid, playerGuid, _, _) = await GetContextName(context);
            if (accountGuid.Equals(System.Guid.Empty))
            {
                return null;
            }
            return _clusterClient.GetGrain<IPlayerGrain>(playerGuid);
        }

        async public override Task ServerStreamServerEvents(global::Google.Protobuf.WellKnownTypes.Empty empty, IServerStreamWriter<StreamServerEventsResponse> responseStreamWriter, ServerCallContext context)
        {
            string strGuid = context.RequestHeaders.Get(s_otp).Value;
            long regionIndex = 0;
            if (!long.TryParse(context.RequestHeaders.Get(s_region).Value, out regionIndex))
            {
                _logger.LogError("");

            }
            var player = await GetPlayer(context);
            if (player is null)
            {
                return;
            }
            Guid guid = Guid.Empty;
            var regionStream = _clusterClient
                .GetStreamProvider(Server.Program.s_streamProviderName)
                .GetStream<StreamServerEventsResponse>(guid, PlayerGrain.GetRegionQueueStreamNamespace(regionIndex));
            var streamObserver = new RegionStreamObserver(guid, responseStreamWriter, regionStream, context.CancellationToken);

            try
            {
                await streamObserver.WaitConsumerTask();
            }
            catch (Exception)
            {

            }

        }

        async public override Task<PlayerData> LoginPlayerData(RegionData request, ServerCallContext context)
        {
            var (accountGuid, playerGuid, _, regionIndex) = await GetContextName(context);
            var playerDataListGrain = _clusterClient.GetGrain<IPlayerDataListGrain>(accountGuid);
            var playerListData = await playerDataListGrain.GetPlayerGuidListDataAsync();
            var playerGuidWithRegionIndex = playerListData.FirstOrDefault(t => t.RegionIndex == t.RegionIndex);
            if(playerGuidWithRegionIndex.Equals(default(PlayerGuid)))
            {
                return default;
            }
            regionIndex = playerGuidWithRegionIndex.RegionIndex;
            playerGuid = playerGuidWithRegionIndex.Player;
            var player = _clusterClient.GetGrain<IPlayerGrain>(playerGuid);
            if (player is null)
            {
                return default;
            }
            var otpGrain = _clusterClient.GetGrain<IOtpGrain>(System.Guid.Parse(context.RequestHeaders.Get(s_otp).Value));
            await otpGrain.SetPlayerGuidAsync(regionIndex, playerGuid, playerGuidWithRegionIndex.PlayerName);
            return await player.GetPlayerDataAsync();
        }
        async public override Task<CreatePlayerResponse> CreatePlayer(CreatePlayerRequest request, ServerCallContext context)
        {
            var (accountGuid, playerGuid,_, regionIndex) = await GetContextName(context);
            var playerDataListGrain = _clusterClient.GetGrain<IPlayerDataListGrain>(accountGuid);
            var playerListData = await playerDataListGrain.GetPlayerGuidListDataAsync();
            if (playerListData.Any(t=> t.RegionIndex == request.RegionIndex))
            {
                return new CreatePlayerResponse() { ErrorCode = ErrorCode.AlreadyCreatedCharacter };
            }
            playerGuid = System.Guid.NewGuid();
            regionIndex = request.RegionIndex;
            var uniqueNameGrain = _clusterClient.GetGrain<IUniqueNameGrain>(regionIndex);
            {
                var result = await uniqueNameGrain.SetPlayerName(request.Name, playerGuid);
                if (result != ErrorCode.Success)
                {
                    return new() { ErrorCode = result };
                }
            }
            var player = _clusterClient.GetGrain<IPlayerGrain>(playerGuid);
            if (player is null)
            {
                return new CreatePlayerResponse();
            }
            var playerData = await player.GetPlayerDataAsync();
            if( !string.IsNullOrEmpty(playerData.Name))
            {
                return new CreatePlayerResponse();
            }
            if(await player.CreatePlayerAsync(regionIndex, request.Name) != ErrorCode.Success)
            {
                return new CreatePlayerResponse();
            }
            await playerDataListGrain.SetPlayerListData(request.RegionIndex, playerGuid, playerName: request.Name);
            return new CreatePlayerResponse() { ErrorCode =  ErrorCode.Success };
        }
        async public override Task<UpdateStageResponse> UpdateStage(UpdateStageRequest request, ServerCallContext context)
        {
            var player = await GetPlayer(context);
            if (player is null)
            {
                return default;
            }
            return new() { ErrorCode = await player.UpdateStageAsync(request.Stage) };
        }

        async public override Task<ChatResponse> Chat(ChatRequest request, ServerCallContext context)
        {
            var (accountGuid, playerGuid, playerName, regionIndex) = await GetContextName(context);
            var roomStream = _clusterClient
                .GetStreamProvider(Server.Program.s_streamProviderName)
                .GetStream<StreamServerEventsResponse>(Guid.Empty, PlayerGrain.GetRegionQueueStreamNamespace(regionIndex));
            StreamServerEventsResponse grpcStreamResponse = new()
            {
                OnChat = new()
                {
                    RegionIndex = regionIndex,
                    OtherPlayer = playerName,
                    Message = request.Message
                }
            };
            await roomStream.OnNextAsync(grpcStreamResponse);
            return new() { ErrorCode = ErrorCode.Success };
        }

        async public override Task<PlayerDataList> GetRegionPlayerDataList(Empty request, ServerCallContext context)
        {
            PlayerDataList ret = new PlayerDataList();
            var (accountGuid, _,_, _) = await GetContextName(context);
            var playerDataListGrain = _clusterClient.GetGrain<IPlayerDataListGrain>(accountGuid);
            var playerListData = await playerDataListGrain.GetPlayerGuidListDataAsync();

            foreach (var playerGuid in playerListData)
            {
                var player = _clusterClient.GetGrain<IPlayerGrain>(playerGuid.Player);
                ret.PlayerDataList_.Add( await player.GetPlayerDataAsync());
            }
            return ret;
        }

        async public override Task<RankList> GetTopRankList(Empty request, ServerCallContext context)
        {
            var (_, _, _, regionIndex) = await GetContextName(context);
            if(regionIndex == default)
            {
                return default;
            }
            var rankingGrain = _clusterClient.GetGrain<IStageRankingGrain>(regionIndex);
            var topRanks = await rankingGrain.GetTopRanks();
            var ret = new RankList();
            ret.Ranks.AddRange(topRanks);
            return ret;
        }
    }
}
