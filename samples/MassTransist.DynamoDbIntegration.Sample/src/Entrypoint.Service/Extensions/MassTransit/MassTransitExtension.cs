using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orquestrator.Service.Contracts;

namespace Entrypoint.Service.Extensions.MassTransit
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

            services.AddMassTransit(configure =>
            {
                configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(configureBus =>
                {
                    configureBus.Host(new Uri(brokerSettings.Host), configureHost =>
                    {
                        configureHost.Username(brokerSettings.User);
                        configureHost.Password(brokerSettings.Password);
                    });

                    configureBus.UseSerilog(demoteDebug: true);
                }));
            });

        }
    }
}
