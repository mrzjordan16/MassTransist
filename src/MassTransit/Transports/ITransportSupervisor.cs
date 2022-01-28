namespace MassTransit.Transports
{
    using GreenPipes;
    using GreenPipes.Agents;


    public interface ITransportSupervisor<out T> :
        ISupervisor<T>
        where T : class, PipeContext
    {
        void AddSendAgent<TAgent>(TAgent agent)
            where TAgent : IAgent;

        void AddConsumeAgent<TAgent>(TAgent agent)
            where TAgent : IAgent;
    }
}
