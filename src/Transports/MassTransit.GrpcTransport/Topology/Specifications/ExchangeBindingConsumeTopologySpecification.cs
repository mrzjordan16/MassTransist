﻿namespace MassTransit.GrpcTransport.Topology.Specifications
{
    using System.Collections.Generic;
    using Contracts;
    using GreenPipes;
    using GrpcTransport.Builders;


    /// <summary>
    /// Used to bind an exchange to the consuming queue's exchange
    /// </summary>
    public class ExchangeBindingConsumeTopologySpecification :
        IGrpcConsumeTopologySpecification
    {
        readonly string _exchange;
        readonly ExchangeType _exchangeType;
        readonly string _routingKey;

        public ExchangeBindingConsumeTopologySpecification(string exchange, ExchangeType exchangeType, string routingKey)
        {
            _exchange = exchange;
            _routingKey = routingKey;
            _exchangeType = exchangeType;
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }

        public void Apply(IGrpcConsumeTopologyBuilder builder)
        {
            builder.ExchangeDeclare(_exchange, _exchangeType);
            builder.ExchangeBind(_exchange, builder.Exchange, _routingKey);
        }
    }
}
