using System;
using Automatonymous;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orchestrator.Service.Contracts.Commands;
using Orchestrator.Service.Contracts.Events;
using Orchestrator.Service.Sagas;
using SagaState = Automatonymous.State;

namespace Orchestrator.Service.StateMachines
{
    public sealed class BankDepositTransactionStateMachine : MassTransitStateMachine<BankDepositTransactionInstance>
    {
        private readonly ILogger<BankDepositTransactionStateMachine> _logger;

        public BankDepositTransactionStateMachine(ILogger<BankDepositTransactionStateMachine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Name("BANK_DEPOSIT_TRANSACTION");
            
            InstanceState(saga => saga.CurrentState);
            
            Event(() => FundTransferWasReceived, @event => @event.CorrelateById(context => context.Message.CorrelationId).SelectId(selector => selector.Message.AuthenticationCode));
            Event(() => FundTransferWasApproved, @event => @event.CorrelateById(context => context.Message.CorrelationId));
            Event(() => AmountWasReserved, @event => @event.CorrelateById(context => context.Message.CorrelationId));
            Event(() => FundTransferWasCompleted, @event => @event.CorrelateById(context => context.Message.CorrelationId));
            
            Initially(
                When(FundTransferWasReceived)
                    .Then(context => context.Instance.Apply(context.Data))
                    .PublishAsync(context => context.Init<AnalyzeFundTransferCommand>(new { context.Instance.CorrelationId }))
                    .TransitionTo(AwaitingRiskAnalysis));

            During(AwaitingRiskAnalysis,
                    When(FundTransferWasApproved)
                        .Then(context => context.Instance.Apply(context.Data))
                        .PublishAsync(context => context.Init<ReserveAmountCommand>(new { context.Instance.CorrelationId }))
                        .TransitionTo(AwaitingReserveAmount));

            During(AwaitingReserveAmount,
                When(AmountWasReserved)
                        .Then(context => context.Instance.Apply(context.Data))
                        .PublishAsync(context => context.Init<CompleteFundTransferCommand>(new { context.Instance.CorrelationId }))
                        .TransitionTo(AwaitingCompleteFundTransfer));

            During(AwaitingCompleteFundTransfer,
                When(FundTransferWasCompleted)
                    .Then(context => context.Instance.Apply(context.Data))
                    .Finalize());

            SetCompletedWhenFinalized();
        }
        
        public SagaState AwaitingRiskAnalysis { get; private set; }
        public SagaState AwaitingReserveAmount { get; private set; }
        public SagaState AwaitingCompleteFundTransfer { get; private set; }
        
        public Event<FundTransferWasReceived> FundTransferWasReceived { get; private set; }
        public Event<AmountWasReserved> AmountWasReserved { get; private set; }
        public Event<FundTransferWasApproved> FundTransferWasApproved { get; private set; }
        public Event<FundTransferWasCompleted> FundTransferWasCompleted { get; private set; }
    }
}
