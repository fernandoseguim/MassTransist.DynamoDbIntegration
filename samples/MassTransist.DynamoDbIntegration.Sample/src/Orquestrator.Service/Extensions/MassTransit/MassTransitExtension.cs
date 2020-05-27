using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using AutoMapper;
using MassTransist.DynamoDbIntegration;
using MassTransist.DynamoDbIntegration.Saga;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Service.Consumers;
using Orchestrator.Service.Contracts;
using Orchestrator.Service.Contracts.Events;
using Orchestrator.Service.Sagas;
using Orchestrator.Service.StateMachines;

namespace Orchestrator.Service.Extensions.MassTransit
{
    [ExcludeFromCodeCoverage]
    public static class MassTransitExtension
    {
        public static Dictionary<string, string> BankDepositSaga { get; set; }

        public static void AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            BankDepositSaga = configuration.GetSection("SagasSettings").GetSection("BankDepositSaga").GetChildren()
                    .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                    .ToDictionary(x => x.Key, x => x.Value);

            var brokerSettings = new BrokerSettings();
            configuration.GetSection("BrokerSettings").Bind(brokerSettings);

            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddSingleton<BankDepositTransactionStateMachine>();
            services.RegisterKnownEventsTypes(
                    typeof(BankDepositTransactionInstance),
                    typeof(FundTransferWasApproved),
                    typeof(FundTransferWasReceived),
                    typeof(AmountWasReserved),
                    typeof(FundTransferWasCompleted));

            services.AddV2DynamoDbEventStore<BankDepositTransactionInstance>(storeOptions =>
            {
                storeOptions.BillingMode = BillingMode.PAY_PER_REQUEST;
                storeOptions.Region = RegionEndpoint.USEast1;
                storeOptions.Credentials = new SessionAWSCredentials("", "", "");
            }, config =>
            {
                config.ConsistentRead = true;
                config.Conversion = DynamoDBEntryConversion.V2;
            });

            services.AddMassTransit(configure =>
            {
                configure.AddConsumer<AnalyzeBankDepositTransactionRequestConsumer>();
                configure.AddConsumer<ReserveAmountCommandConsumer>();
                configure.AddConsumer<CompleteFundTransferCommandConsumer>();

                configure.AddSaga<BankDepositTransactionInstance>();
                configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configureBus =>
                {
                    var host = configureBus.Host(new Uri(brokerSettings.Host), configureHost =>
                    {
                        configureHost.Username(brokerSettings.User);
                        configureHost.Password(brokerSettings.Password);
                    });

                    configureBus.ReceiveEndpoint(host, brokerSettings.Input, configureEndpoint =>
                    {
                        var machine = provider.GetService<BankDepositTransactionStateMachine>();
                        var repository = provider.GetService<V2DynamoDbSagaRepository<BankDepositTransactionInstance>>();

                        configureEndpoint.StateMachineSaga(machine, repository);
                    });

                    configureBus.ReceiveEndpoint(host, "reserve_amount", configureEndpoint =>
                    {
                        configureEndpoint.Consumer<ReserveAmountCommandConsumer>(provider);
                    });

                    configureBus.ReceiveEndpoint(host, "analyze_fund_transfer", configureEndpoint =>
                    {
                        configureEndpoint.Consumer<AnalyzeBankDepositTransactionRequestConsumer>(provider);
                    });

                    configureBus.ReceiveEndpoint(host, "complete_fund_transfer", configureEndpoint =>
                    {
                        configureEndpoint.Consumer<CompleteFundTransferCommandConsumer>(provider);
                    });

                    configureBus.UseInMemoryOutbox();
                    configureBus.UseSerilog();
                }));
            });

            services.AddMassTransitHostedService();
        }
    }
}
