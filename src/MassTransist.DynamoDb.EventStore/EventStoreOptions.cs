using System.Diagnostics.CodeAnalysis;
using Amazon.Extensions.NETCore.Setup;

namespace MassTransist.DynamoDb.EventStore
{
    [ExcludeFromCodeCoverage]
    public class EventStoreOptions : AWSOptions
    {
        public string StoreName { get; set; }
    }
}