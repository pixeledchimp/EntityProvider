using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace EntityProvider
{
    public sealed partial class EP
    {
        #region Constructors
        internal EP(Assembly callingAssenbly, NameSpace? implementationsNamespace = new NameSpace?())
        {
            _callingAssenbly = callingAssenbly;

            _dllLocation = _callingAssenbly.Location;

            if (implementationsNamespace.HasValue)
            {
                _implementationsNamespace = (NameSpace)implementationsNamespace;
            }
        }
        /// <summary>
        ///     Basic Constructor
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        internal EP(string dllLocation, NameSpace implementationsNamespace)
        {
            _dllLocation = dllLocation;
            _implementationsNamespace = implementationsNamespace;
        }

        /// <summary>
        ///     Constructor accepts XElement
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        /// <param name="xmlConfigurationString"></param>
        internal EP(string dllLocation, NameSpace implementationsNamespace, XElement xmlConfigurationString) : this(dllLocation, implementationsNamespace)
        {
            // Config the Types
            SetSingletonAndStrongMaps(xmlConfigurationString);
        }

        /// <summary>
        ///     Full featured xml conf
        /// </summary>
        /// <param name="xroot"></param>
        internal EP(XElement xroot)
        {
            if (xroot.Name.LocalName != _EP && !xroot.Descendants(_EP).Any())
            {
                throw new ArgumentException("The provided configuration does not seem to have a EP configuration");
            }

            if (!xroot.Attributes().Any(a => a.Name.LocalName == _epns) || !xroot.Attributes().Any(a => a.Name.LocalName == _dll))
            {
                throw new ArgumentException("The provided configuration does not seem to be a full feature EP configuration");
            }

            _dllLocation = xroot.Attribute(_dll).Value;
            _implementationsNamespace = xroot.GetNamespaceOfPrefix(_epns).NamespaceName;

            SetSingletonAndStrongMaps(xroot);
        }

        #endregion
    }
}
