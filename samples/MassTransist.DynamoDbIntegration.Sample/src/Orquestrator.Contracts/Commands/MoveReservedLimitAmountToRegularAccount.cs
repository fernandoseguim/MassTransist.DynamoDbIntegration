using System;
using MassTransit;

namespace Orquestrator.Saga.Contracts.Commands
{
    public class MoveReservedLimitAmountToRegularAccount : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}