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
        [PersistentState("player", storageName: "player")] IPersistentState<PlayerData> state,
        ILogger<PlayerGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    ValueTask<PlayerData> IPlayerGrain.GetPlayerDataAsync()
    {
        return ValueTask.FromResult<PlayerData>( new() { Name = this._state.State.Name, Stage = _state.State.Stage });
    }

    async ValueTask<game.ErrorCode> IPlayerGrain.UpdateStageAsync(int stage)
    {
        if(string.IsNullOrEmpty(_state.State.Name))
        {
            return ErrorCode.Failure;
        }
        if(stage > _state.State.Stage)
        {
            _state.State.Stage = stage;
            await _state.WriteStateAsync();
            var stageRankingGrain = this.GrainFactory.GetGrain<IStageUpdateRankingGrain>(_state.State.RegionIndex);
            await stageRankingGrain.UpdateStageAsync(_state.State.Name, stage);
            return ErrorCode.Success;
        }
        return ErrorCode.Failure;
    }

    async ValueTask<game.ErrorCode> IPlayerGrain.CreatePlayerAsync(long regionIndex, string name)
    {
        if(!string.IsNullOrEmpty(_state.State.Name) || _state.State.RegionIndex != default)
        {
            return game.ErrorCode.AlreadyDefinedName;
        }
        // TODO : 중복 체크
        // TODO : 금지어 체크
        _state.State.RegionIndex = regionIndex;
        _state.State.Name = name;
        await _state.WriteStateAsync();
        return game.ErrorCode.Success;
    }
}
