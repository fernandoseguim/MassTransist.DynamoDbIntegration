using System.Collections.Generic;
using System.IO;
using System.Text;
using MassTransist.DynamoDb.EventStore.Saga;
using Newtonsoft.Json;

namespace MassTransist.DynamoDb.EventStore
{
    public static class JsonSerialization
    {
        public static IEnumerable<object> Deserialize(EventModel eventModel, string assemblyName)
        {
            var type = TypeMapping.Get(eventModel.Type, assemblyName);
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
    }
}
