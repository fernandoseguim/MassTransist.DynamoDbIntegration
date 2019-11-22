using System.Collections.Generic;

namespace MassTransist.DynamoDb.EventStore.Saga
{
    public class EventsData
    {
        public IEnumerable<object> Events { get;set; }
        public int? LastVersion { get; set; }
    }
}
