namespace MassTransit.GrpcTransport
{
    using System;
    using Topology.Configurators;


    public interface IGrpcBusFactoryConfigurator :
        IBusFactoryConfigurator<IGrpcReceiveEndpointConfigurator>
    {
        /// <summary>
        /// Sets the maximum number of threads used by an in-memory transport, for partitioning
        /// the input queue. This setting also specifies how many threads will be used for dispatching
        /// messages to consumers.
        /// </summary>
        int TransportConcurrencyLimit { set; }

        new IGrpcPublishTopologyConfigurator PublishTopology { get; }

        /// <summary>
        /// Configure the send topology of the message type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configureTopology"></param>
        void Publish<T>(Action<IGrpcMessagePublishTopologyConfigurator<T>> configureTopology)
            where T : class;

        /// <summary>
        /// Configure the base address for the host
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        void Host(Action<IGrpcHostConfigurator> configure = null);

        /// <summary>
        /// Configure the base address for the host
        /// </summary>
        /// <param name="baseAddress">The base address for the in-memory host</param>
        /// <param name="configure"></param>
        /// <returns></returns>
        void Host(Uri baseAddress, Action<IGrpcHostConfigurator> configure = null);
    }
}