using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public abstract class V2EventSourcedSagaInstance : IV2EventSourcedSaga
    {
        public string AggregateId => CorrelationId.ToString();
        public Guid CorrelationId { get; set; }

        public int? ExpectedVersion { get; set; }

        public string StreamName => TypeMapping.GetTypeName(GetType()) + "-" + CorrelationId.ToString("N");

        private string _currentState;

        public string CurrentState
        {
            get => _currentState;
            set => Apply(new SagaInstanceTransitioned
            {
                InstanceId = CorrelationId,
                CurrentState = value
            });
        }

        private readonly V2EventRecorder _recorder;
        private readonly V2EventRouter _router;

        protected void UpVersion() => SetVersion(ExpectedVersion + 1 ?? 0);

        public void SetVersion(int? version)
        {
            if(version < ExpectedVersion) throw new ArgumentOutOfRangeException();
            ExpectedVersion = version;
        }
        
        protected V2EventSourcedSagaInstance()
        {
            _router = new V2EventRouter();
            _recorder = new V2EventRecorder();
            ExpectedVersion = null;
            Register<SagaInstanceTransitioned>(x => _currentState = x.CurrentState);
        }

        /// <summary>
        /// Registers the state handler to be invoked when the specified event is applied.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to register the handler for.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="handler"/> is null.</exception>
        protected void Register<TEvent>(Action<TEvent> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _router.ConfigureRoute(handler);
        }

        /// <summary>
        /// Applies the specified event to this instance and invokes the associated state handler.
        /// </summary>
        /// <param name="event">The event to apply.</param>
        public void Apply(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            UpVersion();
            Play(@event);
            Record(@event);
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Initializes this instance using the specified events.
        /// </summary>
        /// <param name="events">The events to initialize with.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the <paramref name="events" /> are null.</exception>
        public void Initialize(IEnumerable<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (HasChanges())
                throw new InvalidOperationException("Initialize cannot be called on an instance with changes.");

            foreach (var @event in events)
            {
                Play(@event);
            }
        }

        /// <summary>
        /// Determines whether this instance has state changes.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has state changes; otherwise, <c>false</c>.
        /// </returns>
        public bool HasChanges() => _recorder.Any();

        /// <summary>
        /// Gets the state changes applied to this instance.
        /// </summary>
        /// <returns>A list of recorded state changes.</returns>
        public V2EventStoreModel[] GetChanges() => _recorder.ToArray();

        /// <summary>
        /// Clears the state changes.
        /// </summary>
        public void ClearChanges() => _recorder.Reset();

        public bool Finalized() => CurrentState == "Final";

        private void Play(object @event) => _router.Route(@event);

        private void Record(object @event)
        {
            var model = new V2EventStoreModel(AggregateId, ExpectedVersion, TypeMapping.GetTypeName(@event.GetType()), DateTime.UtcNow, JsonConvert.SerializeObject(@event));
            _recorder.Record(model);
        }

        public class SagaInstanceTransitioned
        {
            public Guid InstanceId { get; set; }
            public string CurrentState { get; set; }
        }
    }
}