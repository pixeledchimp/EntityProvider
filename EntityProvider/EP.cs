using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityProvider
{
    public class EP
    {
        /// <summary>
        ///     Entity Provider singleton instance
        /// </summary>
        private static EP _instance = null;

        /// <summary>
        ///     Path to implementation dll
        /// </summary>
        private string _dllLocation;

        /// <summary>
        ///     Implementations namespace
        /// </summary>
        private NameSpace _implementationsNamespace;

        /// <summary>
        ///     Collection that stores singleton objects
        /// </summary>
        private static readonly IDictionary<NameSpace, IDictionary<Type, object>> Singletons = new Dictionary<NameSpace, IDictionary<Type, object>>();

        /// <summary>
        ///     Returns an instance of the passed ScopeType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private T Get<T>(IDictionary<Type, object> collection = null, params object[] args)
        {
            // Return new instance if transient
            if (collection == null) return New<T>(args);

            // Find Object
            if (collection.ContainsKey(typeof(T))) return (T)collection[typeof(T)];
            var obj = New<T>(args);

            // Add to collection if not exists
            collection.Add(typeof(T), obj);
            // Return object
            return obj;
        }

        /// <summary>
        ///     Entity provider private constructor
        /// </summary>
        private EP(string dllLocation, string implementationsNamespace)
        {
            _dllLocation = dllLocation;
            _implementationsNamespace = new NameSpace(implementationsNamespace);
        }

        /// <summary>
        ///     Entity provider instance provider
        /// </summary>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, string implementationsNamespace)
        {
            return new EP(dllLocation, implementationsNamespace);
        }

        /// <summary>
        ///     Returns an instance of the Implementation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        private T New<T>(params object[] args)
        {
            var wantedType = typeof(T);
            var modelType = GetModelTypeOf(wantedType);
            return (T)Activator.CreateInstance(modelType, args);
        }

        /// <summary>
        ///     Returns a singleton instance of the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T GetSingleton<T>(params object[] args)
        {
            if (!Singletons.ContainsKey(_implementationsNamespace))
            {
                Singletons.Add(_implementationsNamespace, new Dictionary<Type, object>());
            }

            return Get<T>(Singletons[_implementationsNamespace], args);
        }

        /// <summary>
        /// Returns a transient instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T GetTransient<T>(params object[] args)
        {
            return Get<T>(args: args);
        }

        /// <summary>
        ///     Returns the Type of the Interface implementation class
        /// </summary>
        /// <param name="wantedType">The interface type</param>
        /// <returns>The implementation class</returns>
        private Type GetModelTypeOf(Type wantedType)
        {
            var modelTypes = GetTypesInNamespace(Assembly.LoadFrom(_dllLocation), _implementationsNamespace.ToString());

            foreach (var t in modelTypes)
            {
                // Looking for an implementation of the requested type and not of the same type.
                if (wantedType.IsAssignableFrom(t) && wantedType != t)
                {
                    return t;
                }
            }

            throw new NotImplementedException($"The requested Type [{wantedType}] has not been found.");
        }

        /// <summary>
        ///     Returns an array of Types defined in a namespace
        /// </summary>
        /// <param name="assembly">The Assembly that contains the Interface Implementations</param>
        /// <param name="nameSpace">The namespace where the implementations are defined</param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public Scope GetScope()
            => new Scope(_dllLocation, _implementationsNamespace.ToString());

        public class Scope
        {
            private EP _sep;

            public Scope(string dllLocation, string implementationNamespace)
            {
                _sep = GetProvider(dllLocation, implementationNamespace);
            }

            /// <summary>
            ///     Returns a scoped instance of the object
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="args"></param>
            /// <returns></returns>
            public T Get<T>(params object[] args)
            {
                return _sep.Get<T>(_scoped, args);
            }

            /// <summary>
            ///     Collection that stores scoped objects
            /// </summary>
            private readonly IDictionary<Type, object> _scoped = new Dictionary<Type, object>();
        }

        private struct NameSpace : IEquatable<NameSpace>
        {
            private string _namespace;

            public NameSpace(string @namespace)
            {
                _namespace = @namespace;
            }

            public override string ToString()
            {
                return _namespace.Trim();
            }

            public bool Equals(NameSpace other)
            {
                return ToString().Equals(other.ToString());
            }
        }
    }
}
