using System;
using System.Threading.Tasks;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public interface IRetrieveSagaFromRepository<TSaga> where TSaga: IEventSourcedSaga
    {
        Task<TSaga> GetSaga(Guid correlationId);
    }
}
