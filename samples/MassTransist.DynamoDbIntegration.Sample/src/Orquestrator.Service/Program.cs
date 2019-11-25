using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace Orquestrator.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            await host.InitAsync();
            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration.Enrich.FromLogContext();
                    configuration.Enrich.WithExceptionDetails();
                    configuration.Enrich.WithMachineName();
                    configuration.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
                    configuration.MinimumLevel.Override("MassTransit", Serilog.Events.LogEventLevel.Debug);
                    configuration.WriteTo.Console();
                })
                .UseStartup<Startup>();
    }
}
