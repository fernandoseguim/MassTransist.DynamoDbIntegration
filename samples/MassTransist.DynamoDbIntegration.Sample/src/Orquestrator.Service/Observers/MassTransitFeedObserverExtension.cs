using System;
using AutoMapper;
using GreenPipes;
using MassTransit;
using MassTransit.Audit;
using MassTransit.Audit.MetadataFactories;
using MassTransit.Configurators;
using MassTransit.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Orchestrator.Service.Observers
{
    public static class MassTransitFeedObserverExtension
    {
        public static ConnectHandle ConnectAcessoAuditObserver(this IConsumeObserverConnector connector, IServiceProvider provider, Action<IMessageFilterConfigurator> configureFilter = null, IConsumeMetadataFactory metadataFactory = null)
        {
            if(connector == null)
                throw new ArgumentNullException(nameof(connector));
            
            var specification = new ConsumeMessageFilterSpecification();
            configureFilter?.Invoke(specification);

            var factory = metadataFactory ?? new DefaultConsumeMetadataFactory();
            var mapper = provider.GetService<IMapper>();

            return connector.ConnectConsumeObserver(new AcessoAuditEventsObserver(factory, specification.Filter, mapper));
        }
    }
}
