using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IRoomGrain : IGrainWithStringKey
{
    ValueTask LeaveAsync(string player);
    ValueTask<bool> ChatAsync(string player, string message);
    ValueTask<(bool ret, List<string> players)> JoinAsync(string player, string name);

}
