using Grpc.Core;
using game;
using Orleans.Streams;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;
using Google.Protobuf.WellKnownTypes;
using Orleans;

[StorageProvider(ProviderName = "stagerank")]
public class StageRankingGrain : Orleans.Grain, IStageRankingGrain, IRemindable
{
    private readonly ILogger<StageRankingGrain> _logger;

    private readonly IPersistentState<RankList> _state;

    public StageRankingGrain(
        [PersistentState("stagerank", storageName: "stagerank")] IPersistentState<RankList> state,
        ILogger<StageRankingGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync()
    {
        return base.OnActivateAsync();
    }

    public ValueTask<ImmutableList<RankData>> GetTopRanks()
    {
        return ValueTask.FromResult(_state.State.Ranks.ToImmutableList());
    }

    async public ValueTask CheckAsync()
    {
        await this.RegisterOrUpdateReminder("stagerank", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    async public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        var regionIndex = this.GrainReference.GetPrimaryKeyLong(out var keyExt);
        var updateGrain = this.GrainFactory.GetGrain<IStageUpdateRankingGrain>(regionIndex);
        var updateRanksResult = await updateGrain.GetAllDataAsync();
        var updateRanks = updateRanksResult
            .OrderByDescending(t => t.Stage)
            .ThenByDescending(t => t.UpdateDate.ToDateTime());
        int nowRank = 1;
        foreach(var rank in updateRanks)
        {
            rank.Rank = nowRank++;
        }
        this._state.State.Ranks.Clear();
        this._state.State.Ranks.AddRange(updateRanks);
        await this._state.WriteStateAsync();
    }
}
