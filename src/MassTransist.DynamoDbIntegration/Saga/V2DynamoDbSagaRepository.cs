using System;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using GreenPipes;
using MassTransist.DynamoDbIntegration.DatabaseContext;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Saga;
using MassTransit.Util;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class V2DynamoDbSagaRepository<TSaga> : ISagaRepository<TSaga>, IV2RetrieveSagaFromRepository<TSaga> where TSaga : class, IV2EventSourcedSaga
    {
        private static readonly ILog Log = Logger.Get<V2DynamoDbSagaRepository<TSaga>>();
        private readonly IDynamoDBContext _connection;
        private readonly DynamoDbEventStoreOptions _options;
        private readonly KnownEventTypes _knownTypes;
        
        public V2DynamoDbSagaRepository(IDynamoDBContext connection, DynamoDbEventStoreOptions options, KnownEventTypes knownTypes)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _knownTypes = knownTypes ?? throw new ArgumentNullException(nameof(knownTypes));
            
            if (TypeMapping.GetTypeName(typeof(V2EventSourcedSagaInstance.SagaInstanceTransitioned)).Contains("+"))
                TypeMapping.Add<V2EventSourcedSagaInstance.SagaInstanceTransitioned>("SagaInstanceTransitioned");
        }

        public async Task<TSaga> GetSaga(Guid correlationId)
        {
            var aggregatedEvents = await _connection.LoadEventsAsync(correlationId, _knownTypes, _options);

            if (aggregatedEvents == null) return null;

            var saga = SagaFactory();
            saga.Initialize(aggregatedEvents.Events);
            saga.CorrelationId = correlationId;
            saga.ExpectedVersion = aggregatedEvents.LastVersion;
            return saga;
        }

        public async Task Send<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next) where T : class
        {
            if (!context.CorrelationId.HasValue) throw new SagaException("The CorrelationId was not specified", typeof(TSaga), typeof(T));

            var sagaId = context.CorrelationId.Value;

            if (policy.PreInsertInstance(context, out var instance))
            {
                await PreInsertSagaInstance<T>(instance, context.MessageId).ConfigureAwait(false);
            }

            if (instance == null) instance = await GetSaga(sagaId);

            if (instance == null)
            {
                var missingSagaPipe = new MissingPipe<T>(_connection, _options, next);
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

        private async Task SendToInstance<T>(ConsumeContext<T> context, ISagaPolicy<TSaga, T> policy, IPipe<SagaConsumeContext<TSaga, T>> next, TSaga instance) where T : class
        {
            try
            {
                if (Log.IsDebugEnabled)
                    Log.DebugFormat("SAGA:{0}:{1} Used {2}", TypeMetadataCache<TSaga>.ShortName,
                        instance.CorrelationId, TypeMetadataCache<T>.ShortName);

                var sagaConsumeContext = new V2DynamoDbEventStoreSagaConsumeContext<TSaga, T>(_connection, context, instance, _options);

                await policy.Existing(sagaConsumeContext, next).ConfigureAwait(false);
                
                if (!sagaConsumeContext.IsCompleted)
                {
                    await _connection.SaveEventsAsync(instance, _options);
                }
            }
            catch (SagaException sagaException)
            {
                if(Log.IsDebugEnabled)
                    Log.Error(sagaException.Message, sagaException);
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
                await _connection.SaveEventsAsync(instance, _options);

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
            private readonly DynamoDbEventStoreOptions _options;

            public MissingPipe(IDynamoDBContext connection, DynamoDbEventStoreOptions options, IPipe<SagaConsumeContext<TSaga, TMessage>> next)
            {
                _connection = connection ?? throw new ArgumentNullException(nameof(connection));
                _options = options ?? throw new ArgumentNullException(nameof(options));
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
                    new V2DynamoDbEventStoreSagaConsumeContext<TSaga, TMessage>(_connection, context, instance, _options);

                await _next.Send(proxy).ConfigureAwait(false);

                if (!proxy.IsCompleted) { await _connection.SaveEventsAsync(instance, _options); }
            }
        }
    }
}