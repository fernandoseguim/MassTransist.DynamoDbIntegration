using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using MassTransist.DynamoDbIntegration.Saga;
using Newtonsoft.Json;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    public static class DynamoDbContextExtensions
    {
        public static async Task<EventsData> LoadEvensAsync(this IDynamoDBContext connection, Guid correlationId, IEnumerable<Type> knownTypes, DynamoDBOperationConfig configuration)
        {
            var data = await GetEventsAsync(connection, correlationId, configuration);
            
            var events = new List<object>();
            if(data is null) return new EventsData { Events = events, LastVersion = null };

            events.AddRange(data.Events.SelectMany(@event => JsonSerialization.Deserialize(@event, knownTypes)));
            return new EventsData { Events = events, LastVersion = data.Version };
        }

        public static async Task SaveEvensAsync(this IDynamoDBContext connection, Guid correlationId, IEnumerable<object> changes, DynamoDBOperationConfig configuration)
        {
            var data = await GetEventsAsync(connection, correlationId, configuration) ?? new EventStoreModel { CorrelationId = correlationId.ToString(), Metadata = new EventMetadata { StartedAt = DateTime.UtcNow } };

            var events = changes.Select(change => new EventModel
            {
                Id = Guid.NewGuid().ToString(),
                Type = TypeMapping.GetTypeName(change.GetType()),
                Data = JsonConvert.SerializeObject(change),
                Timestamp = DateTime.UtcNow
            });
            
            data.Events.AddRange(events);
            data.Metadata.UpdatedAt = DateTime.UtcNow;

            await connection.SaveAsync(data, configuration);
        }

        private static async Task<EventStoreModel> GetEventsAsync(IDynamoDBContext connection, Guid correlationId, DynamoDBOperationConfig configuration)
            => await connection.LoadAsync<EventStoreModel>(correlationId.ToString(), configuration);
    }
}
