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
        private readonly static EP _instance = null;

        /// <summary>
        ///     Path to implementation dll
        /// </summary>
        private readonly string _dllLocation;

        /// <summary>
        ///     Implementations namespace
        /// </summary>
        private NameSpace _implementationsNamespace;

        /// <summary>
        ///     Collection that stores singleton objects
        /// </summary>
        private static readonly IDictionary<string, IDictionary<NameSpace, IDictionary<Type, object>>> Singletons = new Dictionary<string, IDictionary<NameSpace, IDictionary<Type, object>>>();

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
            try
            {
                return new EP(dllLocation, implementationsNamespace);
            }
            catch (Exception e)
            {

                throw e;
            }
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
            try
            {
                if (!Singletons.ContainsKey(_dllLocation))
                {
                    Singletons.Add(_dllLocation, new Dictionary<NameSpace, IDictionary<Type, object>>());
                }

                if (!Singletons[_dllLocation].ContainsKey(_implementationsNamespace))
                {
                    Singletons[_dllLocation].Add(_implementationsNamespace, new Dictionary<Type, object>());
                }

                return Get<T>(Singletons[_dllLocation][_implementationsNamespace], args);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        /// <summary>
        /// Returns a transient instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T GetTransient<T>(params object[] args)
        {
            try
            {
                return Get<T>(args: args);
            }
            catch (Exception e)
            {

                throw e;
            }
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

        /// <summary>
        ///     Returns a Scope
        /// </summary>
        /// <returns></returns>
        public Scope GetScope()
        {
            try
            {
                return new Scope(_dllLocation, _implementationsNamespace.ToString());
            }
            catch (Exception e)
            {

                throw e;
            }
        }

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
                try
                {
                    return _sep.Get<T>(_scoped, args);
                }
                catch (Exception e)
                {

                    throw e;
                }
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
