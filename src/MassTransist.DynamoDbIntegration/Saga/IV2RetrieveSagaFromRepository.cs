using System;
using System.Threading.Tasks;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public interface IV2RetrieveSagaFromRepository<TSaga> where TSaga: IV2EventSourcedSaga
    {
        Task<TSaga> GetSaga(Guid correlationId);
    }
}