using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using MassTransist.DynamoDb.EventStore.Saga;
using Newtonsoft.Json;

namespace MassTransist.DynamoDb.EventStore
{
    public static class DynamoDbContextExtensions
    {
        public static async Task<EventsData> LoadEvensAsync(this IDynamoDBContext connection, Guid correlationId, Assembly assembly, DynamoDBOperationConfig configuration)
        {
            var data = await GetEventsAsync(connection, correlationId, configuration);
            var assemblyName = assembly.GetName().Name;

            var events = new List<object>();
            if(data is null) return new EventsData { Events = events, LastVersion = null };

            events.AddRange(data.Events.SelectMany(@event => JsonSerialization.Deserialize(@event, assemblyName)));
            return new EventsData{ Events = events, LastVersion = data.Version};
        }

        public static async Task SaveEvensAsync(this IDynamoDBContext connection, Guid correlationId, IEnumerable<object> changes, DynamoDBOperationConfig configuration)
        {
            var data = await GetEventsAsync(connection, correlationId, configuration) ?? new EventStoreModel { AggregateId = correlationId.ToString() };

            var events = changes.Select(change => new EventModel
            {
                Id = Guid.NewGuid().ToString(),
                Type = TypeMapping.GetTypeName(change.GetType()),
                Data = JsonConvert.SerializeObject(change)
            });

            data.Events.AddRange(events);

            await connection.SaveAsync(data, configuration);
        }

        private static async Task<EventStoreModel> GetEventsAsync(IDynamoDBContext connection, Guid correlationId, DynamoDBOperationConfig configuration)
            => await connection.LoadAsync<EventStoreModel>(correlationId.ToString(), configuration);
    }
}
