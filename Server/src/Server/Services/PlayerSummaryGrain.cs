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

public class PlayerSummaryGrain : Orleans.Grain, IPlayerSummaryGrain
{
    private readonly ILogger<PlayerSummaryGrain> _logger;
    private readonly IPersistentState<PlayerSummaryData> _state;
    public PlayerSummaryGrain(
        [PersistentState("playersummary", storageName: "playersummarystore")] IPersistentState<PlayerSummaryData> state,
        ILogger<PlayerSummaryGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    ValueTask<List<PlayerSummary>> IPlayerSummaryGrain.GetPlayerSummaryAsync()
    {
        var ret = new List<PlayerSummary>();
        foreach(var pair in _state.State.Players)
        {
            ret.Add( new() {RegionIndex = pair.Key, Player = pair.Value.Player, PlayerName = pair.Value.PlayerName });
        }
        return ValueTask.FromResult(ret);
    }

    async ValueTask<ErrorCode> IPlayerSummaryGrain.SetPlayerSummaryData(long regionIndex, Guid playerGuid, string playerName)
    {
        _state.State.Players.Add(regionIndex, new PlayerSummary() { RegionIndex = regionIndex, Player = playerGuid,PlayerName = playerName });
        await _state.WriteStateAsync();
        return ErrorCode.Success;
    }
}
