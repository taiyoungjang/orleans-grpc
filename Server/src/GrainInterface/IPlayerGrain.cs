using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithIntegerCompoundKey
{
    ValueTask<int> UpdateStageAsync(int stage);

    ValueTask<game.PlayerData> GetPlayerDataAsync();
}
