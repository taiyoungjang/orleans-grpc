using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IRoomGrain : IGrainWithStringKey
{
    ValueTask LeaveAsync(Guid playerGuid);
    ValueTask<bool> ChatAsync(Guid playerGuid, string message);
    ValueTask<(bool ret, List<string> players)> JoinAsync(Guid playerGuid, string name);

}
