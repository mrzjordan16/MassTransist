namespace MassTransit.ConsumerSpecifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using ConsumeConfigurators;
    using GreenPipes;
    using Metadata;


    public class ConsumerSpecification<TConsumer> :
        OptionsSet,
        IConsumerSpecification<TConsumer>
        where TConsumer : class
    {
        readonly ConnectHandle[] _handles;
        readonly IReadOnlyDictionary<Type, IConsumerMessageSpecification<TConsumer>> _messageTypes;
        readonly ConsumerConfigurationObservable _observers;

        public ConsumerSpecification(IEnumerable<IConsumerMessageSpecification<TConsumer>> messageSpecifications)
        {
            _messageTypes = messageSpecifications.ToDictionary(x => x.MessageType);

            _observers = new ConsumerConfigurationObservable();
            _handles = _messageTypes.Values.Select(x => x.ConnectConsumerConfigurationObserver(_observers)).ToArray();
        }

        public void Message<T>(Action<IConsumerMessageConfigurator<T>> configure)
            where T : class
        {
            IConsumerMessageSpecification<TConsumer, T> specification = GetMessageSpecification<T>();

            configure?.Invoke(specification);
        }

        public void ConsumerMessage<T>(Action<IConsumerMessageConfigurator<TConsumer, T>> configure)
            where T : class
        {
            IConsumerMessageSpecification<TConsumer, T> specification = GetMessageSpecification<T>();

            configure?.Invoke(specification);
        }

        public IConsumerMessageSpecification<TConsumer, T> GetMessageSpecification<T>()
            where T : class
        {
            foreach (IConsumerMessageSpecification<TConsumer> messageSpecification in _messageTypes.Values)
            {
                if (messageSpecification.TryGetMessageSpecification(out IConsumerMessageSpecification<TConsumer, T> result))
                    return result;
            }

            throw new ArgumentException($"MessageType {TypeMetadataCache<T>.ShortName} is not consumed by {TypeMetadataCache<TConsumer>.ShortName}");
        }

        public IEnumerable<ValidationResult> Validate()
        {
            _observers.All(observer =>
            {
                observer.ConsumerConfigured(this);
                return true;
            });

            foreach (var result in _messageTypes.Values.SelectMany(x => x.Validate()))
            {
                yield return result;
            }

            foreach (var result in ValidateOptions())
            {
                yield return result;
            }
        }

        public void AddPipeSpecification(IPipeSpecification<ConsumerConsumeContext<TConsumer>> specification)
        {
            foreach (IConsumerMessageSpecification<TConsumer> messageSpecification in _messageTypes.Values)
                messageSpecification.AddPipeSpecification(specification);
        }

        public ConnectHandle ConnectConsumerConfigurationObserver(IConsumerConfigurationObserver observer)
        {
            return _observers.Connect(observer);
        }
    }
}
