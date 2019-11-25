using System;
using MassTransit;

namespace Orquestrator.Service.Contracts.Commands
{
    public class AnalyzeBankDepositTransactionRequest : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
