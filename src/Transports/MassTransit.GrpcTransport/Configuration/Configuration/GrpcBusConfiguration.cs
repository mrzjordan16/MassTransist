﻿namespace MassTransit.GrpcTransport.Configuration
{
    using System;
    using EndpointConfigurators;
    using GreenPipes;
    using MassTransit.Configuration;
    using MassTransit.Pipeline.Observables;
    using Serialization;


    public class GrpcBusConfiguration :
        GrpcEndpointConfiguration,
        IGrpcBusConfiguration
    {
        readonly BusObservable _busObservers;

        public GrpcBusConfiguration(IGrpcTopologyConfiguration topologyConfiguration, Uri baseAddress)
            : base(topologyConfiguration)
        {
            HostConfiguration = new GrpcHostConfiguration(this, baseAddress, topologyConfiguration);

            Serialization.ClearDeserializers();
            Serialization.DefaultContentType = GrpcMessageSerializer.GrpcContentType;
            Serialization.AddDeserializer(GrpcMessageSerializer.GrpcContentType,
                () => new GrpcMessageDeserializer(GrpcMessageSerializer.Deserializer));
            Serialization.SetSerializer(() => new GrpcMessageSerializer());

            BusEndpointConfiguration = CreateEndpointConfiguration();

            _busObservers = new BusObservable();
        }

        IHostConfiguration IBusConfiguration.HostConfiguration => HostConfiguration;
        IEndpointConfiguration IBusConfiguration.BusEndpointConfiguration => BusEndpointConfiguration;
        IBusObserver IBusConfiguration.BusObservers => _busObservers;

        public IGrpcEndpointConfiguration BusEndpointConfiguration { get; }
        public IGrpcHostConfiguration HostConfiguration { get; }

        public ConnectHandle ConnectBusObserver(IBusObserver observer)
        {
            return _busObservers.Connect(observer);
        }

        public ConnectHandle ConnectEndpointConfigurationObserver(IEndpointConfigurationObserver observer)
        {
            return HostConfiguration.ConnectEndpointConfigurationObserver(observer);
        }
    }
}
