using System;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using GreenPipes;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Saga;
using MassTransit.Util;

namespace MassTransist.DynamoDb.EventStore.Saga
{
    public class DynamoDbSagaRepository<TSaga> : ISagaRepository<TSaga>, IRetrieveSagaFromRepository<TSaga> where TSaga : class, IEventSourcedSaga
    {
        private static readonly ILog Log = Logger.Get<DynamoDbSagaRepository<TSaga>>();
        private readonly IDynamoDBContext _connection;
        private readonly DynamoDBOperationConfig _configuration;

        public DynamoDbSagaRepository(IDynamoDBContext connection, EventStoreOptions options)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            if(options == null) throw new ArgumentNullException(nameof(options));

            _configuration = new DynamoDBOperationConfig { OverrideTableName = options.StoreName, Conversion = DynamoDBEntryConversion.V2 };

            if (TypeMapping.GetTypeName(typeof(EventSourcedSagaInstance.SagaInstanceTransitioned)).Contains("+"))
                TypeMapping.Add<EventSourcedSagaInstance.SagaInstanceTransitioned>("SagaInstanceTransitioned");
        }

        public async Task<TSaga> GetSaga(Guid correlationId)
        {
            var data = await _connection.LoadEvensAsync(correlationId, typeof(TSaga).Assembly, _configuration);

            if (data == null) return null;

            var saga = SagaFactory();
            saga.Initialize(data.Events);
            saga.CorrelationId = correlationId;
            saga.ExpectedVersion = data.LastVersion;
            return saga;
        }

        public async Task Send<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            if (!context.CorrelationId.HasValue) throw new SagaException("The CorrelationId was not specified", typeof(TSaga), typeof(T));

            var sagaId = context.CorrelationId.Value;

            if (policy.PreInsertInstance(context, out var instance)) await PreInsertSagaInstance<T>(instance, context.MessageId).ConfigureAwait(false);

            if (instance == null) instance = await GetSaga(sagaId);

            if (instance == null)
            {
                var missingSagaPipe = new MissingPipe<T>(_connection, _configuration, next);
                await policy.Missing(context, missingSagaPipe).ConfigureAwait(false);
            }
            else
            {
                await SendToInstance(context, policy, next, instance).ConfigureAwait(false);
            }
        }

        public Task SendQuery<T>(SagaQueryConsumeContext<TSaga, T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
            => throw new NotImplementedByDesignException("DynamoDb saga repository does have send quenry implementation");

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("sagaRepository");
            scope.Set(new { Persistence = "connection" });
        }

        async Task SendToInstance<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next, TSaga instance) where T : class
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.DebugFormat("SAGA:{0}:{1} Used {2}", TypeMetadataCache<TSaga>.ShortName,
                        instance.CorrelationId, TypeMetadataCache<T>.ShortName);

                var sagaConsumeContext = new DynamoDbEventStoreSagaConsumeContext<TSaga, T>(_connection, context, instance);

                await policy.Existing(sagaConsumeContext, next).ConfigureAwait(false);
                
                if (!sagaConsumeContext.IsCompleted)
                {
                    var changes = instance.GetChanges();

                    await _connection.SaveEvensAsync(instance.CorrelationId, changes, _configuration);
                }
            }
            catch (SagaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SagaException(ex.Message, typeof(TSaga), typeof(T), instance.CorrelationId, ex);
            }
        }

        private async Task<bool> PreInsertSagaInstance<T>(TSaga instance, Guid? causationId)
        {
            try
            {
                var changes = instance.GetChanges();
                await _connection.SaveEvensAsync(instance.CorrelationId, changes, _configuration);

                if (Log.IsDebugEnabled)
                    Log.DebugFormat("SAGA:{0}:{1} Insert {2}", TypeMetadataCache<TSaga>.ShortName,
                        instance.CorrelationId,
                        TypeMetadataCache<T>.ShortName);
                return true;
            }
            catch (Exception ex)
            {
                if (Log.IsDebugEnabled)
                    Log.DebugFormat("SAGA:{0}:{1} Dupe {2} - {3}", TypeMetadataCache<TSaga>.ShortName,
                        instance.CorrelationId,
                        TypeMetadataCache<T>.ShortName, ex.Message);

                return false;
            }
        }

        private static TSaga SagaFactory()
        {
            var ctor = typeof(TSaga).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
            return (TSaga) ctor?.Invoke(Array.Empty<object>());
        }

        /// <summary>
        /// Once the message pipe has processed the saga instance, add it to the saga repository
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        private class MissingPipe<TMessage> : IPipe<SagaConsumeContext<TSaga, TMessage>> where TMessage : class
        {
            private readonly IPipe<SagaConsumeContext<TSaga, TMessage>> _next;
            private readonly IDynamoDBContext _connection;
            private readonly DynamoDBOperationConfig _configuration;

            public MissingPipe(IDynamoDBContext connection, DynamoDBOperationConfig configuration, IPipe<SagaConsumeContext<TSaga, TMessage>> next)
            {
                _connection = connection ?? throw new ArgumentNullException(nameof(connection));
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _next = next ?? throw new ArgumentNullException(nameof(next));
            }

            void IProbeSite.Probe(ProbeContext context) => _next.Probe(context);

            public async Task Send(SagaConsumeContext<TSaga, TMessage> context)
            {
                var instance = context.Saga;

                if (Log.IsDebugEnabled)
                    Log.DebugFormat("SAGA:{0}:{1} Added {2}", TypeMetadataCache<TSaga>.ShortName,
                        instance.CorrelationId,
                        TypeMetadataCache<TMessage>.ShortName);

                SagaConsumeContext<TSaga, TMessage> proxy =
                    new DynamoDbEventStoreSagaConsumeContext<TSaga, TMessage>(_connection, context, instance);

                await _next.Send(proxy).ConfigureAwait(false);

                if (!proxy.IsCompleted)
                {
                    var changes = instance.GetChanges();
                    await _connection.SaveEvensAsync(instance.CorrelationId, changes, _configuration);
                }
            }
        }
    }
}