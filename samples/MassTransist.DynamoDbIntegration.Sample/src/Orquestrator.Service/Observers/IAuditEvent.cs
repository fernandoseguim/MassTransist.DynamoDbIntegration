using System;
using MassTransit;

namespace Orchestrator.Service.Observers
{
    public interface IAuditEvent : CorrelatedBy<Guid>
    {
        string AggregateId { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        string Label { get; set; }

        string Category { get; set; }

        string CardId { get; set; }

        string Data { get; set; }

        string DocumentNumber { get; set; }

        string CompanyKey { get; set; }

        DateTime Timestamp { get; set; }

        string BankBranch { get; set; }

        string BankAccount { get; set; }

        Decimal? Amount { get; set; }

        DateTime CreatedAt { get; set; }

        DateTime? UpdatedAt { get; set; }

        EventStatus Status { get; set; }

        EventType Type { get; set; }

        string ExceptionMessage { get; set; }
        string StackTrace { get; set; }
    }

    public enum EventType
    {
        INFO,
        TRANSACTION,
        ACTION,
        BACKOFFICE
    }

    public enum EventStatus
    {
        ACTIVATED,
        CANCELED,
        DELETED,
    }

    public class AuditEvent : IAuditEvent
    {
        public Guid CorrelationId { get; }

        public string AggregateId { get;set; }

        public string Name { get;set; }

        public string Description { get;set; }

        public string Label { get;set; }

        public string Category { get;set; }

        public string CardId { get;set; }

        public string Data { get;set; }

        public string DocumentNumber { get;set; }

        public string CompanyKey { get;set; }

        public DateTime Timestamp { get;set; }

        public string BankBranch { get;set; }

        public string BankAccount { get;set; }

        public decimal? Amount { get;set; }

        public DateTime CreatedAt { get;set; }

        public DateTime? UpdatedAt { get;set; }

        public EventStatus Status { get;set; }

        public EventType Type { get;set; }
        public string ExceptionMessage { get;set; }
        public string StackTrace { get;set; }
    }
}