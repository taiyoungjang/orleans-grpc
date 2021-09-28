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

public class OtpGrain : Orleans.Grain, IOtpGrain
{
    private readonly ILogger<AuthGrain> _logger;
    private readonly IPersistentState<OtpData> _state;
    public OtpGrain(
        [PersistentState("otp", storageName: "otpstore")] IPersistentState<OtpData> state,
        ILogger<AuthGrain> logger)
    {
        _state = state;
        _logger = logger;
    }
    public override Task OnDeactivateAsync()
    {
        return base.OnDeactivateAsync();
    }

    async ValueTask IOtpGrain.ClearAsync()
    {
        await _state.ClearStateAsync();
    }

    ValueTask<OtpData> IOtpGrain.GetOtpAsync()
    {
        Guid accountGuid = _state.State.AccountGuid;
        if (accountGuid.Equals(System.Guid.Empty))
        {
            return ValueTask.FromResult< OtpData>(new());
        }
        return ValueTask.FromResult(_state.State);
    }

    async ValueTask<OtpData> IOtpGrain.SetAccountGuidAsync(Guid accountGuid)
    {
        if(!_state.State.AccountGuid.Equals(System.Guid.Empty))
        {
            return default;
        }
        _state.State.Otp = this.GrainReference.GrainIdentity.PrimaryKey;
        _state.State.AccountGuid= accountGuid;
        await this._state.WriteStateAsync();
        return _state.State;
    }

    async ValueTask<OtpData> IOtpGrain.SetPlayerGuidAsync(long regionIndex, Guid playerGuid, string playerName)
    {
        _state.State.RegionIndex = regionIndex;
        _state.State.PlayerGuid = playerGuid;
        _state.State.PlayerName = playerName;
        await this._state.WriteStateAsync();
        return _state.State;
    }
}
