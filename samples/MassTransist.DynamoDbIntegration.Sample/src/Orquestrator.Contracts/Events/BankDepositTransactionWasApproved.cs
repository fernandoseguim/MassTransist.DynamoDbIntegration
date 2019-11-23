using System;
using MassTransit;

namespace Orquestrator.Saga.Contracts.Events
{
    public class BankDepositTransactionWasApproved : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Status { get; set; }
    }
}
