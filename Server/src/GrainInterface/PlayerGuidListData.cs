using System;
using System.Collections.Generic;


public class PlayerGuidListData
{
    public Guid AccountGuid { get; set; }
    public Dictionary<long /*regionIndex*/, PlayerSummary> Players { get; set; } = new();
}
