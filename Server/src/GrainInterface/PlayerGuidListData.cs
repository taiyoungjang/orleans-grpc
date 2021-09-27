using System;
using System.Collections.Generic;


public class PlayerGuidListData
{
    public Guid AccountGuid { get; set; }
    public Dictionary<long /*regionIndex*/, PlayerGuid> Players { get; set; } = new();
}
