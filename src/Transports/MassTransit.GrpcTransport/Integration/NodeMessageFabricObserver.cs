namespace MassTransit.GrpcTransport.Integration
{
    using System;
    using System.Linq;
    using Context;
    using Contexts;
    using Contracts;
    using Fabric;


    public class NodeMessageFabricObserver :
        IMessageFabricObserver
    {
        readonly IGrpcHostNode _hostNode;
        readonly INodeCollection _nodes;

        public NodeMessageFabricObserver(INodeCollection nodes, IGrpcHostNode hostNode)
        {
            _nodes = nodes;
            _hostNode = hostNode;
        }

        public void ExchangeDeclared(NodeContext context, string name, ExchangeType exchangeType)
        {
            if (context.NodeType != NodeType.Host)
                return;

            Send(context, new Topology
            {
                Exchange = new Exchange
                {
                    Name = name,
                    Type = exchangeType
                }
            });
        }

        public void ExchangeBindingCreated(NodeContext context, string source, string destination, string routingKey)
        {
            if (context.NodeType != NodeType.Host)
                return;

            Send(context, new Topology
            {
                ExchangeBind = new ExchangeBind
                {
                    Source = source,
                    Destination = destination,
                    RoutingKey = routingKey ?? ""
                }
            });
        }

        public void QueueDeclared(NodeContext context, string name)
        {
            if (context.NodeType != NodeType.Host)
                return;

            Send(context, new Topology {Queue = new Queue {Name = name}});
        }

        public void QueueBindingCreated(NodeContext context, string source, string destination)
        {
            if (context.NodeType != NodeType.Host)
                return;

            Send(context, new Topology
            {
                QueueBind = new QueueBind
                {
                    Source = source,
                    Destination = destination,
                }
            });
        }

        public TopologyHandle ConsumerConnected(NodeContext context, TopologyHandle handle, string queueName)
        {
            if (context.NodeType != NodeType.Host)
                return handle;

            return Send(context, new Topology
            {
                Receiver = new Receiver
                {
                    QueueName = queueName,
                    ReceiverId = handle.Id
                }
            }, handle);
        }

        TopologyHandle Send(NodeContext context, Topology topology, TopologyHandle handle = default)
        {
            try
            {
                handle = _hostNode.AddTopology(topology, handle);

                var transportMessage = new TransportMessage
                {
                    MessageId = NewId.NextGuid().ToString(),
                    Topology = topology
                };

                foreach (var node in _nodes.Where(x => x.NodeAddress != context.NodeAddress))
                {
                    if (!node.Writer.TryWrite(transportMessage))
                        LogContext.Error?.Log("Failed to Send Topology {Topology} to {Address}", topology.ChangeCase, node.NodeAddress);
                }

                return handle;
            }
            catch (Exception exception)
            {
                LogContext.Error?.Log(exception, "Failed to send topology message");
                throw;
            }
        }
    }
}
