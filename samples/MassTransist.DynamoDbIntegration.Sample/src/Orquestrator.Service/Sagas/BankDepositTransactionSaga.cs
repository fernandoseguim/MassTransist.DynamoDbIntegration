using System;
using Automatonymous;
using MassTransist.DynamoDbIntegration.Saga;
using Orchestrator.Service.Contracts.Events;

namespace Orchestrator.Service.Sagas
{
    public class BankDepositTransactionInstance : V2EventSourcedSagaInstance, SagaStateMachineInstance
    {
        public BankDepositTransactionInstance(Guid correlationId) : this() => CorrelationId = correlationId;

        private BankDepositTransactionInstance()
        {
            Register<FundTransferWasReceived>(x =>
            {
                AuthenticationCode = x.AuthenticationCode;
                Document = x.Document;
                Company = x.Company;
                BankBranch = x.BankBranch;
                BankAccount = x.BankAccount;
                Amount = x.Amount;
                Channel = x.Channel;
            });
            Register<FundTransferWasApproved>(x =>
            {
                StartedAt = x.Date;
            });

            Register<AmountWasReserved>(x =>
            {
                ReservedAmount = x.ReservedAmount;
            });

            Register<FundTransferWasCompleted>(x =>
            {
                EndedAt = x.Date;
            });
        }

        public DateTime StartedAt { get; private set; }
        public DateTime EndedAt { get; private set; }
        public Guid AuthenticationCode { get; private set; }
        public string Document { get; private set; }
        public string Company { get; private set; }
        public string BankBranch { get; private set; }
        public string BankAccount { get; private set; }
        public long Amount { get; private set; }
        public string Channel { get; private set; }
        public long ReservedAmount { get; private set; }
    }
}
