using System;
using MassTransit;

namespace Orquestrator.Service.Contracts.Commands
{
    public class MoveReservedLimitAmountToRegularAccount : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}