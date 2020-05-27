using System;
using MassTransit;

namespace Orchestrator.Service.Contracts.Commands
{
    public class AnalyzeFundTransferCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}
