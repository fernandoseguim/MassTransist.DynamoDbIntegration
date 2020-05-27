using System;
using Amazon.DynamoDBv2.DataModel;
using MassTransit.Audit;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class V2EventStoreModel
    {
        public V2EventStoreModel() { }

        public V2EventStoreModel(string aggregateId, int? version, string name, DateTime timestamp, string data)
        {
            AggregateId = aggregateId;
            Version = version;
            Name = name;
            Timestamp = timestamp;
            Data = data;
        }
        
        [DynamoDBHashKey]
        public string AggregateId { get; set; }

        [DynamoDBRangeKey]
        public int? Version { get; set; }
        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public string Data { get; set; }

        public MessageAuditMetadata Metadata { get;set; }
    }
}