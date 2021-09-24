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
    public static string GetPlayerQueueStreamNamespace(long regionIndex) => $"playergrain-azurequeueprovider-{regionIndex}";
    public static string GetChatRoomQueueStreamNamespace(long regionIndex) => $"chatroomgrain-azurequeueprovider-{regionIndex}";

    private Orleans.Streams.IAsyncStream<game.StreamServerEventsResponse> _stream;
    private StreamSubscriptionHandle<game.StreamServerEventsResponse> _chatRoomSubscriptionHandle;
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
        var streamProvider = GetStreamProvider(Server.Program.s_streamProviderName);
        var regionIndex = this.GrainReference.GrainIdentity.GetPrimaryKeyLong(out var ext);
        _stream = streamProvider.GetStream<game.StreamServerEventsResponse>(guid, GetPlayerQueueStreamNamespace(regionIndex));
        return guid;
    }
    async ValueTask IPlayerGrain.EndOfAsyncStreamAsync()
    {
        if( _chatRoomSubscriptionHandle != null)
        {
            try
            {
                await _chatRoomSubscriptionHandle.UnsubscribeAsync();
                _chatRoomSubscriptionHandle = null;
            }
            catch (Exception)
            {
            }
        }
        _stream = null;
    }

    async ValueTask<bool> IPlayerGrain.ChatAsync(string message)
    {
        var regionIndex = this.GrainReference.GrainIdentity.GetPrimaryKeyLong(out var _);
        var roomStream = this.GetStreamProvider(Server.Program.s_streamProviderName)
            .GetStream<StreamServerEventsResponse>(Guid.Empty, GetChatRoomQueueStreamNamespace(regionIndex));
        StreamServerEventsResponse grpcStreamResponse = new()
        {
            OnChat = new()
            {
                RoomInfo = $"chatroom{regionIndex}",
                OtherPlayer = _state.State.Name,
                Message = message
            }
        };
        await roomStream.OnNextAsync(grpcStreamResponse);
        return true;
    }

    async ValueTask<bool> IPlayerGrain.JoinChatRoomAsync()
    {
        var regionIndex = this.GrainReference.GrainIdentity.GetPrimaryKeyLong(out var _);
        var roomStream = this.GetStreamProvider(Server.Program.s_streamProviderName)
            .GetStream<StreamServerEventsResponse>(Guid.Empty, GetChatRoomQueueStreamNamespace(regionIndex));
        var handle = await roomStream.SubscribeAsync(new RoomStreamObserver(_stream));
        _chatRoomSubscriptionHandle = handle;
        return true;
    }
    async ValueTask<bool> IPlayerGrain.LeaveChatRoomAsync()
    {
        if(_chatRoomSubscriptionHandle != null)
        {
            await _chatRoomSubscriptionHandle.UnsubscribeAsync();
            _chatRoomSubscriptionHandle = null;
        }
        return true;
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
