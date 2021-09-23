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

    private Orleans.Streams.IAsyncStream<game.StreamServerEventsResponse> _stream;
    private Dictionary<string,StreamSubscriptionHandle<game.StreamServerEventsResponse>> _roomSubscriptions;
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
        _roomSubscriptions = new();
        await base.OnActivateAsync();
    }
    public override Task OnDeactivateAsync()
    {
        return base.OnDeactivateAsync();
    }

    async ValueTask<Guid> IPlayerGrain.SetStreamAsync(Guid guid)
    {
        if(_stream != null && this is IPlayerGrain playerGrain)
        {
            StreamServerEventsResponse grpcStreamResponse = new()
            {
                OnClosed = new()
                {
                    Reason = "Duplicated"
                }
            };
            await _stream.OnNextAsync(grpcStreamResponse);
            await playerGrain.EndOfAsyncStreamAsync();
        }
        if(string.IsNullOrEmpty(_state.State.Name))
        {
            _state.State.Name = this.GrainReference.GrainIdentity.PrimaryKeyString;
            await _state.WriteStateAsync();
        }
        var streamProvider = GetStreamProvider(s_streamProviderName);
        _stream = streamProvider.GetStream<game.StreamServerEventsResponse>(guid, s_streamNamespace);
        return guid;
    }
    async ValueTask IPlayerGrain.EndOfAsyncStreamAsync()
    {
        for (int i = 0; i < _joinedRoomList.Count; ++i)
        {
            var room = GrainFactory.GetGrain<IRoomGrain>(_joinedRoomList[i].Name);
            await room.LeaveAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
        }
        foreach(var pair in _roomSubscriptions)
        {
            try
            {
                await pair.Value.UnsubscribeAsync();
            }
            catch (Exception)
            {

            }
        }
        _joinedRoomList.Clear();
        _roomSubscriptions.Clear();
        _stream = null;
    }

    async ValueTask<bool> IPlayerGrain.ChatAsync(string room, string message)
    {
        var roomGrain = GrainFactory.GetGrain<IRoomGrain>(room);
        return await roomGrain.ChatAsync(this.GrainReference.GrainIdentity.PrimaryKeyString, message);
    }

    async ValueTask<(bool ret, List<string> players)> IPlayerGrain.JoinAsync(string room)
    {
        var roomGrain = GrainFactory.GetGrain<IRoomGrain>(room);
        var joinRet = await roomGrain.JoinAsync(this.GrainReference.GrainIdentity.PrimaryKeyString, _state.State.Name);
        if(joinRet.success)
        {
            var roomStream = this.GetStreamProvider(RoomGrain.s_streamProviderName)
                .GetStream<StreamServerEventsResponse>(joinRet.streamGuid, RoomGrain.s_streamNamespace);

            var handle = await roomStream.SubscribeAsync(new RoomStreamObserver(room, _stream));
            _roomSubscriptions.Add(room, handle);

            _joinedRoomList.Add(new() { Name = room });
            return (true, joinRet.players);
        }
        return (false,null);
    }
    async ValueTask<bool> IPlayerGrain.LeaveAsync(string room)
    {
        var roomGrain = GrainFactory.GetGrain<IRoomGrain>(room);
        await roomGrain.LeaveAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
        {
            _joinedRoomList.RemoveAll(t => t.Name.Equals(room));
            if( _roomSubscriptions.Remove(room, out var observer))
            {
                await observer.UnsubscribeAsync();
            }
        }
        return true;
    }
    ValueTask<ImmutableList<game.Room>> IPlayerGrain.GetJoinedRoomListAsync()
    {
        return ValueTask.FromResult(_joinedRoomList.ToImmutableList());
    }

    ValueTask<PlayerData> IPlayerGrain.GetPlayerDataAsync()
    {
        return ValueTask.FromResult<PlayerData>( new() { Name = this.GrainReference.GrainIdentity.PrimaryKeyString, Point = _state.State.Point });
    }

    async ValueTask<int> IPlayerGrain.AddPointAsync(int point)
    {
        _state.State.Point += point;
        await _state.WriteStateAsync();
        return _state.State.Point;
    }
}
