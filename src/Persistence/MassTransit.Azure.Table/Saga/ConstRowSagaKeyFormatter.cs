namespace MassTransit.Azure.Table.Saga
{
    using System;
    using MassTransit.Saga;


    public class ConstRowSagaKeyFormatter<TSaga> :
        ISagaKeyFormatter<TSaga>
        where TSaga : class, ISaga
    {
        readonly string _rowKey;

        public ConstRowSagaKeyFormatter(string rowKey)
        {
            _rowKey = rowKey;
        }

        public (string partitionKey, string rowKey) Format(Guid correlationId)
        {
            return (correlationId.ToString(), _rowKey);
        }
    }
}
