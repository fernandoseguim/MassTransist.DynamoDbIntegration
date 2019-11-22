using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Amazon.DynamoDBv2.DataModel;

namespace MassTransist.DynamoDb.EventStore.Saga
{
    public class EventStoreModel
    {
        public EventStoreModel() => Events = new List<EventModel>();

        [DynamoDBHashKey]
        public string AggregateId { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }

        public List<EventModel> Events { get; set; }
    }

    public class EventModel
    {
        public string Id { get;set; }
        public string Type { get;set; }
        public string Data { get;set; }
    }
}