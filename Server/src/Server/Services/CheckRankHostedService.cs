using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;

public class CheckRankHostedService : BackgroundService
{
    private readonly ILogger<CheckRankHostedService> _logger;
    private readonly IClusterClient _client;

    public CheckRankHostedService(ILogger<CheckRankHostedService> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach(var reginIndex in Server.Program.s_regionList)
        {
            var rankingGrain = _client.GetGrain<IStageRankingGrain>(reginIndex);
            await rankingGrain.CheckAsync();
        }
    }
}
