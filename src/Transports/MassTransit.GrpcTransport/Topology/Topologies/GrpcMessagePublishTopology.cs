﻿namespace MassTransit.GrpcTransport.Topology.Topologies
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Contracts;
    using Integration;
    using MassTransit.Topology;
    using MassTransit.Topology.Topologies;


    public class GrpcMessagePublishTopology<TMessage> :
        MessagePublishTopology<TMessage>,
        IGrpcMessagePublishTopologyConfigurator<TMessage>
        where TMessage : class
    {
        readonly IList<IGrpcMessagePublishTopology> _implementedMessageTypes;
        readonly IMessageTopology<TMessage> _messageTopology;

        public GrpcMessagePublishTopology(IMessageTopology<TMessage> messageTopology)
        {
            _messageTopology = messageTopology;
            _implementedMessageTypes = new List<IGrpcMessagePublishTopology>();
        }

        public ExchangeType ExchangeType { get; set; }

        public void Apply(IGrpcPublishTopologyBuilder builder)
        {
            var exchangeName = _messageTopology.EntityName;

            builder.ExchangeDeclare(exchangeName, ExchangeType);

            if (builder.ExchangeName != null)
                builder.ExchangeBind(builder.ExchangeName, exchangeName, builder.ExchangeType == ExchangeType.Topic ? "#" : default);
            else
            {
                builder.ExchangeName = exchangeName;
                builder.ExchangeType = ExchangeType;
            }

            foreach (var configurator in _implementedMessageTypes)
                configurator.Apply(builder);
        }

        public override bool TryGetPublishAddress(Uri baseAddress, out Uri publishAddress)
        {
            var exchangeName = _messageTopology.EntityName;

            publishAddress = new GrpcEndpointAddress(new GrpcHostAddress(baseAddress), exchangeName, exchangeType: ExchangeType);
            return true;
        }

        public void AddImplementedMessageConfigurator<T>(IGrpcMessagePublishTopologyConfigurator<T> configurator, bool direct)
            where T : class
        {
            var adapter = new TypeAdapter<T>(configurator, direct);

            _implementedMessageTypes.Add(adapter);
        }


        class TypeAdapter<T> :
            IGrpcMessagePublishTopology
            where T : class
        {
            readonly IGrpcMessagePublishTopologyConfigurator<T> _configurator;
            readonly bool _direct;

            public TypeAdapter(IGrpcMessagePublishTopologyConfigurator<T> configurator, bool direct)
            {
                _configurator = configurator;
                _direct = direct;
            }

            public void Apply(IGrpcPublishTopologyBuilder builder)
            {
                if (_direct)
                {
                    var implementedBuilder = builder.CreateImplementedBuilder();

                    _configurator.Apply(implementedBuilder);
                }
            }
        }
    }
}
