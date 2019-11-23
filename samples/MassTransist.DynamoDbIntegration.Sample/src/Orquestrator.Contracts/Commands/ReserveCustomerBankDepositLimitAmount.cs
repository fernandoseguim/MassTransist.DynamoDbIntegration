using System;
using MassTransit;

namespace Orquestrator.Saga.Contracts.Commands
{
    public class ReserveCustomerBankDepositLimitAmount : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}