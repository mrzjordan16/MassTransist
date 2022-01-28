namespace MassTransit.Containers.Tests.DependencyInjection_Tests
{
    using System;
    using System.Threading.Tasks;
    using Common_Tests;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class DependencyInjection_ScopePublish :
        Common_ScopePublish<IServiceProvider>
    {
        readonly IServiceProvider _provider;
        readonly IServiceScope _childContainer;

        public DependencyInjection_ScopePublish()
        {
            var collection = new ServiceCollection();
            collection.AddMassTransit(x =>
            {
                x.AddBus(provider => BusControl);
            });

            _provider = collection.BuildServiceProvider(true);
            _childContainer = _provider.CreateScope();
        }

        [OneTimeTearDown]
        public void Close_container()
        {
            _childContainer.Dispose();
        }

        protected override IPublishEndpoint GetPublishEndpoint()
        {
            return _childContainer.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        }

        protected override void AssertScopesAreEqual(IServiceProvider actual)
        {
            Assert.AreEqual(_childContainer.ServiceProvider, actual);
        }
    }


    [TestFixture]
    public class DependencyInjection_Publish_Filter :
        Common_Publish_Filter
    {
        readonly IServiceProvider _provider;
        readonly IServiceScope _scope;

        public DependencyInjection_Publish_Filter()
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => new MyId(Guid.NewGuid()));
            services.AddSingleton(TaskCompletionSource);

            services.AddMassTransit(ConfigureRegistration);

            _provider = services.BuildServiceProvider();
            _scope = _provider.CreateScope();
        }

        [OneTimeTearDown]
        public void Close_container()
        {
            _scope.Dispose();
        }

        protected override void ConfigureFilter(IPublishPipelineConfigurator configurator)
        {
            DependencyInjectionFilterExtensions.UsePublishFilter(configurator, typeof(ScopedFilter<>), Registration);
        }

        protected override IBusRegistrationContext Registration => _provider.GetRequiredService<IBusRegistrationContext>();
        protected override MyId MyId => _scope.ServiceProvider.GetRequiredService<MyId>();

        protected override IPublishEndpoint PublishEndpoint => _scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
    }


    [TestFixture]
    public class DependencyInjection_Publish_Filter_Outbox :
        Common_Publish_Filter_Outbox
    {
        readonly IServiceProvider _provider;

        public DependencyInjection_Publish_Filter_Outbox()
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => new MyId(NewId.NextGuid()));
            services.AddSingleton(MyIdSource);
            services.AddSingleton(ConsumerSource);

            services.AddMassTransit(ConfigureRegistration);

            _provider = services.BuildServiceProvider();
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            configurator.UseMessageScope(_provider);

            base.ConfigureInMemoryReceiveEndpoint(configurator);
        }

        protected override void ConfigureFilter(IPublishPipelineConfigurator configurator)
        {
            DependencyInjectionFilterExtensions.UsePublishFilter(configurator, typeof(ScopedFilter<>), Registration);
        }

        protected override IBusRegistrationContext Registration => _provider.GetRequiredService<IBusRegistrationContext>();
    }


    [TestFixture]
    public class DependencyInjection_Publish_Filter_Fault :
        Common_Publish_Filter_Fault
    {
        readonly ServiceProvider _provider;

        public DependencyInjection_Publish_Filter_Fault()
        {
            var services = new ServiceCollection();
            services.AddSingleton(TaskCompletionSource);
            services.AddSingleton(Marker);

            services.AddMassTransit(ConfigureRegistration);

            _provider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public async Task Close_container()
        {
            await _provider.DisposeAsync();
        }

        protected override void ConfigureFilter(IPublishPipelineConfigurator configurator)
        {
            DependencyInjectionFilterExtensions.UsePublishFilter(configurator, typeof(ScopedFilter<>), Registration);
        }

        protected override IBusRegistrationContext Registration => _provider.GetRequiredService<IBusRegistrationContext>();
    }
}
