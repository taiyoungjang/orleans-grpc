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

[StorageProvider(ProviderName = "auth")]
public class AuthGrain : Orleans.Grain, IAuthGrain
{
    private readonly ILogger<AuthGrain> _logger;
    private readonly IPersistentState<AuthData> _state;
    public AuthGrain(
        [PersistentState("auth", storageName: "auth")] IPersistentState<AuthData> state,
        ILogger<AuthGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    async ValueTask<AuthData> IAuthGrain.GetAuthDataAsync()
    {
        if(string.IsNullOrEmpty(this.GrainReference.GrainIdentity.PrimaryKeyString))
        {
            return default;
        }
        Guid accountGuid = _state.State.AccountGuid;
        if(accountGuid.Equals(System.Guid.Empty))
        {
            _state.State = new() { FirebaseId = this.GrainReference.GrainIdentity.PrimaryKeyString, AccountGuid = System.Guid.NewGuid() };
            await this._state.WriteStateAsync();
        }
        return _state.State;
    }

    async ValueTask<AuthData> IAuthGrain.SetNewAccountGuid()
    {
        if (!this._state.State.AccountGuid.Equals(System.Guid.Empty))
        {
            return default;
        }
        Guid accountGuid = _state.State.AccountGuid;
        _state.State = new() { FirebaseId = this.GrainReference.GrainIdentity.PrimaryKeyString, AccountGuid = System.Guid.NewGuid() };
        await this._state.WriteStateAsync();
        return _state.State;
    }
}
