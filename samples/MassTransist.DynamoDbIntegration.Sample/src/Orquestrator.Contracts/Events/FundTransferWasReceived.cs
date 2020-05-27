using System;
using MassTransit;

namespace Orchestrator.Service.Contracts.Events
{
    public class FundTransferWasReceived : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public Guid AuthenticationCode { get; set; }
        public string Document { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string BankBranch { get; set; }
        public string BankAccount { get; set; }
        public long Amount { get; set; }
        public string Channel { get; set; }

        public DateTime CreatedAt { get;set; } = DateTime.UtcNow;
    }
}