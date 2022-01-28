﻿namespace MassTransit.Transports.InMemory.Fabric
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenPipes;
    using GreenPipes.Internals.Extensions;
    using GreenPipes.Util;
    using Util;


    public class InMemoryQueue :
        IInMemoryQueue
    {
        readonly TaskCompletionSource<IInMemoryQueueConsumer> _consumer;
        readonly Connectable<IInMemoryQueueConsumer> _consumers;
        readonly ChannelExecutor _executor;
        readonly string _name;
        readonly CancellationTokenSource _source;

        public InMemoryQueue(string name, int concurrencyLevel)
        {
            _name = name;

            _consumers = new Connectable<IInMemoryQueueConsumer>();
            _consumer = Util.TaskUtil.GetTask<IInMemoryQueueConsumer>();
            _source = new CancellationTokenSource();

            _executor = new ChannelExecutor(concurrencyLevel, false);
        }

        public ConnectHandle ConnectConsumer(IInMemoryQueueConsumer consumer)
        {
            try
            {
                var handle = _consumers.Connect(consumer);

                _consumer.TrySetResult(consumer);

                return new ConsumerHandle(this, handle);
            }
            catch (Exception exception)
            {
                throw new ConfigurationException($"Only a single consumer can be connected to a queue: {_name}", exception);
            }
        }

        public Task Deliver(DeliveryContext<InMemoryTransportMessage> context)
        {
            return context.WasAlreadyDelivered(this)
                ? Task.CompletedTask
                : context.Message.Delay > TimeSpan.Zero
                    ? DeliverWithDelay(context)
                    : _executor.Push(() => DispatchMessage(context), context.CancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            _source.Cancel();

            await _executor.DisposeAsync().ConfigureAwait(false);
        }

        Task DeliverWithDelay(DeliveryContext<InMemoryTransportMessage> context)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(context.Message.Delay.Value, _source.Token).ConfigureAwait(false);

                    await _executor.Push(() => DispatchMessage(context), _source.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }, context.CancellationToken);

            return Task.CompletedTask;
        }

        async Task DispatchMessage(DeliveryContext<InMemoryTransportMessage> context)
        {
            await _consumer.Task.OrCanceled(_source.Token).ConfigureAwait(false);

            try
            {
                await _consumers.ForEachAsync(x => x.Consume(context.Message, _source.Token)).ConfigureAwait(false);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public override string ToString()
        {
            return $"Queue({_name})";
        }


        class ConsumerHandle :
            ConnectHandle
        {
            readonly ConnectHandle _handle;
            readonly InMemoryQueue _queue;

            public ConsumerHandle(InMemoryQueue queue, ConnectHandle handle)
            {
                _queue = queue;
                _handle = handle;
            }

            public void Dispose()
            {
                _handle.Dispose();
            }

            public void Disconnect()
            {
                _handle.Disconnect();
            }
        }
    }
}
