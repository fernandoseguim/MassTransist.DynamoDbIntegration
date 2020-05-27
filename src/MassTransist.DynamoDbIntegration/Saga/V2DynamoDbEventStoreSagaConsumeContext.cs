using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using MassTransist.DynamoDbIntegration.DatabaseContext;
using MassTransit;
using MassTransit.Context;
using MassTransit.Logging;
using MassTransit.Util;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class V2DynamoDbEventStoreSagaConsumeContext<TSaga, TMessage> : ConsumeContextProxyScope<TMessage>, SagaConsumeContext<TSaga, TMessage>
            where TMessage : class
            where TSaga : class, IV2EventSourcedSaga
    {
        private static readonly ILog Log = Logger.Get<V2DynamoDbSagaRepository<TSaga>>();
        private readonly IDynamoDBContext _connection;
        private readonly DynamoDbEventStoreOptions _options;

        public V2DynamoDbEventStoreSagaConsumeContext(IDynamoDBContext connection, ConsumeContext<TMessage> context, TSaga instance, DynamoDbEventStoreOptions options) : base(context)
        {
            Saga = instance;
            _connection = connection;
            _options = options;
        }

        Guid? MessageContext.CorrelationId => Saga.CorrelationId;

        async Task SagaConsumeContext<TSaga>.SetCompleted()
        {
            if(_options.DeleteWhenCompleted)
            {
                await _connection.DeleteEventsAsync(Saga, _options);
            }
            
            IsCompleted = Saga.Finalized() && _options.SaveFinalState is false;

            if (Log.IsDebugEnabled)
                Log.DebugFormat("SAGA:{0}:{1} Removed {2}", TypeMetadataCache<TSaga>.ShortName,
                    TypeMetadataCache<TMessage>.ShortName,
                    Saga.CorrelationId);
        }

        public TSaga Saga { get; }
        public bool IsCompleted { get; private set; }
    }
}