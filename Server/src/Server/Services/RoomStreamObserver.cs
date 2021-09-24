using System;
using System.Threading.Tasks;
using Orleans.Streams;

public class RoomStreamObserver : IAsyncObserver<game.StreamServerEventsResponse>, IDisposable
{
    private readonly Orleans.Streams.IAsyncStream<game.StreamServerEventsResponse> _playerStream;
    public RoomStreamObserver(Orleans.Streams.IAsyncStream<game.StreamServerEventsResponse> playerStream)
    {
        _playerStream = playerStream;
    }
    public void Dispose()
    { 

    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }

    async public Task OnNextAsync(game.StreamServerEventsResponse item, StreamSequenceToken token = null)
    {
        await _playerStream.OnNextAsync(item);
    }
}
