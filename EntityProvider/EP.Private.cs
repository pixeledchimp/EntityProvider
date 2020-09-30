using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace EntityProvider
{
    public sealed partial class EP
    {
        #region PrivateInterface
        /// <summary>
        ///     Tells if Type is in the collection destinated to be singleton
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private bool InSingletonTypes<T>()
        {
            return _singletonTypes.ContainsKey(typeof(T).Namespace)
                && _singletonTypes[typeof(T).Namespace].Contains(typeof(T).Name);
        }

        /// <summary>
        ///     Returns the Type of the Interface implementation class
        /// </summary>
        /// <param name="wantedType">The interface type</param>
        /// <returns>The implementation class</returns>
        private Type GetModelTypeOf(Type wantedType)
        {
            var assembly = GetAssembly(wantedType);
            var modelTypes = GetModelTypes(assembly);
            var foundType = FindImplementation(wantedType, modelTypes);

            return foundType ?? throw new NotImplementedException($"The requested Type [{wantedType}] has not been found.");
        }

        /// <summary>
        ///      Tries to get the implementation of the interface among the available assemblies.
        ///      First using the strong maps if available then among the available types.
        /// </summary>
        /// <param name="wantedType"></param>
        /// <param name="modelTypes"></param>
        /// <returns></returns>
        private Type? FindImplementation(Type wantedType, IEnumerable<Type> modelTypes)
        {
            return GetImplementationUsingStrongMaps(wantedType, modelTypes) ?? GetImplementationFromModelTypes(wantedType, modelTypes);
        }

        /// <summary>
        ///     Tries to get the implementation from the types found.
        /// </summary>
        /// <param name="wantedType"></param>
        /// <param name="modelTypes"></param>
        /// <returns></returns>
        private static Type? GetImplementationFromModelTypes(Type wantedType, IEnumerable<Type> modelTypes)
        {
            return modelTypes.FirstOrDefault(t => wantedType.IsAssignableFrom(t) && wantedType != t);
        }

        /// <summary>
        ///     Tries to get the implementation from the strong maps
        /// </summary>
        /// <param name="wantedType"></param>
        /// <param name="modelTypes"></param>
        /// <returns></returns>
        private Type? GetImplementationUsingStrongMaps(Type wantedType, IEnumerable<Type> modelTypes)
        {
            if (_strongMaps == null || _strongMaps.Count <= 0) return null;

            if (!_strongMaps.ContainsKey(wantedType.ToString())) return null;

            return modelTypes.FirstOrDefault(t => t.FullName == _strongMaps[wantedType.ToString()]) ?? throw new NotImplementedException(@"The required type has not been found where the configuration expected it to be.");
        }

        private IEnumerable<Type> GetModelTypes(Assembly assembly)
        {
            return _implementationsNamespace != null
                ? GetTypesInNamespace(assembly, _implementationsNamespace)
                : assembly.GetTypes();
        }

        private Assembly GetAssembly(Type wantedType)
        {
            return _callingAssenbly != null
                            ? _callingAssenbly
                            : _dllLocation == null
                                ? Assembly.GetAssembly(wantedType)
                                : GetAssemblyIfLoaded(_dllLocation) ?? Assembly.LoadFrom(_dllLocation);
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
        ///     Returns an instance of the passed ScopeType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal T Get<T>(IDictionary<Type, object> collection = null, params object[] args)
        {
            // Return new instance if transient
            if (collection == null) return GetNewInstance<T>(args);

            // Find Object
            if (collection.ContainsKey(typeof(T))) return (T)collection[typeof(T)];
            var obj = GetNewInstance<T>(args);

            // Add to collection if not exists
            collection.Add(typeof(T), obj);
            // Return object
            return obj;
        }

        /// <summary>
        ///     Defines the strongMaps
        /// </summary>
        /// <param name="xroot"></param>
        /// <returns></returns>
        private IDictionary<string, string> SetStrongMaps(XElement xroot)
        {
            return xroot
                .Descendants("StrongMaps")
                .Descendants()
                .Where(d => d.Name.LocalName == "Map")
                .ToDictionary(m => m.Value, m =>
                {
                    var typeName = m.Attribute("implementation").Value;
                    return typeName.Contains(_implementationsNamespace.ToString())
                    ? typeName
                    : $"{_implementationsNamespace}.{typeName}";
                });
        }

        private void SetSingletonAndStrongMaps(XElement xroot)
        {
            if (xroot.Name.LocalName != _EP && !xroot.Descendants(_EP).Any())
            {
                throw new ArgumentException("The provided configuration does not seem to have a EP configuration");
            }
            _singletonTypes = new Dictionary<string, IEnumerable<string>>();
            _singletonTypes = SetTypesForSingleton(xroot);
            _strongMaps = SetStrongMaps(xroot);
        }

        /// <summary>
        ///     Returns a list of Types of the instatiationType passed from the configurationXML
        /// </summary>
        /// <param name="epRoot"></param>
        /// <returns></returns>
        private static IDictionary<string, IEnumerable<string>> SetTypesForSingleton(XElement epRoot)
        {
            return epRoot
                .Descendants(_Singletons)
                .Descendants().Where(d => d.Name.LocalName == _Type).GroupBy(n => n.GetNamespaceOfPrefix(_epns).ToString())
               .ToDictionary(g => g.Key, g => g.Select(ge => ge.Value))
               ?? new Dictionary<string, IEnumerable<string>>();
        }

        /// <summary>
        ///     Returns an instance of the Implementation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        private T GetNewInstance<T>(params object[] args)
        {
            var wantedType = typeof(T);
            var modelType = GetModelTypeOf(wantedType);
            return (T)Activator.CreateInstance(modelType, args);
        }

        private Assembly GetAssemblyIfLoaded(string? name)
        {
            return Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name.Equals(name));
        }

        #endregion
    }
}
