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

public class UniqueNameGrain : Orleans.Grain, IUniqueNameGrain
{
    private readonly ILogger<UniqueNameGrain> _logger;
    private readonly IPersistentState<Dictionary<string, Guid>> _state;
    public UniqueNameGrain(
        [PersistentState("uniquename", storageName: "uniquenamestore")] IPersistentState<Dictionary<string, Guid>> state,
        ILogger<UniqueNameGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    ValueTask<Guid> IUniqueNameGrain.GetPlayerNameAsync(string name)
    {
        Guid playerGuid = Guid.Empty;
        if(_state.State.TryGetValue(name, out playerGuid))
        {

        }
        return ValueTask.FromResult(playerGuid);
    }

    async ValueTask<ErrorCode> IUniqueNameGrain.SetPlayerNameAsync(string name, Guid playerGuid)
    {
        if(_state.State.TryAdd(name,playerGuid))
        {
            await _state.WriteStateAsync();
            return ErrorCode.Success;
        }
        return ErrorCode.AlreadyDefinedName;
    }
}
