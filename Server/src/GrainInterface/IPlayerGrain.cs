using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithIntegerCompoundKey
{
    ValueTask EndOfAsyncStreamAsync();
    ValueTask<Guid> SetStreamAsync(Guid guid);

    ValueTask<int> AddPointAsync(int point);

    ValueTask<bool> ChatAsync(string message);

    ValueTask<bool> JoinChatRoomAsync();
    ValueTask<bool> LeaveChatRoomAsync();

    ValueTask<game.PlayerData> GetPlayerDataAsync();
}
