using System;
using System.Collections;
using System.Collections.Generic;

namespace MassTransist.DynamoDbIntegration
{
    public class KnownEventTypes : IEnumerable<Type>
    {
        private readonly List<Type> _innetKnownTypes;

        public KnownEventTypes() => _innetKnownTypes = new List<Type>();

        public void RegisterTypes(params Type[] types)
        {
            if(types == null) throw new ArgumentNullException(nameof(types));
            _innetKnownTypes.AddRange(types);
        }

        public IEnumerator<Type> GetEnumerator() => _innetKnownTypes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}