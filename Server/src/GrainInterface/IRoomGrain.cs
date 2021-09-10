using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IRoomGrain : IGrainWithStringKey
{
    ValueTask<(bool success, List<string> players, Guid streamGuid)> JoinAsync(string player, string name);
    ValueTask LeaveAsync(string player);
    ValueTask<bool> ChatAsync(string player, string message);

}
