using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MassTransist.DynamoDbIntegration.DatabaseContext;

namespace MassTransist.DynamoDbIntegration.Tests
{
    public class DynamoDbEventStoreFixture : IDisposable
    {
        public DynamoDbEventStoreFixture()
        {
            var amazonDynamoDbConfig = new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" };
            var client = new AmazonDynamoDBClient("root", "secret", amazonDynamoDbConfig);
            Options = new DynamoDbEventStoreOptions{ StoreName = Guid.NewGuid().ToString() };
            var context = new DynamoDbEventStoreDatabaseContext(client, Options);
            context.ConfigureAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Connection = new DynamoDBContext(client);
            OperationConfig = new DynamoDBOperationConfig { OverrideTableName = Options.StoreName, Conversion = DynamoDBEntryConversion.V2 };
        }

        public IDynamoDBContext Connection { get; }

        public DynamoDbEventStoreOptions Options { get; }
        public DynamoDBOperationConfig OperationConfig { get; }

        #region Dispose

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if(_disposed) return;

            if(disposing) Connection.Dispose();

            _disposed = true;
        }

        ~DynamoDbEventStoreFixture()
        {
            Dispose(false);
        }

        #endregion
    }
}