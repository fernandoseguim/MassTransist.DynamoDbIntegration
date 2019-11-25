using System;
using Automatonymous;
using MassTransist.DynamoDbIntegration.Saga;
using Orquestrator.Service.Contracts.Events;

namespace Orquestrator.Service.Sagas
{
    public class BankDepositTransactionInstance : EventSourcedSagaInstance, SagaStateMachineInstance
    {
        public BankDepositTransactionInstance(Guid correlationId) : this() => CorrelationId = correlationId;

        private BankDepositTransactionInstance()
        {
            Register<BankDepositTransactionWasReceived>(x =>
            {
                CashInTransactionId = x.CashInTransactionId;
                Document = x.Document;
                Company = x.Company;
                BankBranch = x.BankBranch;
                BankAccount = x.BankAccount;
                Amount = x.Amount;
                Channel = x.Channel;
            });
            Register<BankDepositTransactionWasApproved>(x =>
            {
                Status = x.Status;
            });
            Register<ReservedLimitAmountWasMovedToRegularAccount>(x =>
            {
                ReservedAmount = x.ReservedAmount;
            });
        }

        public string Status { get; private set; }
        public string CashInTransactionId { get; private set; }
        public string Document { get; private set; }
        public string Company { get; private set; }
        public string BankBranch { get; private set; }
        public string BankAccount { get; private set; }
        public long Amount { get; private set; }
        public string Channel { get; private set; }
        public long ReservedAmount { get; private set; }
    }
}
