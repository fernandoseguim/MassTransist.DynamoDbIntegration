using System;
using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransist.DynamoDb.EventStore
{
    [ExcludeFromCodeCoverage]
    public static class EventStoreServiceExtensions
    {
        public static void AddEventStore(this IServiceCollection services, Action<EventStoreOptions> configure = null)
        {
            var options = new EventStoreOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            //services.AddAWSService<IAmazonDynamoDB>(options);

            var client = new AmazonDynamoDBClient(options.Credentials, options.Region);

            services.AddSingleton<IDynamoDBContext>(new DynamoDBContext(client));
        }

    }
}
