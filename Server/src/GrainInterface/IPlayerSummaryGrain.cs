using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerSummaryGrain : IGrainWithGuidKey
{
    ValueTask<List<PlayerSummary>> GetPlayerSummaryAsync();
    ValueTask<game.ErrorCode> SetPlayerSummaryData(long regionIndex, Guid playerGuid, string playerName);
}

public struct PlayerSummary
{
    public long RegionIndex { get; set; }
    public Guid Player { get; set; }
    public string PlayerName { get; set; }
}