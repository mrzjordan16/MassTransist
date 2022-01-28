namespace MassTransit.Azure.ServiceBus.Core.Pipeline
{
    using Contexts;
    using Transport;


    /// <summary>
    /// Creates a message session receiver
    /// </summary>
    public class MessageSessionReceiverFilter :
        MessageReceiverFilter
    {
        public MessageSessionReceiverFilter(IServiceBusMessageReceiver messageReceiver, ServiceBusReceiveEndpointContext context)
            : base(messageReceiver, context)
        {
        }

        protected override IReceiver CreateMessageReceiver(ClientContext context, IServiceBusMessageReceiver messageReceiver)
        {
            return new SessionReceiver(context, messageReceiver);
        }
    }
}
