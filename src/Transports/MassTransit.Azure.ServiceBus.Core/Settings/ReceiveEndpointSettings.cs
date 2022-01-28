namespace MassTransit.Azure.ServiceBus.Core.Settings
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using global::Azure.Messaging.ServiceBus.Administration;
    using Topology;
    using Topology.Configurators;
    using Transport;


    public class ReceiveEndpointSettings :
        BaseClientSettings,
        ReceiveSettings
    {
        readonly QueueConfigurator _queueConfigurator;

        public ReceiveEndpointSettings(IServiceBusEndpointConfiguration endpointConfiguration, string queueName, QueueConfigurator queueConfigurator)
            : base(endpointConfiguration, queueConfigurator)
        {
            _queueConfigurator = queueConfigurator;

            Name = queueName;
        }

        public IQueueConfigurator QueueConfigurator => _queueConfigurator;

        public bool RemoveSubscriptions { get; set; }

        public override string Path => _queueConfigurator.FullPath;

        public override bool RequiresSession => _queueConfigurator.RequiresSession ?? false;

        public CreateQueueOptions GetCreateQueueOptions()
        {
            return _queueConfigurator.GetCreateQueueOptions();
        }

        public override void SelectBasicTier()
        {
            _queueConfigurator.AutoDeleteOnIdle = default;
            _queueConfigurator.DefaultMessageTimeToLive = Defaults.BasicMessageTimeToLive;
        }

        protected override IEnumerable<string> GetQueryStringOptions()
        {
            if (_queueConfigurator.AutoDeleteOnIdle.HasValue && _queueConfigurator.AutoDeleteOnIdle.Value > TimeSpan.Zero
                && _queueConfigurator.AutoDeleteOnIdle.Value != Defaults.AutoDeleteOnIdle)
                yield return $"autodelete={_queueConfigurator.AutoDeleteOnIdle.Value.TotalSeconds}";
        }
    }
}
