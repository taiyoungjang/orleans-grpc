using Grpc.Core;
using game;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;

public class PlayerGrain : Orleans.Grain, IPlayerGrain
{
    private readonly ILogger<PlayerGrain> _logger;
    public static string RegionQueueStreamNamespace => $"region-azurequeueprovider";
    public static Guid GetRegionGuid(long regionIndex) => Guid.Parse(string.Format("00000000-0000-0000-0000-00{0:0000000000}", regionIndex));
    private readonly IPersistentState<PlayerData> _playerState;
    private readonly IPersistentState<MailListData> _mailState;
    public PlayerGrain(
        [PersistentState("player", storageName: "playerstore")] IPersistentState<PlayerData> playerState,
        [PersistentState("mail", storageName: "mailstore")] IPersistentState<MailListData> mailState,
        ILogger<PlayerGrain> logger)
    {
        _playerState = playerState;
        _mailState = mailState;
        _logger = logger;
    }
    async public override Task OnActivateAsync()
    {
        await base.OnActivateAsync();
    }
    public override Task OnDeactivateAsync()
    {
        return base.OnDeactivateAsync();
    }

    ValueTask<PlayerData> IPlayerGrain.GetPlayerDataAsync()
    {
        return ValueTask.FromResult<PlayerData>( 
            new() { 
                RegionIndex = this._playerState.State.RegionIndex, 
                Name = this._playerState.State.Name, 
                Stage = _playerState.State.Stage 
            });
    }

    async ValueTask<game.ErrorCode> IPlayerGrain.UpdateStageAsync(long stage)
    {
        if(string.IsNullOrEmpty(_playerState.State.Name))
        {
            return ErrorCode.Failure;
        }
        if(stage > _playerState.State.Stage)
        {
            _playerState.State.Stage = stage;
            await _playerState.WriteStateAsync();
            var stageRankingGrain = this.GrainFactory.GetGrain<IStageUpdateRankingGrain>(_playerState.State.RegionIndex);
            var playerGuid = this.GrainReference.GrainIdentity.PrimaryKey;
            await stageRankingGrain.UpdateAsync(_playerState.State.Name, playerGuid, stage);
            return ErrorCode.Success;
        }
        return ErrorCode.Failure;
    }

    async ValueTask<game.ErrorCode> IPlayerGrain.CreatePlayerAsync(long regionIndex, string name)
    {
        if(!string.IsNullOrEmpty(_playerState.State.Name) || _playerState.State.RegionIndex != default)
        {
            return game.ErrorCode.AlreadyDefinedName;
        }
        // TODO : 금지어 체크
        _playerState.State.RegionIndex = regionIndex;
        _playerState.State.Name = name;
        await _playerState.WriteStateAsync();
        return game.ErrorCode.Success;
    }

    ValueTask<MailListData> IPlayerGrain.GetMailsAsync()
    {
        return ValueTask.FromResult(_mailState.State);
    }

    async public ValueTask<ErrorCode> AddMailAsync(MailData mailData)
    {
        _mailState.State.Mails.Add( new Guid(mailData.Uuid.Value.Span).ToString(),mailData);
        await _mailState.WriteStateAsync();
        return ErrorCode.Success;
    }

    async public ValueTask<ErrorCode> DeleteMailAsync(UUID guid)
    {
        var key = new Guid(guid.Value.Span).ToString();
        if(_mailState.State.Mails.Remove(key))
        {
            await _mailState.WriteStateAsync();
            return ErrorCode.Success;
        }
        return ErrorCode.Failure;
    }
}
