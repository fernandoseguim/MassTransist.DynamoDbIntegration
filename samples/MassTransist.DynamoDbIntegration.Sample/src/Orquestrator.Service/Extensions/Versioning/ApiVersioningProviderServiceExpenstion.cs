using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Orchestrator.Service.Extensions.Versioning
{
    [ExcludeFromCodeCoverage]
	public static class ApiVersioningProviderServiceExpenstion
    {
        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddApiVersioning(options => { options.ReportApiVersions = true; });
            return services;
        }
    }
}
