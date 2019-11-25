using System;
using MassTransit;

namespace Orquestrator.Service.Contracts.Commands
{
    public class ReserveCustomerBankDepositLimitAmount : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}