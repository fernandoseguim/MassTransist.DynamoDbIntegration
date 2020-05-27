using System.Collections.Generic;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class AggregatedEvents
    {
        public string AggregateId { get; set; }
        public int? LastVersion { get; set; }
        public IEnumerable<object> Events { get; set; }
    }
}