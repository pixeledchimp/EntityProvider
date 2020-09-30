using System;
using System.Collections.Generic;
using System.Reflection;


namespace EntityProvider
{
    public sealed partial class EP
    {
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
        private NameSpace? _implementationsNamespace;

        /// <summary>
        ///      The assembly from where the EP instance is been created
        /// </summary>
        private Assembly? _callingAssenbly;

        /// <summary>
        ///     Collection that stores singleton objects
        /// </summary>
        private static readonly IDictionary<string, IDictionary<NameSpace, IDictionary<Type, object>>> _singletons = new Dictionary<string, IDictionary<NameSpace, IDictionary<Type, object>>>();

        /// <summary>
        ///     StrongMaps
        /// </summary>
        private IDictionary<string, string> _strongMaps = new Dictionary<string, string>();

        #endregion
    }
}
