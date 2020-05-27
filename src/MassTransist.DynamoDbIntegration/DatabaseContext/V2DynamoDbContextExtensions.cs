using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using MassTransist.DynamoDbIntegration.Saga;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    public static class V2DynamoDbContextExtensions
    {
        public static async Task<AggregatedEvents> LoadEventsAsync(this IDynamoDBContext context, Guid aggregateId, IEnumerable<Type> knownTypes, DynamoDbEventStoreOptions options)
        {
            var data = await context.GetEventsAsync(aggregateId, options);

            var events = new List<object>();
            if(data is null) return new AggregatedEvents { Events = events, LastVersion = null };

            events.AddRange(data.SelectMany(@event => JsonSerialization.Deserialize(@event, knownTypes)));
            return new AggregatedEvents { Events = events, LastVersion = data.Max(model => model.Version), AggregateId = aggregateId.ToString() };
        }

        public static async Task SaveEventsAsync<TSaga>(this IDynamoDBContext context, TSaga instance, DynamoDbEventStoreOptions options) where TSaga : class, IV2EventSourcedSaga
        {
            var writer = context.CreateBatchWrite<V2EventStoreModel>(new DynamoDBOperationConfig { OverrideTableName = options.StoreName });
            
            var changes = instance.GetChanges();
            writer.AddPutItems(changes);

            await writer.ExecuteAsync();
        }

        public static async Task DeleteEventsAsync<TSaga>(this IDynamoDBContext context, TSaga instance, DynamoDbEventStoreOptions options) where TSaga : class, IV2EventSourcedSaga
        {
            var events = await context.GetEventsAsync(instance.CorrelationId, options);
            var writer = context.CreateBatchWrite<V2EventStoreModel>(new DynamoDBOperationConfig { OverrideTableName = options.StoreName });

            writer.AddDeleteItems(events);

            await writer.ExecuteAsync();
        }

        private static async Task<List<V2EventStoreModel>> GetEventsAsync(this IDynamoDBContext context, Guid correlationId, DynamoDbEventStoreOptions options)
        {
            var events = new List<V2EventStoreModel>();
            var query = context.QueryAsync<V2EventStoreModel>(correlationId.ToString(), new DynamoDBOperationConfig { OverrideTableName = options.StoreName });

            do
            {
                events.AddRange(await query.GetNextSetAsync());
            }
            while(query.IsDone is false);
            
            return events;
        }
    }
}
