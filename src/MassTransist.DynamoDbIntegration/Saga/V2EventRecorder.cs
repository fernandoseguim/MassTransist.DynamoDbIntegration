using System;
using System.Collections;
using System.Collections.Generic;

namespace MassTransist.DynamoDbIntegration.Saga
{
    public class V2EventRecorder : IEnumerable<V2EventStoreModel>
    {
        private readonly List<V2EventStoreModel> _recorded;

        /// <summary>
        /// Initializes a new instance of the <see cref="V2EventRecorder"/> class.
        /// </summary>
        public V2EventRecorder() => _recorded = new List<V2EventStoreModel>();

        /// <summary>
        /// Records that the specified event happened.
        /// </summary>
        /// <param name="event">The event to record.</param>
        /// <exception cref="ArgumentNullException">Thrown when the specified <paramref name="event"/> is <c>null</c>.</exception>
        public void Record(V2EventStoreModel @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            _recorded.Add(@event);
        }

        /// <summary>
        /// Resets this instance to its initial state.
        /// </summary>
        public void Reset() => _recorded.Clear();

        /// <summary>
        /// Gets an enumeration of recorded events.
        /// </summary>
        /// <returns>The recorded event enumerator.</returns>
        public IEnumerator<V2EventStoreModel> GetEnumerator() => _recorded.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}