using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orquestrator.Saga.Contracts.Commands;
using Orquestrator.Saga.Contracts.Events;

namespace Fraud.Service.Consumers
{
    public class AnalyzeBankDepositTransactionRequestConsumer : IConsumer<AnalyzeBankDepositTransactionRequest>
    {
        private readonly ILogger<AnalyzeBankDepositTransactionRequestConsumer> _logger;
        public AnalyzeBankDepositTransactionRequestConsumer(ILogger<AnalyzeBankDepositTransactionRequestConsumer> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Consume(ConsumeContext<AnalyzeBankDepositTransactionRequest> context)
        {
            _logger.LogInformation($"Request for analyze bank deposit transaction to {context.Message.CorrelationId} was received");
            await context.Publish<BankDepositTransactionWasApproved>(new BankDepositTransactionWasApproved{ CorrelationId = context.Message.CorrelationId, Status = "Approved" } );
        }
    }
}
