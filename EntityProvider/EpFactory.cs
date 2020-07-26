using System;
using System.Reflection;
using System.Xml.Linq;

namespace EntityProvider
{
    public static class EpFactory
    {
        /// <summary>
        ///     Entity provider instance provider
        /// </summary>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, NameSpace implementationsNamespace, string xmlConfigurationString = null)
        {
            var xdoc = XDocument.Parse(xmlConfigurationString);
            if (xdoc == null) throw new ArgumentException($"Error parsing configuration");
            return new EP(dllLocation, implementationsNamespace, xdoc.Root);
        }

        /// <summary>
        ///     Provider accepts xelement conf
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        /// <param name="xmlConfiguration"></param>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, NameSpace implementationsNamespace, XElement xmlConfiguration)
        {
            return new EP(dllLocation, implementationsNamespace, xmlConfiguration);
        }

        /// <summary>
        ///     Simple provider
        /// </summary>
        /// <param name="dllLocation"></param>
        /// <param name="implementationsNamespace"></param>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, NameSpace implementationsNamespace)
        {
            return new EP(dllLocation, implementationsNamespace);
        }

        /// <summary>
        ///     Full featured conf provider
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static EP GetProvider(string conf)
        {
            var xdoc = XDocument.Parse(conf);
            if (xdoc == null) throw new ArgumentException($"Error parsing configuration");
            return new EP(xdoc.Root);
        }

        /// <summary>
        ///     Provider accepts Xelement conf
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static EP GetProvider(XElement conf)
        {
            return new EP(conf);
        }

        /// <summary>
        ///     No arguments will get entities from the current assembly.
        /// </summary>
        /// <returns></returns>
        public static EP GetProvider(NameSpace? ns = new NameSpace?())
        {
            return new EP(Assembly.GetCallingAssembly(), ns);
        }
    }
}
