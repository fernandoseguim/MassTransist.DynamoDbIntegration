using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orchestrator.Service.Contracts.Commands;
using Orchestrator.Service.Contracts.Events;

namespace Orchestrator.Service.Consumers
{
    public class CompleteFundTransferCommandConsumer : IConsumer<CompleteFundTransferCommand>
    {
        private readonly ILogger<CompleteFundTransferCommandConsumer> _logger;
        public CompleteFundTransferCommandConsumer(ILogger<CompleteFundTransferCommandConsumer> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Consume(ConsumeContext<CompleteFundTransferCommand> context)
        {
            _logger.LogInformation($"Request to move reserved limit to regular account to {context.Message.CorrelationId} was received");
            await context.Publish<FundTransferWasCompleted>(new FundTransferWasCompleted{ CorrelationId = context.Message.CorrelationId, Date = DateTime.UtcNow });
        }
    }
}
