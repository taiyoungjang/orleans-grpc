using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IRankingGrain : IGrainWithIntegerKey
{
    ValueTask<List<game.RankData>> GetTopRanks();

}
