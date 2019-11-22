using System;
using Newtonsoft.Json;

namespace MassTransist.DynamoDb.EventStore.Saga
{
    public class EventMetadata
    {
        [JsonProperty("$correlationId")]
        public Guid CorrelationId { get; set; }

        [JsonProperty("$causationId")]
        public Guid? CausationId { get; set; }
    }
}