using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerDataListGrain : IGrainWithGuidKey
{
    ValueTask<List<PlayerGuid>> GetPlayerGuidListDataAsync();
    ValueTask<game.ErrorCode> SetPlayerListData(long regionIndex, Guid playerGuid, string playerName);
}

public struct PlayerGuid
{
    public long RegionIndex { get; set; }
    public Guid Player { get; set; }
    public string PlayerName { get; set; }
}