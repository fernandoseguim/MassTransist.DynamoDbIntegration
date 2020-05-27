using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orchestrator.Service.Contracts.Commands;
using Orchestrator.Service.Contracts.Events;

namespace Orchestrator.Service.Consumers
{
    public class AnalyzeBankDepositTransactionRequestConsumer : IConsumer<AnalyzeFundTransferCommand>
    {
        private readonly ILogger<AnalyzeBankDepositTransactionRequestConsumer> _logger;
        public AnalyzeBankDepositTransactionRequestConsumer(ILogger<AnalyzeBankDepositTransactionRequestConsumer> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Consume(ConsumeContext<AnalyzeFundTransferCommand> context)
        {
            _logger.LogInformation($"Request for analyze bank deposit transaction to {context.Message.CorrelationId} was received");
            await context.Publish<FundTransferWasApproved>(new FundTransferWasApproved{ CorrelationId = context.Message.CorrelationId, Date = DateTime.UtcNow } );
        }
    }
}
