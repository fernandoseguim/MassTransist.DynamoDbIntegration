using System;
using MassTransit;

namespace Orquestrator.Service.Contracts.Events
{
    public class ReservedLimitAmountWasMovedToRegularAccount : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public long ReservedAmount { get; private set; }
    }
}