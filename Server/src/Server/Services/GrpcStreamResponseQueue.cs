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
    private readonly Grpc.Core.IServerStreamWriter<game.GrpcStreamResponse> _streamWriter;
    private readonly Task _consumer;
    public Task ConsumerTask => _consumer;
    private readonly Guid _guid;
    private Orleans.Streams.StreamSubscriptionHandle<game.GrpcStreamResponse> _orleansStreamHandle;
    private System.Func<Task> _disconnectAction;

    private readonly Channel<game.GrpcStreamResponse> _channel = 
        Channel.CreateUnbounded<game.GrpcStreamResponse>(
            new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true,
            });
    public GrpcStreamResponseQueue(
        Guid guid,
        IServerStreamWriter<game.GrpcStreamResponse> stream,
        System.Func<Task> disconnectAction,
        CancellationToken cancellationToken = default
    )
    {
        _guid = guid;
        _streamWriter = stream;
        _disconnectAction = disconnectAction;
        _consumer = ConsumeTask(cancellationToken);
    }
    public void SetHandle(Orleans.Streams.StreamSubscriptionHandle<game.GrpcStreamResponse> handle)
    {
        if(_consumer.IsCompleted)
        {
            handle.UnsubscribeAsync();
            return;
        }
        _orleansStreamHandle = handle;
    }

    /// <summary>
    /// Asynchronously writes an item to the channel.
    /// </summary>
    /// <param name="message">The value to write to the channel.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the write operation.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.ValueTask" /> that represents the asynchronous write operation.</returns>
    public async ValueTask WriteAsync(game.GrpcStreamResponse message, CancellationToken cancellationToken = default)
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
                await _streamWriter.WriteAsync(message);
            }
        }
        catch(System.Exception)
        {
        }
        try
        {
            _disconnectAction?.Invoke();
        }
        catch (Exception)
        {

        }
        try
        {
            await _orleansStreamHandle?.UnsubscribeAsync();
        }
        catch (Exception)
        {
        }
    }
}
