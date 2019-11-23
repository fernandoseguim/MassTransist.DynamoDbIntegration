using System.Collections.Generic;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class EventsData
    {
        public IEnumerable<object> Events { get;set; }
        public int? LastVersion { get; set; }
    }
}
