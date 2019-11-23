using System.Threading.Tasks;

namespace MassTransist.DynamoDbIntegration.DatabaseContext
{
    public interface IDynamoDbEventStoreDatabaseContext
    {
        Task ConfigureAsync();

        Task<bool> TableExist(string tableName);
    }
}