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
                storeOptions.Credentials = new SessionAWSCredentials("ASIA47PSP45XQ4NUYOOV",
                        "9kSZdUd1392FXQpI6OzwrG20PjYOwBSnPRektAb0",
                        "IQoJb3JpZ2luX2VjEPf//////////wEaCXVzLWVhc3QtMSJGMEQCIB3eB55m1q4Rt0mfHgNV9BUS2kNjia6ryJlJxdPqNiV4AiAXvXy9m+gpWLuWId0S86PZV2pdPrZ/J2Y2pf3JScpVtyqRAggwEAAaDDg5MjI1MTEzNzkwMyIMw1idW+WB4LhpeW7xKu4Be7qqauXRwRxP8Rfx1BUS1SyWACr43zr5WrHetDzttoJdq66+KX7n87ffM4HMERH+yPF6L663//WDDI7dj5GDJalQUTbdnYlb3npRuL3juCv0sngX7gz5XtnJIHJHbiPGr72c6BbKogW7jujdO2e/fwL3mVA2dlJ4czzZ902jQaTs3neozz7aB6/P5jT1Ys9gLeFaAjwjfxKpS68MIGu5Y24n0oUCjm0pOqddB9oSuGLtct9SB6LwDcxJOEnx4uFlyZmGyp0QqGwx8z4dCGvEduJeB14OwwnKlx7qBT2CZtJFlB4Rc54NmJWUcqQ48zCdguXuBTrqAZHu07IdMNaP9mP8msnIVSLbViQHUgyl4h6b0kdLBdQ4xC+y137MY1vHLb1jNoxVer2ikODggzUv4JTlZ0gPSiTNWBTapMmiaDLYCpPe7PQcQK9DFzFFmU/F8RjRxrrHbc39Xl7jyAWNKdC2P+DPfmgX5p1qnwl55mITRsqi+1oOjRBHSHh09y5YrC3H6eMu6P3meKhnn5Kbhpe/4NABwNJ9X1b0KE33vKOAdp9frt54L28EPfr5582iKrSwBTOYXBIQed0Rva0/OhJdsydkQpnpktTInoGqF4Kmt+74wzKGg8StV/jNNZCN2A==");
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
