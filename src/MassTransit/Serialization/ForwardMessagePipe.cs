namespace MassTransit.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GreenPipes;


    public class ForwardMessagePipe<T> :
        IPipe<SendContext<T>>
        where T : class
    {
        readonly ConsumeContext<T> _context;
        readonly IPipe<SendContext<T>> _pipe;

        public ForwardMessagePipe(ConsumeContext<T> context, IPipe<SendContext<T>> pipe = default)
        {
            _context = context;
            _pipe = pipe;
        }

        void IProbeSite.Probe(ProbeContext context)
        {
            _pipe.Probe(context);
        }

        public async Task Send(SendContext<T> context)
        {
            context.MessageId = _context.MessageId;
            context.RequestId = _context.RequestId;
            context.ConversationId = _context.ConversationId;
            context.CorrelationId = _context.CorrelationId;
            context.InitiatorId = _context.InitiatorId;
            context.SourceAddress = _context.SourceAddress;
            context.ResponseAddress = _context.ResponseAddress;
            context.FaultAddress = _context.FaultAddress;

            if (_context.ExpirationTime.HasValue)
                context.TimeToLive = _context.ExpirationTime.Value.ToUniversalTime() - DateTime.UtcNow;

            foreach (KeyValuePair<string, object> header in _context.Headers.GetAll())
                context.Headers.Set(header.Key, header.Value);

            if (_pipe.IsNotEmpty())
                await _pipe.Send(context).ConfigureAwait(false);

            var forwarderAddress = _context.ReceiveContext.InputAddress ?? _context.DestinationAddress;
            if (forwarderAddress != null)
                context.Headers.Set(MessageHeaders.ForwarderAddress, forwarderAddress.ToString());

            if (JsonMessageSerializer.JsonContentType.Equals(_context.ReceiveContext.ContentType))
                context.Serializer = new ForwardJsonMessageSerializer(_context.ReceiveContext);
            else if (XmlMessageSerializer.XmlContentType.Equals(_context.ReceiveContext.ContentType))
                context.Serializer = new ForwardXmlMessageSerializer(_context.ReceiveContext);
            else
                context.Serializer = new CopyBodySerializer(_context.ReceiveContext);
        }
    }
}
