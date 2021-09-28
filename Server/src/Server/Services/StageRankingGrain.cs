using Grpc.Core;
using game;
using Orleans.Streams;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;
using Google.Protobuf.WellKnownTypes;
using Orleans;

public class StageRankingGrain : Orleans.Grain, IStageRankingGrain, IRemindable
{
    private readonly ILogger<StageRankingGrain> _logger;

    private readonly IPersistentState<StageRankInfo> _state;

    private readonly int _topRankCount = 100;
    private readonly RankType _rankType = RankType.Stage;
    private Guid _regionGuid;
    public StageRankingGrain(
        [PersistentState("stagerank", storageName: "stagerankstore")] IPersistentState<StageRankInfo> state,
        ILogger<StageRankingGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync()
    {
        var regionIndex = this.GrainReference.GetPrimaryKeyLong(out var keyExt);
        _regionGuid = Guid.Parse(string.Format("00000000-0000-0000-0000-00{0:0000000000}", regionIndex));
        return base.OnActivateAsync();
    }

    public ValueTask<(RanksMap topRanks,RankData myRank)> GetTopRanks(string withPlayerName)
    {
        if(!this._state.State.AllMembers.Ranks.TryGetValue(withPlayerName, out var myRank))
        {
            myRank = new RankData() { Rank = long.MaxValue};
        }
        return ValueTask.FromResult( (this._state.State.TopRanks, myRank));
    }

    async public ValueTask CheckAsync()
    {
        await this.RegisterOrUpdateReminder("stagerank", dueTime: TimeSpan.FromMinutes(1), period: TimeSpan.FromMinutes(1));
    }

    async public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        var regionIndex = this.GrainReference.GetPrimaryKeyLong(out var keyExt);
        var updateGrain = this.GrainFactory.GetGrain<IStageUpdateRankingGrain>(regionIndex);
        var updateRanksResult = await updateGrain.GetAllDataAsync();
        var updateRanks = updateRanksResult
            .OrderByDescending(t => t.Value)
            .ThenByDescending(t => t.UpdateDate.ToDateTime());
        int nowRank = 1;
        RanksMap topRanks = new RanksMap();
        RanksMap allMembers = new RanksMap();
        foreach (var rank in updateRanks)
        {
            rank.Rank = nowRank++;
            if(rank.Rank <= _topRankCount)
            {
                topRanks.Ranks.Add(rank.Name, rank);
                {
                    var mailData = new MailData() { Uuid = new UUID()};
                    mailData.Uuid.Value = Google.Protobuf.ByteString.CopyFrom(Guid.NewGuid().ToByteArray());
                    mailData.SendDate = Timestamp.FromDateTime(System.DateTime.UtcNow);
                    var playerGrain = this.GrainFactory.GetGrain<IPlayerGrain>(new Guid(rank.PlayerGuid));
                    await playerGrain.AddMailAsync(mailData);
                }
            }
            allMembers.Ranks.Add(rank.Name, rank);
        }
        this._state.State.TopRanks = topRanks;
        this._state.State.AllMembers = allMembers;
        await this._state.WriteStateAsync();

        {
            var regionStream = this.GetStreamProvider(Server.Program.s_streamProviderName)
            .GetStream<StreamServerEventsResponse>(_regionGuid, PlayerGrain.RegionQueueStreamNamespace);
            StreamServerEventsResponse grpcStreamResponse = new()
            {
                OnUpdateRanking = new()
                {
                    RankType = _rankType
                }
            };
            await regionStream.OnNextAsync(grpcStreamResponse);
        }
    }
}

public class StageRankInfo
{
    public RanksMap TopRanks { get; set; } = new();
    public RanksMap AllMembers { get; set; } = new();
}