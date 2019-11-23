using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class EventStoreModel
    {
        public EventStoreModel() => Events = new List<EventModel>();

        [DynamoDBHashKey]
        public string CorrelationId { get; set; }

        [DynamoDBVersion]
        public int? Version { get; set; }

        public EventMetadata Metadata { get;set; }
        
        public List<EventModel> Events { get; set; }
    }

    public class EventModel
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get;set; }
        public string Data { get;set; }
    }
}