using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace EntityProvider
{
    public sealed class EP
    {
        #region Constructor
        
        /// <summary>
        ///     Basic Constructor
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        private EP(string dllLocation, string implementationsNamespace)
        {
            _dllLocation = dllLocation;
            _implementationsNamespace = new NameSpace(implementationsNamespace);
        }

        /// <summary>
        ///     Constructor accepts XElement
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        /// <param name="xmlConfigurationString"></param>
        private EP(string dllLocation, string implementationsNamespace, XElement xmlConfigurationString) : this(dllLocation, implementationsNamespace)
        {
            // Config the Types
            SetSingletonAndStrongMaps(xmlConfigurationString);
        }

        /// <summary>
        ///     Full featured xml conf
        /// </summary>
        /// <param name="xroot"></param>
        public EP(XElement xroot)
        {
            if(xroot.Name.LocalName != _EP && !xroot.Descendants(_EP).Any())
            {
                throw new ArgumentException("The provided configuration does not seem to have a EP configuration");
            }

            if (!xroot.Attributes().Any( a => a.Name.LocalName == _epns) || !xroot.Attributes().Any( a => a.Name.LocalName == _dll ))
            {
                throw new ArgumentException("The provided configuration does not seem to be a full feature EP configuration");
            }

            _dllLocation = xroot.Attribute(_dll).Value;
            _implementationsNamespace = new NameSpace(xroot.GetNamespaceOfPrefix(_epns).NamespaceName);
            
            SetSingletonAndStrongMaps(xroot);
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Types for Singleton
        ///     Dictionary of 
        /// </summary>
        private IDictionary<string, IEnumerable<string>> _singletonTypes = new Dictionary<string, IEnumerable<string>>();

        /// <summary>
        ///     Path to implementation dll
        /// </summary>
        private readonly string _dllLocation;

        /// <summary>
        ///     Name of the TAG that contains the EntityProvider configuration options
        /// </summary>
        private const string _EP = "EP";

        /// <summary>
        ///     Node that contains the Types to be instantiated a singletons
        /// </summary>
        private const string _Singletons = "Singletons";

        /// <summary>
        ///     Name of the XML TAG that represents a type
        /// </summary>
        private const string _Type = "Type";

        /// <summary>
        ///     epns: Entity provider namesspae
        ///     Used to set the xml namespace in Singletons
        /// </summary>
        private const string _epns = "epns";

        /// <summary>
        ///     Name of attribute to define the dll filename path
        /// </summary>
        private const string _dll = "dll";

        /// <summary>
        ///     Implementations namespace
        /// </summary>
        private NameSpace _implementationsNamespace;
        
        /// <summary>
        ///     Collection that stores singleton objects
        /// </summary>
        private static readonly IDictionary<string, IDictionary<NameSpace, IDictionary<Type, object>>> _singletons = new Dictionary<string, IDictionary<NameSpace, IDictionary<Type, object>>>();

        /// <summary>
        ///     StrongMaps
        /// </summary>
        private IDictionary<string, string> _strongMaps;

        #endregion

        #region PrivateInterface
        
        /// <summary>
        ///     Returns the Type of the Interface implementation class
        /// </summary>
        /// <param name="wantedType">The interface type</param>
        /// <returns>The implementation class</returns>
        private Type GetModelTypeOf(Type wantedType)
        {
            var modelTypes = GetTypesInNamespace(Assembly.LoadFrom(_dllLocation), _implementationsNamespace.ToString());
            
            if(_strongMaps?.Count > 0)
            {
                var wanterTypeString = wantedType.ToString();

                if (_strongMaps.ContainsKey(wanterTypeString))
                {
                    var foundType =  modelTypes.FirstOrDefault(t => t.FullName == _strongMaps[wantedType.ToString()]);
                    if(foundType != null) return foundType;
                }
            }

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
        ///     Returns an instance of the passed ScopeType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private T Get<T>(IDictionary<Type, object> collection = null, params object[] args)
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
                .Where( d => d.Name.LocalName == "Map")
                .ToDictionary( m => m.Value, m => { 
                    var typeName = m.Attribute("implementation").Value;
                    return typeName.Contains(_implementationsNamespace.ToString())
                    ? typeName
                    : $"{_implementationsNamespace}.{typeName}";
                });
        }


        #endregion

        #region PublicInterface
        
        /// <summary>
        ///     Entity provider instance provider
        /// </summary>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, string implementationsNamespace, string xmlConfigurationString = null)
        {
            try
            {
                var xdoc = XDocument.Parse(xmlConfigurationString);
                if(xdoc == null) throw new ArgumentException($"Error parsing configuration");
                return new EP(dllLocation, implementationsNamespace, xdoc.Root);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        /// <summary>
        ///     Provider accepts xelement conf
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        /// <param name="xmlConfiguration"></param>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, string implementationsNamespace, XElement xmlConfiguration)
        {
            try
            {
                return new EP(dllLocation, implementationsNamespace, xmlConfiguration);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        /// <summary>
        ///     Simple provider
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
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
        ///     Full featured conf provider
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static EP GetProvider(string conf)
        {
            try
            {
                var xdoc = XDocument.Parse(conf);
                if(xdoc == null) throw new ArgumentException($"Error parsing configuration");
                return new EP(xdoc.Root);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        /// <summary>
        ///     Provider accepts Xelement conf
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static EP GetProvider(XElement conf)
        {
            try
            {
                return new EP(conf);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private void SetSingletonAndStrongMaps(XElement xroot)
        {
            if(xroot.Name.LocalName != _EP && !xroot.Descendants(_EP).Any())
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
        /// <param name="xmlConfigurationString"></param>
        /// <param name="InstatiationType"></param>
        /// <returns></returns>
        private static IDictionary<string, IEnumerable<string>> SetTypesForSingleton(XElement epRoot)
        {
            return epRoot
                .Descendants(_Singletons)
                .Descendants().Where( d => d.Name.LocalName == _Type).GroupBy( n => n.GetNamespaceOfPrefix(_epns).ToString())
               .ToDictionary( g => g.Key, g => g.Select( ge => ge.Value )) 
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

        /// <summary>
        ///     Returns a Singleton or Transient instance depending on the configuration passed
        ///     Falling back to transient if no configuration given
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T New<T>(params object[] args)
        {
            if(_singletonTypes.Count > 0 && InSingletonTypes<T>())
                return GetSingleton<T>(args);

            return GetTransient<T>(args);
        }

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
        ///     Returns a singleton instance of the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T GetSingleton<T>(params object[] args)
        {
            try
            {
                if(_singletonTypes.Any() && !InSingletonTypes<T>())
                {
                    throw new TypeAccessException("The requested Is not meant to be singleton. Please add it to your configuration if you want it so.");
                }

                if (!_singletons.ContainsKey(_dllLocation))
                {
                    _singletons.Add(_dllLocation, new Dictionary<NameSpace, IDictionary<Type, object>>());
                }

                if (!_singletons[_dllLocation].ContainsKey(_implementationsNamespace))
                {
                    _singletons[_dllLocation].Add(_implementationsNamespace, new Dictionary<Type, object>());
                }

                return Get<T>(_singletons[_dllLocation][_implementationsNamespace], args);
            }
            catch (TypeAccessException)
            {
                throw;
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


        #endregion

        #region InnerTypes
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

        /// <summary>
        ///     Represents a Type namespace
        /// </summary>
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
        #endregion
    }
}
