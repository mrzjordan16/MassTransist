﻿namespace MassTransit.ConsumeConfigurators
{
    using System;
    using GreenPipes;


    public interface IConsumerConfigurator :
        IConsumeConfigurator,
        IConsumerConfigurationObserverConnector
    {
    }


    public interface IConsumerConfigurator<TConsumer> :
        IPipeConfigurator<ConsumerConsumeContext<TConsumer>>,
        IConsumerConfigurationObserverConnector,
        IConsumeConfigurator,
        IOptionsSet
        where TConsumer : class
    {
        /// <summary>
        /// Add middleware to the message pipeline, which is invoked prior to the consumer factory.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="configure">The callback to configure the message pipeline</param>
        void Message<T>(Action<IConsumerMessageConfigurator<T>> configure)
            where T : class;

        /// <summary>
        /// Add middleware to the consumer pipeline, for the specified message type, which is invoked
        /// after the consumer factory.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="configure">The callback to configure the message pipeline</param>
        void ConsumerMessage<T>(Action<IConsumerMessageConfigurator<TConsumer, T>> configure)
            where T : class;
    }
}
