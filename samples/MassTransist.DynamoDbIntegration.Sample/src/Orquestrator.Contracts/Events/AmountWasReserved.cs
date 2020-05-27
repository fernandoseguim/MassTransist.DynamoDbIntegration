using System;
using MassTransit;

namespace Orchestrator.Service.Contracts.Events
{
    public class AmountWasReserved : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public long ReservedAmount { get; set; }
    }
}