using System;
using MassTransit;

namespace Orchestrator.Service.Contracts.Commands
{
    public class ReserveAmountCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}