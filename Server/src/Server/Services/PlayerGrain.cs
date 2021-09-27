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
    public static string GetRegionQueueStreamNamespace(long regionIndex) => $"region-azurequeueprovider-{regionIndex}";
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
        return ValueTask.FromResult<PlayerData>( new() { Name = this.GrainReference.GrainIdentity.PrimaryKeyString, Stage = _state.State.Stage });
    }

    async ValueTask<int> IPlayerGrain.UpdateStageAsync(int stage)
    {
        if(stage > _state.State.Stage)
        {
            long regionIndex = this.GrainReference.GrainIdentity.GetPrimaryKeyLong(out var name);
            _state.State.Stage = stage;
            await _state.WriteStateAsync();
            var stageRankingGrain = this.GrainFactory.GetGrain<IStageUpdateRankingGrain>(regionIndex);
            await stageRankingGrain.UpdateStageAsync(name, stage);
        }
        return _state.State.Stage;
    }
}
