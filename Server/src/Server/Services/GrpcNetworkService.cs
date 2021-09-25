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
        private readonly IDistributedCache _cache;
        private readonly static string s_region = "region";
        private readonly static string s_authorization = "authorization";
        public GrpcNetworkService(Orleans.IClusterClient clusterClient, ILogger<GrpcNetworkService> logger, IDistributedCache cache)
        {
            _clusterClient = clusterClient;
            _logger = logger;
            _cache = cache;
        }
        async public override Task<UUID> GetAuth(AuthRequest request, ServerCallContext context)
        {
            Guid guid;
            try
            {
                if(!Guid.TryParse( await _cache.GetStringAsync($"player-{request.Name}"), out guid))
                {
                    guid = System.Guid.NewGuid();
                    await _cache.SetStringAsync($"player-{request.Name}", guid.ToString());
                }
                await _cache.SetStringAsync(guid.ToString(), request.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            UUID ret = new() { Value = Google.Protobuf.ByteString.CopyFrom(guid.ToByteArray()) };
            return ret;
        }

        async private Task<(string name,int regionIndex)> GetContextName(ServerCallContext context)
        {
            string name = await _cache.GetStringAsync(context.RequestHeaders.Get(s_authorization).Value);
            int regionIndex = 0;
            if( int.TryParse(context.RequestHeaders.Get(s_region).Value, out regionIndex))
            {

            }
            return (name, regionIndex);
        }
        async private Task<IPlayerGrain> GetPlayer(ServerCallContext context)
        {
            var (name,regionIndex) = await GetContextName(context);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return _clusterClient.GetGrain<IPlayerGrain>(regionIndex,name);
        }

        async public override Task ServerStreamServerEvents(global::Google.Protobuf.WellKnownTypes.Empty empty, IServerStreamWriter<StreamServerEventsResponse> responseStreamWriter, ServerCallContext context)
        {
            string strGuid = context.RequestHeaders.Get(s_authorization).Value;
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
            var roomStream = _clusterClient
                .GetStreamProvider(Server.Program.s_streamProviderName)
                .GetStream<StreamServerEventsResponse>(guid, PlayerGrain.GetChatRoomQueueStreamNamespace(regionIndex));
            var streamObserver = new ChatRoomStreamObserver(guid, responseStreamWriter, roomStream, context.CancellationToken);

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
            string strGuid = context.RequestHeaders.Get(s_authorization).Value;
            var (name, regionIndex) = await GetContextName(context);
            var player = _clusterClient.GetGrain<IPlayerGrain>(request.RegionIndex, name);
            if (player is null)
            {
                return default;
            }
            Guid guid = Guid.Parse(strGuid);
            return await player.GetPlayerDataAsync();
        }
        async public override Task<AddPointResponse> AddPoint(AddPointRequest request, ServerCallContext context)
        {
            var player = await GetPlayer(context);
            if (player is null)
            {
                return default;
            }
            return new() { AddedPoint = await player.AddPointAsync(request.AddPoint) };
        }

        async public override Task<ChatResponse> Chat(ChatRequest request, ServerCallContext context)
        {
            var (name, regionIndex) = await GetContextName(context);
            var roomStream = _clusterClient.GetStreamProvider(Server.Program.s_streamProviderName)
   .GetStream<StreamServerEventsResponse>(Guid.Empty, PlayerGrain.GetChatRoomQueueStreamNamespace(regionIndex));
            StreamServerEventsResponse grpcStreamResponse = new()
            {
                OnChat = new()
                {
                    RoomInfo = $"chatroom{regionIndex}",
                    OtherPlayer = name,
                    Message = request.Message
                }
            };
            await roomStream.OnNextAsync(grpcStreamResponse);
            return new() { Success = true };
        }

        async public override Task<PlayerDataList> GetRegionPlayerDataList(Empty request, ServerCallContext context)
        {
            PlayerDataList ret = new PlayerDataList();
            var (name, _) = await GetContextName(context);
            foreach (var regionIndex in Program.s_regionList)
            {
                var player = _clusterClient.GetGrain<IPlayerGrain>(regionIndex, name);
                ret.PlayerDataList_.Add( await player.GetPlayerDataAsync());
            }
            return ret;
        }

        async public override Task<RankList> GetTopRankList(Empty request, ServerCallContext context)
        {
            var (name, regionIndex) = await GetContextName(context);
            var rankingGrain = _clusterClient.GetGrain<IRankingGrain>(regionIndex);
            var topRanks = await rankingGrain.GetTopRanks();
            var ret = new RankList();
            ret.Ranks.AddRange(topRanks);
            return ret;
        }
    }
}
