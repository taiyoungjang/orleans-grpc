using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IUniqueNameGrain : IGrainWithIntegerKey
{
    ValueTask<Guid> GetPlayerNameAsync(string name);
    ValueTask<game.ErrorCode> SetPlayerNameAsync(string name, Guid playerGuid);
}

