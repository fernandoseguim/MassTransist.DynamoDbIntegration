using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    [ExcludeFromCodeCoverage]
    public class DynamoDbEventStoreOptions : AWSOptions
    {
        /// <summary>
        /// Optional: Define a name to event store name. Default value is MassTransist.DynamoDbIntegration.[YOU-SAGA-NAMEOF]
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Optional: Set a billing mode to DynamoDb table usage. Default value as PAY_PER_REQUEST
        /// </summary>
        public BillingMode BillingMode { get; set; } = BillingMode.PAY_PER_REQUEST;
    }
}