using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace MassTransist.DynamoDbIntegration
{
    /// <summary>
    /// Event resolve type cache. Can be used in "default" mode when assembly name
    /// is used to compose the full type name, or in static resolve mode
    /// when you can define own mappings to be more implementation agnostic
    /// </summary>
    public static class TypeMapping
    {
        /// <summary>
        /// Add type to cache
        /// </summary>
        /// <param name="key">Type key</param>
        /// <param name="type">Type</param>
        public static void Add(string key, Type type)
        {
            if (Cached.Instance.ContainsKey(key)) Cached.Instance.TryRemove(key, out _);
            if (Cached.ReverseInstance.ContainsKey(type)) Cached.ReverseInstance.TryRemove(type, out _);

            Cached.Instance.TryAdd(key, type);
            Cached.ReverseInstance.TryAdd(type, key);
        }

        /// <summary>
        /// Add type to cache
        /// </summary>
        /// <param name="key">Type key</param>
        /// <typeparam name="T">Type</typeparam>
        public static void Add<T>(string key) => Add(key, typeof(T));

        /// <summary>
        /// Retrieve type from cache or find using reflections
        /// </summary>
        /// <param name="key">Type key or type name</param>
        /// <param name="knownTypess">Knows types where to search the type in</param>
        /// <returns></returns>
        public static Type Get(string key, IEnumerable<Type> knownTypess)
        {
            if(Cached.Instance.ContainsKey(key)) return Cached.Instance[key];
            
            Type type;
            foreach(var knownTypes in knownTypess)
            {
                var assemblyName = knownTypes.Assembly.GetName().Name;
                type = Type.GetType($"{key}, {assemblyName}");

                if(type is null) continue;

                Add(key, type);

                return type;
            }

            type = Type.GetType($"{key}, {Assembly.GetExecutingAssembly().GetName().Name}");
            Add(key, type);
            return type;
        }

        /// <summary>
        /// Get the type name, either from mapping or full assembly name
        /// </summary>
        /// <param name="type">Type that you need a name for</param>
        /// <returns>Type name string</returns>
        public static string GetTypeName(Type type) => Cached.ReverseInstance.ContainsKey(type) ? Cached.ReverseInstance[type] : type.FullName;

        private static class Cached
        {
            internal static readonly ConcurrentDictionary<string, Type> Instance = new ConcurrentDictionary<string, Type>();

            internal static readonly ConcurrentDictionary<Type, string> ReverseInstance = new ConcurrentDictionary<Type, string>();
        }
    }
}