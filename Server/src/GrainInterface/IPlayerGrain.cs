using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithStringKey
{
    ValueTask EndOfAsyncStreamAsync();
    ValueTask<Guid> SetStreamAsync(Guid guid);

    ValueTask<int> AddPointAsync(int point);

    ValueTask<bool> ChatAsync(string room, string message);

    ValueTask<(bool ret, List<string> players)> JoinAsync(string room);
    ValueTask<bool> LeaveAsync(string room);

    ValueTask<game.PlayerData> GetPlayerDataAsync();

    ValueTask<ImmutableList<game.Room>> GetJoinedRoomListAsync();

}
