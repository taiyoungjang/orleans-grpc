using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes;
using game;

namespace Server
{
    public class PlayerGrpcNetworkService : game.PlayerNetwork.PlayerNetworkBase
    {
        private readonly ILogger<PlayerGrpcNetworkService> _logger;
        private readonly Orleans.IClusterClient _clusterClient;
        private static game.RoomList s_emptyRoomList = new();
        public PlayerGrpcNetworkService(Orleans.IClusterClient clusterClient, ILogger<PlayerGrpcNetworkService> logger)
        {
            _clusterClient = clusterClient;
            _logger = logger;
        }
        async public override Task<UUID> GetAuth(AuthRequest request, ServerCallContext context)
        {
            Guid guid;
            try
            {
                var player = _clusterClient.GetGrain<IPlayerGrain>(request.Name);
                guid = await player.SetStreamAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            UUID ret = new() { Value = Google.Protobuf.ByteString.CopyFrom(guid.ToByteArray()) };
            return ret;
        }
        private Guid GetAuthorization(ServerCallContext context)
        {
            var metaData = context.RequestHeaders.Get("uuid-bin");
            if (metaData == null)
            {
                return Guid.Empty;
            }
            return new Guid(metaData.ValueBytes);
        }
        private string GetContextName(ServerCallContext context)
        {
            var metaData = context.RequestHeaders.Get("name");
            if (metaData == null)
            {
                return string.Empty;
            }
            return metaData.Value;
        }
        private bool GetPlayer(ServerCallContext context, out IPlayerGrain outPlayer)
        {
            var name = GetContextName(context);
            if (string.IsNullOrEmpty(name))
            {
                outPlayer = null;
                return false;
            }
            outPlayer = _clusterClient.GetGrain<IPlayerGrain>(name);
            return true;
        }

        public override Task GetAsyncStreams(global::Google.Protobuf.WellKnownTypes.Empty empty, IServerStreamWriter<GrpcStreamResponse> responseStreamWriter, ServerCallContext context)
        {
            Guid guid = GetAuthorization(context);
            if(guid.Equals(Guid.Empty))
            {
                return Task.CompletedTask;
            }
            if (!GetPlayer(context, out var player))
            {
                return Task.CompletedTask;
            }
            async Task EndofAsyncStream()
            {
                await player.EndofAsyncStream();
            }
            var stream = _clusterClient
                .GetStreamProvider(PlayerGrain.s_streamProviderName)
                .GetStream<GrpcStreamResponse>(guid, PlayerGrain.s_streamNamespace);
            var streamObserver = new OrleansStreamObserver(guid, responseStreamWriter, stream, EndofAsyncStream, context.CancellationToken);
            return streamObserver.WaitConsumerTask();
        }

        async public override Task<ChatResponse> Chat(ChatRequest request, ServerCallContext context)
        {
            if(!GetPlayer(context, out var player))
            {
                return new() {Success = false};
            }
            var ret = await player.ChatFromClient(room: request.Room, message: request.Message);
            return new() { Success = ret };
        }
        async public override Task<JoinResponse> Join(JoinRequest request, ServerCallContext context)
        {
            if (!GetPlayer(context, out var player))
            {
                return new() { Success = false };
            }
            var joinRet = await player.JoinFromClient(room: request.Room);
            JoinResponse ret = new() { Success = joinRet.ret};
            ret.Players.AddRange(joinRet.players);
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
            if (!GetPlayer(context, out var player))
            {
                return s_emptyRoomList;
            }
            RoomList roomList = new ();
            roomList.Rooms.AddRange(await player.GetJoinedRoomList());
            return roomList; 
        }
    }
}
