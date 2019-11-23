using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orquestrator.Saga.Contracts.Commands;
using Orquestrator.Saga.Contracts.Events;

namespace Clearing.Service.Consumers
{
    public class MoveReservedLimitAmountToRegularAccountConsumer : IConsumer<MoveReservedLimitAmountToRegularAccount>
    {
        private readonly ILogger<MoveReservedLimitAmountToRegularAccountConsumer> _logger;
        public MoveReservedLimitAmountToRegularAccountConsumer(ILogger<MoveReservedLimitAmountToRegularAccountConsumer> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Consume(ConsumeContext<MoveReservedLimitAmountToRegularAccount> context)
        {
            _logger.LogInformation($"Request to move reserved limit to regular account to {context.Message.CorrelationId} was received");
            await context.Publish<ReservedLimitAmountWasMovedToRegularAccount>(new ReservedLimitAmountWasMovedToRegularAccount(){ CorrelationId = context.Message.CorrelationId });
        }
    }
}
