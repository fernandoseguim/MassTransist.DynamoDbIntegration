using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    public class DynamoDbEventStoreTableRequest : CreateTableRequest
    {
        public DynamoDbEventStoreTableRequest(DynamoDbEventStoreOptions options)
        {
            if(options is null) throw new ArgumentNullException(nameof(options));

            TableName = options.StoreName;
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("CorrelationId", ScalarAttributeType.S)
            };
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("CorrelationId", KeyType.HASH)
            };
            //TODO: Define stratety to others billing modes
            BillingMode = options.BillingMode;
        }
    }
}