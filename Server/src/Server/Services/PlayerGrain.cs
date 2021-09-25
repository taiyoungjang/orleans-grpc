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
    public static string GetChatRoomQueueStreamNamespace(long regionIndex) => $"chatroomgrain-azurequeueprovider-{regionIndex}";
    private readonly IPersistentState<PlayerData> _state;
    public PlayerGrain(
        [PersistentState("player",storageName:"player")] IPersistentState<PlayerData> state,
        ILogger<PlayerGrain> logger)
    {
        _state = state;
        _logger = logger;
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
