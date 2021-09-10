using System;
using System.Threading.Tasks;
using Orleans.Streams;

public class RoomStreamObserver : IAsyncObserver<game.GrpcStreamResponse>
{
    private readonly string _roomName;
    private PlayerGrain _playerGrain;
    public RoomStreamObserver(string roomName, PlayerGrain playerGrain)
    {
        _roomName = roomName;
        _playerGrain = playerGrain;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    public Task OnNextAsync(game.GrpcStreamResponse item, StreamSequenceToken token = null)
    {
        return _playerGrain.OnObserveItemAsync(item, token);
    }
}
