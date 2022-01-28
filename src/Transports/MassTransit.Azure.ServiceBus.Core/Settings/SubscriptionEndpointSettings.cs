namespace MassTransit.Azure.ServiceBus.Core.Settings
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using global::Azure.Messaging.ServiceBus.Administration;
    using Topology;
    using Topology.Configurators;
    using Transport;


    public class SubscriptionEndpointSettings :
        BaseClientSettings,
        SubscriptionSettings
    {
        readonly CreateTopicOptions _createTopicOptions;
        readonly SubscriptionConfigurator _subscriptionConfigurator;

        public SubscriptionEndpointSettings(IServiceBusEndpointConfiguration configuration, string topicName, string subscriptionName)
            : this(configuration, Defaults.GetCreateTopicOptions(topicName), subscriptionName)
        {
        }

        public SubscriptionEndpointSettings(IServiceBusEndpointConfiguration configuration, CreateTopicOptions createTopicOptions, string subscriptionName)
            : this(configuration, createTopicOptions, new SubscriptionConfigurator(createTopicOptions.Name, subscriptionName))
        {
        }

        SubscriptionEndpointSettings(IServiceBusEndpointConfiguration configuration, CreateTopicOptions createTopicOptions,
            SubscriptionConfigurator configurator)
            : base(configuration, configurator)
        {
            _createTopicOptions = createTopicOptions;
            _subscriptionConfigurator = configurator;

            Name = Path = EntityNameFormatter.FormatSubscriptionPath(_subscriptionConfigurator.TopicPath, _subscriptionConfigurator.SubscriptionName);
        }

        public ISubscriptionConfigurator SubscriptionConfigurator => _subscriptionConfigurator;

        public override bool RequiresSession => _subscriptionConfigurator.RequiresSession ?? false;

        CreateTopicOptions SubscriptionSettings.CreateTopicOptions => _createTopicOptions;
        CreateSubscriptionOptions SubscriptionSettings.CreateSubscriptionOptions => _subscriptionConfigurator.GetCreateSubscriptionOptions();

        public CreateRuleOptions Rule { get; set; }
        public RuleFilter Filter { get; set; }

        public override string Path { get; }

        public bool RemoveSubscriptions { get; set; }

        protected override IEnumerable<string> GetQueryStringOptions()
        {
            if (_subscriptionConfigurator.AutoDeleteOnIdle.HasValue && _subscriptionConfigurator.AutoDeleteOnIdle.Value > TimeSpan.Zero
                && _subscriptionConfigurator.AutoDeleteOnIdle.Value != Defaults.AutoDeleteOnIdle)
                yield return $"autodelete={_subscriptionConfigurator.AutoDeleteOnIdle.Value.TotalSeconds}";
        }

        public override void SelectBasicTier()
        {
            _subscriptionConfigurator.AutoDeleteOnIdle = default;
            _subscriptionConfigurator.DefaultMessageTimeToLive = Defaults.BasicMessageTimeToLive;
        }
    }
}
