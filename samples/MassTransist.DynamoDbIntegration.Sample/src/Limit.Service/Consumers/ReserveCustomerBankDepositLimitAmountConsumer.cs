using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orquestrator.Service.Contracts.Commands;
using Orquestrator.Service.Contracts.Events;

namespace Limit.Service.Consumers
{
    public class ReserveCustomerBankDepositLimitAmountConsumer : IConsumer<ReserveCustomerBankDepositLimitAmount>
    {
        private readonly ILogger<ReserveCustomerBankDepositLimitAmountConsumer> _logger;
        public ReserveCustomerBankDepositLimitAmountConsumer(ILogger<ReserveCustomerBankDepositLimitAmountConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReserveCustomerBankDepositLimitAmount> context)
        {
            _logger.LogInformation($"Request for reserve limit for customer to {context.Message.CorrelationId} was received");
            await context.Publish<CustomerBankDepositLimitAmountWasReserved>(new CustomerBankDepositLimitAmountWasReserved { CorrelationId = context.Message.CorrelationId, ReservedAmount = 1000 } );
        }
    }
}
