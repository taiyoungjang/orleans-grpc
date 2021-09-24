using System;
using System.Threading.Tasks;
using Orleans.Streams;
using game;

namespace Server
{
    public class OrleansStreamObserver : Orleans.Streams.IAsyncObserver<StreamServerEventsResponse>
    {
        private readonly Guid _key;
        private GrpcStreamResponseQueue _grpcPub;
        private readonly Orleans.Streams.IAsyncStream<StreamServerEventsResponse> _pub;
        private System.Threading.CancellationToken _cancellationToken;
        public OrleansStreamObserver(
            Guid key,
            Grpc.Core.IServerStreamWriter<StreamServerEventsResponse> grpcPub,
            Orleans.Streams.IAsyncStream<StreamServerEventsResponse> orleansPub, 
            System.Func<Task> disconnectAction,
            System.Threading.CancellationToken cancellationToken)
        {
            _key = key;
            _grpcPub = new(key, grpcPub, disconnectAction, cancellationToken);
            _pub = orleansPub;
            _cancellationToken = cancellationToken;
        }

        async public Task WaitConsumerTask()
        {
            try
            {
                var subscribeHandle = await _pub.SubscribeAsync(this);
                _grpcPub.SetHandle(subscribeHandle);
            }
            catch (Exception exc)
            {

                throw;
            }
            await _grpcPub.ConsumerTask;
        }

        public Task OnCompletedAsync() => Task.CompletedTask;

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        async public Task OnNextAsync(StreamServerEventsResponse item, StreamSequenceToken token = null)
        {
            await _grpcPub.WriteAsync(item, _cancellationToken);
        }
    }
}
