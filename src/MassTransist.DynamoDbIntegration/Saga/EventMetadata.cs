using System;
using Newtonsoft.Json;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class EventMetadata
    {
        public DateTime StartedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}