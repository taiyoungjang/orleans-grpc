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

[StorageProvider(ProviderName = "stageupdaterank")]
public class StageUpdateRankingGrain : Orleans.Grain, IStageUpdateRankingGrain
{
    private readonly ILogger<StageUpdateRankingGrain> _logger;

    private readonly IPersistentState<RankList> _state;

    public StageUpdateRankingGrain(
        [PersistentState("stageupdaterank", storageName: "stageupdaterank")] IPersistentState<RankList> state,
        ILogger<StageUpdateRankingGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public ValueTask<ImmutableList<RankData>> GetAllDataAsync()
    {
        return ValueTask.FromResult(_state.State.Ranks.Select( t => t.Clone() ).ToImmutableList());
    }

    public override Task OnActivateAsync()
    {
        return base.OnActivateAsync();
    }

    async ValueTask<game.ErrorCode> IStageUpdateRankingGrain.UpdateStageAsync(string name, int stage)
    {
        if(string.IsNullOrEmpty(name))
        {
            return game.ErrorCode.Failure;
        }
        bool bFound = false;
        var ranks = _state.State.Ranks;
        for (int i=0;i< ranks.Count;++i)
        {
            if (ranks[i].Name.Equals(name))
            {
                bFound = true;
                ranks[i].Stage = stage;
                ranks[i].UpdateDate = Timestamp.FromDateTime(System.DateTime.UtcNow);
                break;
            }
        }
        if(!bFound)
        {
            ranks.Add(new RankData() { Name = name, Stage = stage, Rank = long.MaxValue, UpdateDate = Timestamp.FromDateTime(System.DateTime.UtcNow) });
        }
        await _state.WriteStateAsync();
        return ErrorCode.Success;
    }

}
