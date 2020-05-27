using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    [ExcludeFromCodeCoverage]
    public class V2DynamoDbEventStoreDatabaseContextInitializer : IAsyncInitializer
    {
        private readonly IDynamoDbEventStoreDatabaseContext _databaseContext;

        public V2DynamoDbEventStoreDatabaseContextInitializer(IDynamoDbEventStoreDatabaseContext databaseContext) => _databaseContext = databaseContext;

        public async Task InitializeAsync() => await _databaseContext.ConfigureAsync();
    }
}