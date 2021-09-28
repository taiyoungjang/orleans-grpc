using System;
using System.Collections.Generic;


public class PlayerSummaryData
{
    public Guid AccountGuid { get; set; }
    public Dictionary<long /*regionIndex*/, PlayerSummary> Players { get; set; } = new();
}
