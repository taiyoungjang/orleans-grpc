using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IStageUpdateRankingGrain : IGrainWithIntegerKey
{
    ValueTask<bool> UpdateStageAsync(string name, int stage);

    ValueTask<ImmutableList<game.RankData>> GetAllDataAsync();
}
