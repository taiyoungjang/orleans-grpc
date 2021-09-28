using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Orleans;
public interface IPlayerGrain : IGrainWithGuidKey
{
    ValueTask<game.ErrorCode> UpdateStageAsync(long stage);

    ValueTask<game.PlayerData> GetPlayerDataAsync();

    ValueTask<game.MailListData> GetMailsAsync();
    ValueTask<game.ErrorCode> AddMailAsync(game.MailData mailData);
    ValueTask<game.ErrorCode> DeleteMailAsync(game.UUID guid);
    ValueTask<game.ErrorCode> CreatePlayerAsync(long regionIndex, string name);
}
