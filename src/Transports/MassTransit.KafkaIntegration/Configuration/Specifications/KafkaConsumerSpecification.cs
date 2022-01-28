namespace MassTransit.KafkaIntegration.Specifications
{
    using System;
    using System.Collections.Generic;
    using Configurators;
    using Confluent.Kafka;
    using GreenPipes;
    using MassTransit.Configurators;
    using MassTransit.Pipeline.Observables;
    using MassTransit.Registration;
    using Serializers;
    using Transports;


    public class KafkaConsumerSpecification<TKey, TValue> :
        IKafkaConsumerSpecification
        where TValue : class
    {
        readonly Action<IKafkaTopicReceiveEndpointConfigurator<TKey, TValue>> _configure;
        readonly ConsumerConfig _consumerConfig;
        readonly ReceiveEndpointObservable _endpointObservers;
        readonly IHeadersDeserializer _headersDeserializer;
        readonly IKafkaHostConfiguration _hostConfiguration;
        readonly string _topicName;

        public KafkaConsumerSpecification(IKafkaHostConfiguration hostConfiguration, ConsumerConfig consumerConfig, string topicName,
            IHeadersDeserializer headersDeserializer,
            Action<IKafkaTopicReceiveEndpointConfigurator<TKey, TValue>> configure)
        {
            _hostConfiguration = hostConfiguration;
            _consumerConfig = consumerConfig;
            _topicName = topicName;
            _endpointObservers = new ReceiveEndpointObservable();
            _headersDeserializer = headersDeserializer;
            _configure = configure;
            EndpointName = $"{KafkaTopicAddress.PathPrefix}/{_topicName}";
        }

        public string EndpointName { get; }

        public ReceiveEndpoint CreateReceiveEndpoint(IBusInstance busInstance)
        {
            var endpointConfiguration = busInstance.HostConfiguration.CreateReceiveEndpointConfiguration(EndpointName);
            endpointConfiguration.ConnectReceiveEndpointObserver(_endpointObservers);

            var configurator = new KafkaTopicReceiveEndpointConfiguration<TKey, TValue>(_hostConfiguration, _consumerConfig, _topicName, busInstance,
                endpointConfiguration, _headersDeserializer);

            _configure?.Invoke(configurator);

            var result = BusConfigurationResult.CompileResults(configurator.Validate());

            try
            {
                return configurator.Build();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException(result, "An exception occurred creating the Kafka receive endpoint", ex);
            }
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (string.IsNullOrEmpty(_topicName))
                yield return this.Failure("Topic", "should not be empty");

            if (string.IsNullOrEmpty(_consumerConfig.GroupId))
                yield return this.Failure("GroupId", "should not be empty");
        }

        public ConnectHandle ConnectReceiveEndpointObserver(IReceiveEndpointObserver observer)
        {
            return _endpointObservers.Connect(observer);
        }
    }
}
