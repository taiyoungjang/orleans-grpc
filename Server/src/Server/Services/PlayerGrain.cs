using Grpc.Core;
using game;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

public class PlayerGrain : Orleans.Grain, IPlayerGrain
{
    private readonly ILogger<PlayerGrain> _logger;
    public static string s_streamProviderName = "playergrain";
    public static string s_streamNamespace = "default";
    private Orleans.Streams.IAsyncStream<game.GrpcStreamResponse> _streamToGrpc;

    private List<Room> _joinedRoomList;
    private string _name;
    public PlayerGrain(ILogger<PlayerGrain> logger)
    {
        _logger = logger;
    }
    public override Task OnActivateAsync()
    {
        _joinedRoomList = new();
        var streamProvider = GetStreamProvider(s_streamProviderName);
        _name = base.GrainReference.GrainIdentity.PrimaryKey.ToString();
        _streamToGrpc = streamProvider.GetStream<game.GrpcStreamResponse>(base.GrainReference.GrainIdentity.PrimaryKey, s_streamNamespace);
        return base.OnActivateAsync();
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
            await room.LeaveAsync(this.GrainReference.GrainIdentity.PrimaryKey);
        }
        _joinedRoomList.Clear();
    }
    ValueTask IPlayerGrain.SetNameAsync(string name)
    {
        _name = name;
        return ValueTask.CompletedTask;
    }

    async ValueTask<bool> IPlayerGrain.ChatFromClient(string room, string message)
    {
        var roomGrain = GrainFactory.GetGrain<IRoomGrain>(room);
        return await roomGrain.ChatAsync(this.GrainReference.GrainIdentity.PrimaryKey, message);
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
        var joinRet = await roomGrain.JoinAsync(this.GrainReference.GrainIdentity.PrimaryKey, _name);
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
