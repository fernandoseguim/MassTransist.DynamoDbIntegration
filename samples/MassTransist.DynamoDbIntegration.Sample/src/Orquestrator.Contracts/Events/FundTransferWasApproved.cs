using System;
using MassTransit;

namespace Orchestrator.Service.Contracts.Events
{
    public class FundTransferWasApproved : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public DateTime Date { get; set; }
    }
}
