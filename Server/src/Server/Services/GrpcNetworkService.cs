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
                var player = _clusterClient.GetGrain<IPlayerGrain>(request.Name);
                if(!Guid.TryParse( await _cache.GetStringAsync($"player-{request.Name}"), out guid))
                {
                    guid = System.Guid.NewGuid();
                    await _cache.SetStringAsync($"player-{request.Name}", guid.ToString());
                }
                guid = await player.SetStreamAsync(guid);
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

        async private Task<string> GetContextName(ServerCallContext context)
        {
            return await _cache.GetStringAsync(context.RequestHeaders.Get("authorization").Value);
        }
        async private Task<IPlayerGrain> GetPlayer(ServerCallContext context)
        {
            var name = await GetContextName(context);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            return _clusterClient.GetGrain<IPlayerGrain>(name);
        }

        async public override Task ServerStreamServerEvents(global::Google.Protobuf.WellKnownTypes.Empty empty, IServerStreamWriter<StreamServerEventsResponse> responseStreamWriter, ServerCallContext context)
        {
            string strGuid = context.RequestHeaders.Get("authorization").Value;
            var player = await GetPlayer(context);
            if (player is null)
            {
                return;
            }
            Guid guid = Guid.Parse(strGuid);
            async Task EndOfAsyncStream()
            {
                await player.EndOfAsyncStreamAsync();
            }
            var playerStream = _clusterClient
                .GetStreamProvider(PlayerGrain.s_streamProviderName)
                .GetStream<StreamServerEventsResponse>(guid, PlayerGrain.s_streamNamespace);
            var streamObserver = new OrleansStreamObserver(guid, responseStreamWriter, playerStream, EndOfAsyncStream, context.CancellationToken);
            await streamObserver.WaitConsumerTask();
        }

        async public override Task<PlayerData> GetPlayerData(Empty request, ServerCallContext context)
        {
            var player = await GetPlayer(context);
            if (player is null)
            {
                return default;
            }
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
            var player = await GetPlayer(context);
            if (player is null)
            {
                return default;
            }
            var ret = await player.ChatAsync(room: request.Room, message: request.Message);
            return new() { Success = ret };
        }
        async public override Task<JoinResponse> Join(JoinRequest request, ServerCallContext context)
        {
            var player = await GetPlayer(context);
            if (player is null)
            {
                return default;
            }
            var joinRet = await player.JoinAsync(room: request.Room);
            JoinResponse ret = new() { Success = joinRet.ret};
            ret.Players.AddRange(joinRet.players);
            return ret;
        }
        async public override Task<LeaveResponse> Leave(LeaveRequest request, ServerCallContext context)
        {
            var player = await GetPlayer(context);
            if (player is null)
            {
                return default;
            }
            var leaveRet = await player.LeaveAsync(room: request.Room);
            LeaveResponse ret = new() { Success = leaveRet };
            return ret;
        }
        async public override Task<RoomList> GetAvailableRoomList(Empty request, ServerCallContext context)
        {
            var roomManager = _clusterClient.GetGrain<IRoomManagerGrain>(0);
            RoomList roomList = new();
            roomList.Rooms.AddRange(await roomManager.GetListAsync());
            return roomList;
        }
        async public override Task<RoomList> GetJoinedRoomList(Empty request, ServerCallContext context)
        {
            var player = await GetPlayer(context);
            if (player is null)
            {
                return s_emptyRoomList;
            }
            RoomList roomList = new ();
            roomList.Rooms.AddRange(await player.GetJoinedRoomListAsync());
            return roomList; 
        }
    }
}
