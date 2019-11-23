using System;
using MassTransit;

namespace Orquestrator.Saga.Contracts.Events
{
    public class CustomerBankDepositLimitAmountWasReserved : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public long ReservedAmount { get; set; }
    }
}