using System;
using Automatonymous;
using Microsoft.Extensions.Logging;
using Orquestrator.Service.Contracts.Commands;
using Orquestrator.Service.Contracts.Events;
using Orquestrator.Service.Sagas;
using SagaState = Automatonymous.State;

namespace Orquestrator.Service.StateMachines
{
    public sealed class BankDepositTransactionStateMachine : MassTransitStateMachine<BankDepositTransactionInstance>
    {
        private readonly ILogger<BankDepositTransactionStateMachine> _logger;

        public BankDepositTransactionStateMachine(ILogger<BankDepositTransactionStateMachine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Name("BANK_DEPOSIT_TRANSACTION");
            InstanceState(saga => saga.CurrentState);

            Event(() => BankDepositTransactionWasReceived, @event => @event.CorrelateById(context => context.Message.CorrelationId).SelectId(selector => selector.Message.CorrelationId));
            Event(() => BankDepositTransactionWasApproved, @event => @event.CorrelateById(context => context.Message.CorrelationId));
            Event(() => CustomerBankDepositLimitAmountWasReserved, @event => @event.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ReservedLimitAmountWasMovedToRegularAccount, @event => @event.CorrelateById(context => context.Message.CorrelationId));
            
            Initially(
                When(BankDepositTransactionWasReceived)
                    .Then(context => context.Instance.Apply(context.Data))
                    .Then(context => _logger.LogInformation($"Bank deposit transaction to {context.Data.CorrelationId} was received"))
                    .ThenAsync(async context =>
                    {
                        var endpoint = await context.GetSendEndpoint(new Uri("rabbitmq://localhost/core-antifraud-queue"));
                        await endpoint.Send<AnalyzeBankDepositTransactionRequest>(new AnalyzeBankDepositTransactionRequest {CorrelationId = context.Data.CorrelationId});
                    })
                    .TransitionTo(Processing)
                    .TransitionTo(AwaitingRiskAnalysis));

            During(Processing, AwaitingRiskAnalysis,
                    When(BankDepositTransactionWasApproved)
                        .Then(context => context.Instance.Apply(context.Data))
                        .Then(context => _logger.LogInformation($"Bank deposit transaction to {context.Data.CorrelationId} was approved"))
                        .ThenAsync(async context =>
                        {
                            var endpoint = await context.GetSendEndpoint(new Uri("rabbitmq://localhost/core-limit-queue"));
                            await endpoint.Send<ReserveCustomerBankDepositLimitAmount>(new ReserveCustomerBankDepositLimitAmount{ CorrelationId = context.Data.CorrelationId });
                        })
                        .TransitionTo(AwaitingLimitReserve));

            During(Processing, AwaitingLimitReserve,
                When(CustomerBankDepositLimitAmountWasReserved)
                        .Then(context => context.Instance.Apply(context.Data))
                        .Then(context => _logger.LogInformation($"Bank deposit amount limit to {context.Data.CorrelationId} was reserved"))
                        .ThenAsync(async context =>
                        {
                            var endpoint = await context.GetSendEndpoint(new Uri("rabbitmq://localhost/core-account-queue"));
                            await endpoint.Send<MoveReservedLimitAmountToRegularAccount>(new MoveReservedLimitAmountToRegularAccount { CorrelationId = context.Data.CorrelationId });
                        })
                        .TransitionTo(AwaitingMovingAmountToRegularAccount));

            During(Processing, AwaitingMovingAmountToRegularAccount,
                When(ReservedLimitAmountWasMovedToRegularAccount)
                    .Then(context => context.Instance.Apply(context.Data))
                    .Then(context => _logger.LogInformation($"Reserved limit amount to {context.Data.CorrelationId} was moved to regular account"))
                    .Finalize());

            //SetCompletedWhenFinalized();
        }

        
        //public SagaState Processing { get; private set; }
        public SagaState Processing { get; private set; }
        public SagaState Done { get; private set; }

        public SagaState AwaitingRiskAnalysis { get; private set; }
        public SagaState AwaitingLimitReserve { get; private set; }
        public SagaState AwaitingMovingAmountToRegularAccount { get; private set; }
        
        public Event<BankDepositTransactionWasReceived> BankDepositTransactionWasReceived { get; private set; }
        public Event<BankDepositTransactionWasApproved> BankDepositTransactionWasApproved { get; private set; }
        public Event<CustomerBankDepositLimitAmountWasReserved> CustomerBankDepositLimitAmountWasReserved { get; private set; }
        public Event<ReservedLimitAmountWasMovedToRegularAccount> ReservedLimitAmountWasMovedToRegularAccount { get; private set; }
    }
}
