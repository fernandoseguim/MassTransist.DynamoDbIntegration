using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MassTransist.DynamoDbIntegration.Saga;
using Newtonsoft.Json;

namespace MassTransist.DynamoDbIntegration
{
    public static class JsonSerialization
    {
        public static IEnumerable<object> Deserialize(EventModel eventModel, IEnumerable<Type> knownTypes)
        {
            var type = TypeMapping.Get(eventModel.Type, knownTypes);
            if(type == null)
            {
                yield return null;
                yield break;
            }

            using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(eventModel.Data)))
                using(var reader = new StreamReader(stream))
                {
                    yield return JsonSerializer.CreateDefault().Deserialize(reader, type);
                }
        }

        public static IEnumerable<object> Deserialize(V2EventStoreModel model, IEnumerable<Type> knownTypes)
        {
            var type = TypeMapping.Get(model.Name, knownTypes);
            if(type == null)
            {
                yield return null;
                yield break;
            }

            using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(model.Data)))
                using(var reader = new StreamReader(stream))
                {
                    yield return JsonSerializer.CreateDefault().Deserialize(reader, type);
                }
        }
    }
}
