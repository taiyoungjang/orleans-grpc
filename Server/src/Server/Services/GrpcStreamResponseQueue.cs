using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
/// <summary>
/// Wraps <see cref="IServerStreamWriter{T}"/> which only supports one writer at a time.
/// This class can receive messages from multiple threads, and writes them to the stream
/// one at a time.
/// </summary>
/// <typeparam name="T">Type of message written to the stream</typeparam>
public class GrpcStreamResponseQueue
{
    private readonly Grpc.Core.IServerStreamWriter<game.StreamServerEventsResponse> _grpcPub;
    private readonly Task _consumer;
    public Task ConsumerTask => _consumer;
    private readonly Guid _guid;
    private Orleans.Streams.StreamSubscriptionHandle<game.StreamServerEventsResponse> _orleansSubHandle;

    private readonly Channel<game.StreamServerEventsResponse> _channel = 
        Channel.CreateUnbounded<game.StreamServerEventsResponse>(
            new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true,
            });
    public GrpcStreamResponseQueue(
        Guid guid,
        IServerStreamWriter<game.StreamServerEventsResponse> stream,
        CancellationToken cancellationToken = default
    )
    {
        _guid = guid;
        _grpcPub = stream;
        _consumer = ConsumeTask(cancellationToken);
    }
    public void SetHandle(Orleans.Streams.StreamSubscriptionHandle<game.StreamServerEventsResponse> handle)
    {
        if(_consumer.IsCompleted)
        {
            handle.UnsubscribeAsync();
            return;
        }
        _orleansSubHandle = handle;
    }

    /// <summary>
    /// Asynchronously writes an item to the channel.
    /// </summary>
    /// <param name="message">The value to write to the channel.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the write operation.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.ValueTask" /> that represents the asynchronous write operation.</returns>
    public async ValueTask WriteAsync(game.StreamServerEventsResponse message, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    /// <summary>
    /// Marks the writer as completed, and waits for all writes to complete.
    /// </summary>
    public Task CompleteAsync()
    {
        _channel.Writer.Complete();
        return _consumer;
    }

    private async Task ConsumeTask(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                await _grpcPub.WriteAsync(message);
                if(message.ActionCase == game.StreamServerEventsResponse.ActionOneofCase.OnClosed)
                {
                    //twice call
                    //_disconnectAction = null;
                    _ =  CompleteAsync();
                }
            }
        }
        catch(System.Exception)
        {
        }
        try
        {
            await _orleansSubHandle?.UnsubscribeAsync();
        }
        catch (Exception)
        {
        }
    }
}
