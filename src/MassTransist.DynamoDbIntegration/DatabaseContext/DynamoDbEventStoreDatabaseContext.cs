using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    public class DynamoDbEventStoreDatabaseContext : IDynamoDbEventStoreDatabaseContext
    {
        public DynamoDbEventStoreDatabaseContext(IAmazonDynamoDB dynamoDb, DynamoDbEventStoreOptions options)
        {
            DynamoDb = dynamoDb ?? throw new ArgumentNullException(nameof(dynamoDb));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IAmazonDynamoDB DynamoDb { get; }
        public DynamoDbEventStoreOptions Options { get; }

        public async Task ConfigureAsync()
        {
            var request = new DynamoDbEventStoreTableRequest(Options);

            await CreateIfNotExist(request, Options.StoreName);
        }

        public async Task<bool> TableExist(string tableName)
        {
            var tables = await DynamoDb.ListTablesAsync();
            var existTable = tables.TableNames.Contains(tableName);
            return existTable;
        }

        private async Task CreateIfNotExist(CreateTableRequest request, string tableName)
        {
            if(await TableExist(tableName)) { return; }

            await DynamoDb.CreateTableAsync(request);
        }
    }
}