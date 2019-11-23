using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AspNetCore.AsyncInitialization;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    [ExcludeFromCodeCoverage]
    public class DynamoDbEventStoreDatabaseContextInitializer : IAsyncInitializer
    {
        private readonly IDynamoDbEventStoreDatabaseContext _databaseContext;

        public DynamoDbEventStoreDatabaseContextInitializer(IDynamoDbEventStoreDatabaseContext databaseContext) => _databaseContext = databaseContext;

        public async Task InitializeAsync() => await _databaseContext.ConfigureAsync();
    }
}