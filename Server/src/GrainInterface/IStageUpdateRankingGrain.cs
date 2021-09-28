using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IStageUpdateRankingGrain : IGrainWithIntegerKey
{
    ValueTask<game.ErrorCode> UpdateAsync(string name, Guid playerGuid, long value);

    ValueTask<ImmutableList<game.RankData>> GetAllDataAsync();
}
