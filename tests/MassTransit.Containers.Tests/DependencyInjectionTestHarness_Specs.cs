namespace MassTransit.Containers.Tests
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Saga;
    using Scenarios;
    using TestFramework.Messages;
    using Testing;


    [TestFixture]
    public class DependencyInjectionTestHarness_Specs
    {
        [Test]
        public async Task Should_support_the_test_harness()
        {
            var provider = new ServiceCollection()
                .AddMassTransitInMemoryTestHarness(cfg =>
                {
                    cfg.AddConsumer<PingRequestConsumer>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<InMemoryTestHarness>();

            await harness.Start();
            try
            {
                var bus = provider.GetRequiredService<IBus>();

                IRequestClient<PingMessage> client = bus.CreateRequestClient<PingMessage>();

                await client.GetResponse<PongMessage>(new PingMessage());

                Assert.That(await harness.Consumed.Any<PingMessage>());
            }
            finally
            {
                await harness.Stop();

                await provider.DisposeAsync();
            }
        }
    }


    [TestFixture]
    public class DependencyInjectionConsumerHarness_Specs
    {
        [Test]
        public async Task Should_support_the_consumer_harness()
        {
            var provider = new ServiceCollection()
                .AddMassTransitInMemoryTestHarness(cfg =>
                {
                    cfg.AddConsumer<PingRequestConsumer>();

                    cfg.AddConsumerTestHarness<PingRequestConsumer>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<InMemoryTestHarness>();

            await harness.Start();
            try
            {
                var bus = provider.GetRequiredService<IBus>();

                IRequestClient<PingMessage> client = bus.CreateRequestClient<PingMessage>();

                await client.GetResponse<PongMessage>(new PingMessage());

                Assert.That(await harness.Consumed.Any<PingMessage>());

                var consumerHarness = provider.GetRequiredService<IConsumerTestHarness<PingRequestConsumer>>();

                Assert.That(await consumerHarness.Consumed.Any<PingMessage>());
            }
            finally
            {
                await harness.Stop();

                await provider.DisposeAsync();
            }
        }
    }


    [TestFixture]
    public class DependencyInjectionSagaHarness_Specs
    {
        [Test]
        public async Task Should_support_the_saga_harness()
        {
            var provider = new ServiceCollection()
                .AddMassTransitInMemoryTestHarness(cfg =>
                {
                    cfg.AddSaga<TestSaga>()
                        .InMemoryRepository();

                    cfg.AddSagaTestHarness<TestSaga>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<InMemoryTestHarness>();

            await harness.Start();
            try
            {
                _sagaId = Guid.NewGuid();
                _testValueA = "TestValueA";

                await harness.Bus.Publish(new A
                {
                    CorrelationId = _sagaId,
                    Value = _testValueA
                });

                Assert.That(await harness.Published.Any<A>());

                Assert.That(await harness.Consumed.Any<A>());

                var sagaHarness = provider.GetRequiredService<ISagaTestHarness<TestSaga>>();

                Assert.That(await sagaHarness.Consumed.Any<A>());

                Assert.That(await sagaHarness.Created.Any(x => x.CorrelationId == _sagaId));

                var saga = sagaHarness.Created.Contains(_sagaId);
                Assert.That(saga, Is.Not.Null);
                Assert.That(saga.ValueA, Is.EqualTo(_testValueA));

                Assert.That(await harness.Published.Any<Aa>());

                Assert.That(await harness.Published.Any<B>(), Is.False);
            }
            finally
            {
                await harness.Stop();

                await provider.DisposeAsync();
            }
        }

        Guid _sagaId;
        string _testValueA;


        class TestSaga :
            ISaga,
            InitiatedBy<A>,
            Orchestrates<B>,
            Observes<C, TestSaga>
        {
            protected TestSaga()
            {
            }

            public TestSaga(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            public string ValueA { get; private set; }

            public async Task Consume(ConsumeContext<A> context)
            {
                ValueA = context.Message.Value;
                await context.Publish(new Aa {CorrelationId = CorrelationId});
            }

            public Guid CorrelationId { get; set; }

            public async Task Consume(ConsumeContext<C> message)
            {
            }

            public Expression<Func<TestSaga, C, bool>> CorrelationExpression
            {
                get { return (saga, message) => saga.CorrelationId.ToString() == message.CorrelationId; }
            }

            public async Task Consume(ConsumeContext<B> message)
            {
            }
        }


        class A :
            CorrelatedBy<Guid>
        {
            public string Value { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class Aa :
            CorrelatedBy<Guid>
        {
            public Guid CorrelationId { get; set; }
        }


        class B :
            CorrelatedBy<Guid>
        {
            public Guid CorrelationId { get; set; }
        }


        class C
        {
            public string CorrelationId { get; set; }
        }
    }


    [TestFixture]
    public class DependencyInjectionSagaStateMachineHarness_Specs
    {
        [Test]
        public async Task Should_support_the_saga_harness()
        {
            var provider = new ServiceCollection()
                .AddMassTransitInMemoryTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<TestStateMachine, Instance>()
                        .InMemoryRepository();

                    cfg.AddSagaStateMachineTestHarness<TestStateMachine, Instance>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetRequiredService<InMemoryTestHarness>();

            await harness.Start();
            try
            {
                var sagaId = Guid.NewGuid();

                await harness.Bus.Publish(new Start {CorrelationId = sagaId});

                Assert.IsTrue(await harness.Consumed.Any<Start>(), "Message not received");

                var sagaHarness = provider.GetRequiredService<IStateMachineSagaTestHarness<Instance, TestStateMachine>>();

                Assert.That(await sagaHarness.Consumed.Any<Start>());

                Assert.That(await sagaHarness.Created.Any(x => x.CorrelationId == sagaId));

                var machine = provider.GetRequiredService<TestStateMachine>();

                var instance = sagaHarness.Created.ContainsInState(sagaId, machine, machine.Running);
                Assert.IsNotNull(instance, "Saga instance not found");

                Assert.IsTrue(await harness.Published.Any<Started>(), "Event not published");
            }
            finally
            {
                await harness.Stop();

                await provider.DisposeAsync();
            }
        }


        class Instance :
            SagaStateMachineInstance
        {
            public Instance(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            protected Instance()
            {
            }

            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        sealed class TestStateMachine :
            MassTransitStateMachine<Instance>
        {
            public TestStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Event(() => StartReceived);

                Initially(
                    When(StartReceived)
                        .Activity(x => x.OfType<StartupActivity>())
                        .TransitionTo(Running));
            } // ReSharper disable UnassignedGetOnlyAutoProperty
            public State Running { get; }
            public Event<Start> StartReceived { get; }
        }


        class StartupActivity :
            Activity<Instance, Start>
        {
            readonly IPublishEndpoint _publishEndpoint;

            public StartupActivity(IPublishEndpoint publishEndpoint)
            {
                _publishEndpoint = publishEndpoint;
            }

            public void Probe(ProbeContext context)
            {
            }

            public void Accept(StateMachineVisitor visitor)
            {
                visitor.Visit(this);
            }

            public async Task Execute(BehaviorContext<Instance, Start> context, Behavior<Instance, Start> next)
            {
                await _publishEndpoint.Publish(new Started {CorrelationId = context.Instance.CorrelationId});

                await next.Execute(context);
            }

            public Task Faulted<TException>(BehaviorExceptionContext<Instance, Start, TException> context, Behavior<Instance, Start> next)
                where TException : Exception
            {
                return next.Faulted(context);
            }
        }


        class Start :
            CorrelatedBy<Guid>
        {
            public Guid CorrelationId { get; set; }
        }


        class Started :
            CorrelatedBy<Guid>
        {
            public Guid CorrelationId { get; set; }
        }
    }
}
