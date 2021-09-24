using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using game;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

public class RoomGrain : Orleans.Grain, IRoomGrain
{
    public static string s_streamNamespace = $"{nameof(RoomGrain)}-azurequeueprovider-0".ToLower();
    private readonly ILogger<RoomGrain> _logger;
    private List<PlayerInfo> _playerInfos;
    private IAsyncStream<game.StreamServerEventsResponse> _stream;
    public RoomGrain(ILogger<RoomGrain> logger)
    {
        _logger = logger;
        _playerInfos = new();
    }
    async public override Task OnActivateAsync()
    {
        var roomManager = this.GrainFactory.GetGrain<IRoomManagerGrain>(0);
        await roomManager.CreateAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
        var streamProvider = GetStreamProvider(Server.Program.s_streamProviderName);
        _stream = streamProvider.GetStream<game.StreamServerEventsResponse>(Guid.NewGuid(), s_streamNamespace);
        await base.OnActivateAsync();
    }
    async public override Task OnDeactivateAsync()
    {
        var roomManager = this.GrainFactory.GetGrain<IRoomManagerGrain>(0);
        await roomManager.DestroyAsync(this.GrainReference.GrainIdentity.PrimaryKeyString);
        await _stream.OnCompletedAsync();
        await base.OnDeactivateAsync();
    }
    async ValueTask<bool> IRoomGrain.ChatAsync(string playerName, string message)
    {
        string roomName = this.GrainReference.GrainIdentity.PrimaryKeyString;
        StreamServerEventsResponse grpcStreamResponse = new()
        {
            OnChat = new()
            {
                RoomInfo = roomName,
                OtherPlayer = playerName,
                Message = message
            }
        };
        await _stream.OnNextAsync(grpcStreamResponse);
        return true;
    }

    async ValueTask IRoomGrain.LeaveAsync(string player)
    {
        string roomName = this.GrainReference.GrainIdentity.PrimaryKeyString;
        var leaver = _playerInfos.FirstOrDefault(t => t.Name == player);
        if (leaver != null)
        {
            _playerInfos.RemoveAll(t => t.Name == player);
            StreamServerEventsResponse grpcStreamResponse = new()
            {
                OnLeave = new()
                {
                    OtherPlayer = player,
                    RoomInfo = roomName
                }
            };
            await _stream.OnNextAsync(grpcStreamResponse);
        }
    }

    async ValueTask<(bool success, List<string> players, Guid streamGuid)> IRoomGrain.JoinAsync(string player, string name)
    {
        if (_playerInfos.Exists(t => t.Name == player))
        {
            return (false, null, Guid.Empty);
        }

        string roomName = this.GrainReference.GrainIdentity.PrimaryKeyString;

        List<string> players = _playerInfos.Select(t => t.Name).ToList();
        _playerInfos.Add(new(name));

        StreamServerEventsResponse grpcStreamResponse = new()
        {
            OnJoin = new()
            {
                OtherPlayer = player,
                RoomInfo = roomName
            }
        };
        await _stream.OnNextAsync(grpcStreamResponse);
        return new(true, players, _stream.Guid);
    }
}

