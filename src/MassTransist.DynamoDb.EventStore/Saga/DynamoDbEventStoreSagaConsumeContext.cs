using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using MassTransit;
using MassTransit.Context;
using MassTransit.Logging;
using MassTransit.Util;

namespace MassTransist.DynamoDb.EventStore.Saga
{
    public class DynamoDbEventStoreSagaConsumeContext<TSaga, TMessage> : ConsumeContextProxyScope<TMessage>, SagaConsumeContext<TSaga, TMessage>
            where TMessage : class
            where TSaga : class, IEventSourcedSaga
    {
        private static readonly ILog Log = Logger.Get<DynamoDbSagaRepository<TSaga>>();
        private readonly IDynamoDBContext _connection;

        public DynamoDbEventStoreSagaConsumeContext(IDynamoDBContext connection, ConsumeContext<TMessage> context,
            TSaga instance) : base(context)
        {
            Saga = instance;
            _connection = connection;
        }

        Guid? MessageContext.CorrelationId => Saga.CorrelationId;

        async Task SagaConsumeContext<TSaga>.SetCompleted()
        {
            //TODO: Implement strategy to remove saga events
            //await _connection.DeleteStreamAsync(Saga.StreamName, Saga.ExpectedVersion, false);

            IsCompleted = true;
            if (Log.IsDebugEnabled)
                Log.DebugFormat("SAGA:{0}:{1} Removed {2}", TypeMetadataCache<TSaga>.ShortName,
                    TypeMetadataCache<TMessage>.ShortName,
                    Saga.CorrelationId);
        }

        public TSaga Saga { get; }
        public bool IsCompleted { get; private set; }
    }
}