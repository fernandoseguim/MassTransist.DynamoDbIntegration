using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Orchestrator.Service.Extensions.Swagger
{
    [ExcludeFromCodeCoverage]
    public static class SwaggerMiddlewareExtension
    {
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder builder, IApiVersionDescriptionProvider versionProvider)
        {
            builder.UseSwagger(o => o.RouteTemplate = "docs/{documentName}/swagger.json");

            builder.UseSwaggerUI(options =>
            {

                foreach(var description in versionProvider.ApiVersionDescriptions)
                {
                    options.RoutePrefix = "docs";
                    options.SwaggerEndpoint($"../docs/{description.GroupName}/swagger.json", description.GroupName);
                }
            });

            return builder;
        }
    }
}
