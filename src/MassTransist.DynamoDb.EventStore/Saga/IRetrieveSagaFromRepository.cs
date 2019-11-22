using System;
using System.Threading.Tasks;

namespace MassTransist.DynamoDb.EventStore.Saga
{
    public interface IRetrieveSagaFromRepository<TSaga> where TSaga: IEventSourcedSaga
    {
        Task<TSaga> GetSaga(Guid correlationId);
    }
}
