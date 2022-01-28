namespace MassTransit.KafkaIntegration.Contexts
{
    using Transport;
    using Transports;


    public interface IProducerContextSupervisor<TKey, TValue> :
        ITransportSupervisor<ProducerContext<TKey, TValue>>,
        IKafkaProducerFactory<TKey, TValue>
        where TValue : class
    {
    }
}
