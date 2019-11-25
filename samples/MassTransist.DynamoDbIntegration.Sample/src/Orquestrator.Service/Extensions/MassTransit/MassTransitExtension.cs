using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using MassTransist.DynamoDbIntegration;
using MassTransist.DynamoDbIntegration.Saga;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orquestrator.Saga.Contracts;
using Orquestrator.Saga.Contracts.Events;
using Orquestrator.Saga.Sagas;
using Orquestrator.Saga.StateMachines;

namespace Orquestrator.Saga.Extensions.MassTransit
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

            services.AddSingleton<BankDepositTransactionStateMachine>();
            services.RegisterKnownEventsTypes(
                    typeof(BankDepositTransactionInstance),
                    typeof(BankDepositTransactionWasApproved),
                    typeof(BankDepositTransactionWasReceived),
                    typeof(ReservedLimitAmountWasMovedToRegularAccount),
                    typeof(CustomerBankDepositLimitAmountWasReserved));

            services.AddDynamoDbEventStore<BankDepositTransactionInstance>(storeOptions =>
            {
                storeOptions.BillingMode = BillingMode.PAY_PER_REQUEST;
                storeOptions.Region = RegionEndpoint.USEast1;
                storeOptions.Credentials = new SessionAWSCredentials("AWS_ACCESS_KEY", "AWS_SECRET_KEY", "AWS_SESSION_TOKEN");
            });

            services.AddMassTransit(configure =>
            {
                configure.AddSaga<BankDepositTransactionInstance>();
                configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configureBus =>
                {
                    var host = configureBus.Host(new Uri(brokerSettings.Host), configureHost =>
                    {
                        configureHost.Username(brokerSettings.User);
                        configureHost.Password(brokerSettings.Password);
                    });


                    configureBus.ReceiveEndpoint(host, brokerSettings.InputQueue, configureEndpoint =>
                    {
                        var machine = provider.GetService<BankDepositTransactionStateMachine>();
                        var repository = provider.GetService<DynamoDbSagaRepository<BankDepositTransactionInstance>>();
                        
                        configureEndpoint.StateMachineSaga(machine, repository);
                    });

                    configureBus.UseInMemoryOutbox();
                    configureBus.UseSerilog();
                }));
            });

            services.AddMassTransitHostedService();
        }
    }
}
