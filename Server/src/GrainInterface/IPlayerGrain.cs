using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithIntegerCompoundKey
{
    ValueTask<int> AddPointAsync(int point);

    ValueTask<game.PlayerData> GetPlayerDataAsync();
}
