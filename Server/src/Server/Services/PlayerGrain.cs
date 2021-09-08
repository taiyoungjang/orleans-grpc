using Grpc.Core;
using game;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;

[StorageProvider(ProviderName = "player")]
public class PlayerGrain : Orleans.Grain, IPlayerGrain
{
    private readonly ILogger<PlayerGrain> _logger;
    public static string s_streamProviderName = "playergrain";
    public static string s_streamNamespace = "default";
    private Orleans.Streams.IAsyncStream<game.GrpcStreamResponse> _streamToGrpc;

    private List<Room> _joinedRoomList;
    private readonly IPersistentState<PlayerData> _state;
    public PlayerGrain(
        [PersistentState("player",storageName:"player")] IPersistentState<PlayerData> state,
        ILogger<PlayerGrain> logger)
    {
        _state = state;
        _logger = logger;
    }
    async public override Task OnActivateAsync()
    {
        _joinedRoomList = new();
        await base.OnActivateAsync();
    }
    public override Task OnDeactivateAsync()
    {
        return base.OnDeactivateAsync();
    }
    async ValueTask IPlayerGrain.EndofAsyncStream()
    {
        for (int i = 0; i < _joinedRoomList.Count; ++i)
        {
            var room = GrainFactory.GetGrain<IRoomGrain>(_joinedRoomList[i].Name);
            await room.LeaveAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
        }
        _joinedRoomList.Clear();
    }
    async ValueTask<Guid> IPlayerGrain.SetStreamAsync()
    {
        if(string.IsNullOrEmpty(_state.State.Name))
        {
            _state.State.Name = this.GrainReference.GrainIdentity.PrimaryKeyString;
            await _state.WriteStateAsync();
        }
        Guid guid = Guid.Parse(_state.Etag);
        var streamProvider = GetStreamProvider(s_streamProviderName);
        _streamToGrpc = streamProvider.GetStream<game.GrpcStreamResponse>(guid, s_streamNamespace);
        return guid;
    }

    async ValueTask<bool> IPlayerGrain.ChatFromClient(string room, string message)
    {
        var roomGrain = GrainFactory.GetGrain<IRoomGrain>(room);
        return await roomGrain.ChatAsync(this.GrainReference.GrainIdentity.PrimaryKeyString, message);
    }
    async ValueTask IPlayerGrain.OnChat(string player, string room, string message)
    {
        GrpcStreamResponse grpcStreamResponse = new()
        {
            OnChat = new()
            {
                RoomInfo = room,
                OtherPlayer = player,
                Message = message
            }
        };
        await _streamToGrpc.OnNextAsync(grpcStreamResponse);
    }

    async ValueTask<(bool ret, List<string> players)> IPlayerGrain.JoinFromClient(string room)
    {
        var roomGrain = GrainFactory.GetGrain<IRoomGrain>(room);
        var joinRet = await roomGrain.JoinAsync(this.GrainReference.GrainIdentity.PrimaryKeyString, _state.State.Name);
        if(joinRet.ret)
        {
            _joinedRoomList.Add(new() { Name = room });
            return (true, joinRet.players);
        }
        return (false,null);
    }

    async ValueTask IPlayerGrain.OnJoin(string player, string room)
    {
        GrpcStreamResponse grpcStreamResponse = new()
        {
            OnJoin = new()
            {
                OtherPlayer = player,
                RoomInfo = room
            }
        };
        await _streamToGrpc.OnNextAsync(grpcStreamResponse);
    }

    ValueTask<ImmutableList<game.Room>> IPlayerGrain.GetJoinedRoomList()
    {
        return ValueTask.FromResult(_joinedRoomList.ToImmutableList());
    }

    async ValueTask IPlayerGrain.OnLeave(string player, string room)
    {
        GrpcStreamResponse grpcStreamResponse = new()
        {
            OnLeave = new()
            {
                OtherPlayer = player,
                RoomInfo = room
            }
        };
        await _streamToGrpc.OnNextAsync(grpcStreamResponse);
    }
}
