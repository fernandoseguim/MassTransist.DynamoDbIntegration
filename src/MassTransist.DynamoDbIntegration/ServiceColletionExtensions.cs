using System;
using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using GreenPipes.Internals.Extensions;
using MassTransist.DynamoDbIntegration.DatabaseContext;
using MassTransist.DynamoDbIntegration.Saga;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransist.DynamoDbIntegration
{
    [ExcludeFromCodeCoverage]
    public static class ServiceColletionExtensions
    {
        /// <summary>
        /// Configure DynamoDb saga repository 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        public static void AddDynamoDbEventStore<TSaga>(this IServiceCollection services, Action<DynamoDbEventStoreOptions> configure = null) where TSaga : class, IEventSourcedSaga
        {
            var options = new DynamoDbEventStoreOptions();
            configure?.Invoke(options);

            var name = typeof(TSaga).Name;
            if(options.StoreName is null) { options.StoreName = $"MassTransist.DynamoDbIntegration.{name}"; }
            
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            var knownEventTypes = provider.GetService<KnownEventTypes>();

            if(knownEventTypes is null) throw new InvalidOperationException("Known events types should be registered before register saga repository. Consider use RegisterKnownEventsTypes(params Type[] knownEvetTypes)");

            services.AddAWSService<IAmazonDynamoDB>(options);
            
            services.AddSingleton<IDynamoDbEventStoreDatabaseContext, DynamoDbEventStoreDatabaseContext>();
            services.AddAsyncInitializer<DynamoDbEventStoreDatabaseContextInitializer>();

            var client = services.BuildServiceProvider().GetService<IAmazonDynamoDB>();
            var repository = new DynamoDbSagaRepository<TSaga>(new DynamoDBContext(client), options, knownEventTypes);
            services.AddSingleton<DynamoDbSagaRepository<TSaga>>(repository);
        }

        /// <summary>
        /// Configure DynamoDb saga repository 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <param name="configureContext"></param>
        public static void AddV2DynamoDbEventStore<TSaga>(this IServiceCollection services,
                                                          Action<DynamoDbEventStoreOptions> configureOptions = null,
                                                          Action<DynamoDBContextConfig> configureContext = null) where TSaga : class, IV2EventSourcedSaga
        {
            var options = new DynamoDbEventStoreOptions();
            configureOptions?.Invoke(options);

            var name = typeof(TSaga).Name;
            if(options.StoreName is null) { options.StoreName = $"MassTransist.DynamoDbIntegration.{name}"; }

            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            var knownEventTypes = provider.GetService<KnownEventTypes>();

            if(knownEventTypes is null) throw new InvalidOperationException("Known events types should be registered before register saga repository. Use RegisterKnownEventsTypes(params Type[] knownEventTypes)");

            services.AddAWSService<IAmazonDynamoDB>(options);

            services.AddSingleton<IDynamoDbEventStoreDatabaseContext, V2DynamoDbEventStoreDatabaseContext>();
            services.AddAsyncInitializer<V2DynamoDbEventStoreDatabaseContextInitializer>();

            var client = services.BuildServiceProvider().GetService<IAmazonDynamoDB>();

            var configuration = new DynamoDBContextConfig();
            configureContext?.Invoke(configuration);

            var repository = new V2DynamoDbSagaRepository<TSaga>(new DynamoDBContext(client, configuration), options, knownEventTypes);
            services.AddSingleton<V2DynamoDbSagaRepository<TSaga>>(repository);
        }

        /// <summary>
        /// Register known events types that will be used during saga orquestration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="knownEvetTypes"></param>
        public static void RegisterKnownEventsTypes(this IServiceCollection services, params Type[] knownEventTypes)
        {
            var knownTypes = new KnownEventTypes();
            knownTypes.RegisterTypes(knownEventTypes);
            services.AddSingleton<KnownEventTypes>(knownTypes);
        }
    }
}
