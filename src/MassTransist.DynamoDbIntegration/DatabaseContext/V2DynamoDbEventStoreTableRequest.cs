using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    public class V2DynamoDbEventStoreTableRequest : CreateTableRequest
    {
        public V2DynamoDbEventStoreTableRequest(DynamoDbEventStoreOptions options)
        {
            if(options is null) throw new ArgumentNullException(nameof(options));

            TableName = options.StoreName;
            AttributeDefinitions = new List<AttributeDefinition>
            {
                    new AttributeDefinition("AggregateId", ScalarAttributeType.S),
                    new AttributeDefinition("Version", ScalarAttributeType.N)
            };
            KeySchema = new List<KeySchemaElement>
            {
                    new KeySchemaElement("AggregateId", KeyType.HASH),
                    new KeySchemaElement("Version", KeyType.RANGE)
            };
            //TODO: Define stratety to others billing modes
            BillingMode = options.BillingMode;
        }
    }
}