using System;
using MassTransit;

namespace Orchestrator.Service.Contracts.Commands
{
    public class CompleteFundTransferCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}