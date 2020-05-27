using System;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using MassTransit.Audit;
using MassTransit.Util;
using MassTransit.Util.Scanning;

namespace Orchestrator.Service.Observers
{
    public class AcessoAuditEventsObserver : IAcessoAuditEventsObserver 
    {
        readonly CompositeFilter<ConsumeContext> _filter;
        readonly IConsumeMetadataFactory _metadataFactory;
        private readonly IMapper _mapper;

        public AcessoAuditEventsObserver(IConsumeMetadataFactory metadataFactory, CompositeFilter<ConsumeContext> filter, IMapper mapper)
        {
            _metadataFactory = metadataFactory;
            _filter = filter;
            _mapper = mapper;
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            if(!_filter.Matches(context))
                return TaskUtil.Completed;

            var metadata = _metadataFactory.CreateAuditMetadata(context);
            var message = context.Message;
            if(context.CorrelationId is null) throw new InvalidOperationException();

            var pacoca = _mapper.Map<IAuditEvent>(message);
            
            return context.Publish<IAuditEvent>(pacoca);
        }

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            var metadata = _metadataFactory.CreateAuditMetadata(context);
            var message = context.Message;
            if(context.CorrelationId is null) throw new InvalidOperationException();

            var pacoca = _mapper.Map<IAuditEvent>(message);
            pacoca.ExceptionMessage = exception.Message;
            pacoca.StackTrace = exception.StackTrace;
            
            return context.Publish<IAuditEvent>(pacoca);
        }
    }
}