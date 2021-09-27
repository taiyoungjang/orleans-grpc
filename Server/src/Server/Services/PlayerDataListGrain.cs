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

[StorageProvider(ProviderName = "playerguidlist")]
public class PlayerDataListGrain : Orleans.Grain, IPlayerDataListGrain
{
    private readonly ILogger<PlayerDataListGrain> _logger;
    private readonly IPersistentState<PlayerGuidListData> _state;
    public PlayerDataListGrain(
        [PersistentState("playerguidlist", storageName: "playerguidlist")] IPersistentState<PlayerGuidListData> state,
        ILogger<PlayerDataListGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    ValueTask<List<PlayerGuid>> IPlayerDataListGrain.GetPlayerGuidListDataAsync()
    {
        var ret = new List<PlayerGuid>();
        foreach(var pair in _state.State.Players)
        {
            ret.Add( new() {RegionIndex = pair.Key, Player = pair.Value.Player, PlayerName = pair.Value.PlayerName });
        }
        return ValueTask.FromResult(ret);
    }

    async ValueTask<ErrorCode> IPlayerDataListGrain.SetPlayerListData(long regionIndex, Guid playerGuid, string playerName)
    {
        _state.State.Players.Add(regionIndex, new PlayerGuid() { RegionIndex = regionIndex, Player = playerGuid,PlayerName = playerName });
        await _state.WriteStateAsync();
        return ErrorCode.Success;
    }
}
