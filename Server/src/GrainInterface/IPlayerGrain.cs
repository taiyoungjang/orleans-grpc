using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithGuidKey
{
    ValueTask EndofAsyncStream();
    ValueTask SetNameAsync(string name);

    ValueTask<bool> ChatFromClient(string room, string message);
    ValueTask OnChat(string player, string room, string message);

    ValueTask<(bool ret, List<string> players)> JoinFromClient(string room);
    ValueTask OnJoin(string player, string room);
    ValueTask OnLeave(string player, string room);

    ValueTask<ImmutableList<game.Room>> GetJoinedRoomList();

}
