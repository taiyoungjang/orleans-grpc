using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithStringKey
{
    ValueTask EndOfAsyncStream();
    ValueTask<Guid> SetStreamAsync();

    ValueTask<int> AddPoint(int point);

    ValueTask<bool> ChatFromClient(string room, string message);
    ValueTask OnChat(string player, string room, string message);

    ValueTask<(bool ret, List<string> players)> JoinFromClient(string room);
    ValueTask OnJoin(string player, string room);
    ValueTask OnLeave(string player, string room);

    ValueTask<game.PlayerData> GetPlayerData();

    ValueTask<ImmutableList<game.Room>> GetJoinedRoomList();

}
