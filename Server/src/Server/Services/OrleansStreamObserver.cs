using System;
using System.Threading.Tasks;
using Orleans.Streams;
using game;

namespace Server
{
    public class OrleansStreamObserver : Orleans.Streams.IAsyncObserver<GrpcStreamResponse>
    {
        private readonly Guid _key;
        private GrpcStreamResponseQueue _grpcStreamResponseQueue;
        private readonly Orleans.Streams.IAsyncStream<GrpcStreamResponse> _asyncStream;
        private System.Threading.CancellationToken _cancellationToken;
        public OrleansStreamObserver(
            Guid key,
            Grpc.Core.IServerStreamWriter<GrpcStreamResponse> serverStream,
            Orleans.Streams.IAsyncStream<GrpcStreamResponse> asyncStream, 
            System.Func<Task> disconnectAction,
            System.Threading.CancellationToken cancellationToken)
        {
            _key = key;
            _grpcStreamResponseQueue = new(key, serverStream, disconnectAction, cancellationToken);
            _asyncStream = asyncStream;
            _cancellationToken = cancellationToken;
        }

        public Task WaitConsumerTask()
        {
            _asyncStream.SubscribeAsync(this)
                .ContinueWith( t =>
                {
                    if (t.Exception == null)
                    {
                        _grpcStreamResponseQueue.SetHandle(t.Result);
                    }
                });
            return _grpcStreamResponseQueue.ConsumerTask;
        }

        public Task OnCompletedAsync() => Task.CompletedTask;

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task OnNextAsync(GrpcStreamResponse item, StreamSequenceToken token = null)
        {
            return _grpcStreamResponseQueue.WriteAsync(item, _cancellationToken).AsTask();
        }
    }
}
