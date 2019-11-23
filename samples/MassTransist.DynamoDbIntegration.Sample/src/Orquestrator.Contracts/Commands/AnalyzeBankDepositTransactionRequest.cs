using System;
using MassTransit;

namespace Orquestrator.Saga.Contracts.Commands
{
    public class AnalyzeBankDepositTransactionRequest : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
