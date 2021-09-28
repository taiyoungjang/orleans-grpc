using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IUniqueNameGrain : IGrainWithIntegerKey
{
    ValueTask<game.ErrorCode> SetPlayerName(string name, Guid playerGuid);
}

