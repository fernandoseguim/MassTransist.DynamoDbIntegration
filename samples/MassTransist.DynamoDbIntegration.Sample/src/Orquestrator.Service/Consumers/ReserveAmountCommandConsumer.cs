using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orchestrator.Service.Contracts.Commands;
using Orchestrator.Service.Contracts.Events;

namespace Orchestrator.Service.Consumers
{
    public class ReserveAmountCommandConsumer : IConsumer<ReserveAmountCommand>
    {
        private readonly ILogger<ReserveAmountCommandConsumer> _logger;
        public ReserveAmountCommandConsumer(ILogger<ReserveAmountCommandConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReserveAmountCommand> context)
        {
            _logger.LogInformation($"Request for reserve limit for customer to {context.Message.CorrelationId} was received");
            await context.Publish<AmountWasReserved>(new AmountWasReserved { CorrelationId = context.Message.CorrelationId, ReservedAmount = 1000 } );
        }
    }
}
