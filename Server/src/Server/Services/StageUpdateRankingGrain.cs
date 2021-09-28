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

public class StageUpdateRankingGrain : Orleans.Grain, IStageUpdateRankingGrain
{
    private readonly ILogger<StageUpdateRankingGrain> _logger;

    private readonly IPersistentState<Dictionary<string,RankData>> _state;
    private RankType _rankType = RankType.Stage;
    public StageUpdateRankingGrain(
        [PersistentState("stageupdaterank", storageName: "stageupdaterankstore")] IPersistentState<Dictionary<string, RankData>> state,
        ILogger<StageUpdateRankingGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public ValueTask<ImmutableList<RankData>> GetAllDataAsync()
    {
        return ValueTask.FromResult(_state.State.Values.Select( t => t.Clone() ).ToImmutableList());
    }

    public override Task OnActivateAsync()
    {
        return base.OnActivateAsync();
    }

    async ValueTask<game.ErrorCode> IStageUpdateRankingGrain.UpdateAsync(string name, Guid playerGuid, long value)
    {
        if(string.IsNullOrEmpty(name))
        {
            return game.ErrorCode.Failure;
        }
        if(!_state.State.TryGetValue(name, out RankData rank))
        {
            rank = new RankData() 
            {
                RankType = _rankType,
                Name = name, Value = value, 
                Rank = long.MaxValue, 
                UpdateDate = Timestamp.FromDateTime(System.DateTime.UtcNow) ,
                PlayerGuid = playerGuid.ToString()
            };
            _state.State.Add(name,rank);
        }
        else
        {
            rank.Value = value;
            rank.UpdateDate = Timestamp.FromDateTime(System.DateTime.UtcNow);
        }
        await _state.WriteStateAsync();
        return ErrorCode.Success;
    }

}
