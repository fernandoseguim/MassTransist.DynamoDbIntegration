using System;
using System.Threading.Tasks;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public interface IRetrieveSagaFromRepository<TSaga> where TSaga: IEventSourcedSaga
    {
        Task<TSaga> GetSaga(Guid correlationId);
    }

    public interface IV2RetrieveSagaFromRepository<TSaga> where TSaga: IV2EventSourcedSaga
    {
        Task<TSaga> GetSaga(Guid correlationId);
    }
}
