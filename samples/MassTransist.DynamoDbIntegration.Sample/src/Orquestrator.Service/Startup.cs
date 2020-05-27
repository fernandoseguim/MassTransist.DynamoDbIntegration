using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Service.Extensions.MassTransit;
using Orchestrator.Service.Extensions.Swagger;
using Orchestrator.Service.Extensions.Versioning;

namespace Orchestrator.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMassTransitWithRabbitMq(Configuration);
            services.AddVersioning();
            services.AddSwaggerDocumentation();
            //services.AddTransient<IAnalyzeBankDepositTransactionPublisher, AnalyzeBankDepositTransactionPublisher>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider versionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(versionDescriptionProvider);
            app.UseMvc();
        }
    }
}
