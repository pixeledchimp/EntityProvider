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
        private string _dllLocation;
        private string _implementationsNamespace;
        /// <summary>
        ///     Entity provider private constructor
        /// </summary>
        private EP(string dllLocation, string implementationsNamespace)
        {
            _dllLocation = dllLocation;
            _implementationsNamespace = implementationsNamespace;
        }

        /// <summary>
        ///     Entity provider singleton instance provider
        /// </summary>
        /// <returns></returns>
        public static EP GetProvider(string dllLocation, string implementationsNamespace)
        {
            return new EP(dllLocation, implementationsNamespace);
        }

        /// <summary>
        ///     Returns fresh instance of an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T GetTransient<T>(params object[] args)
        {
            var wantedType = typeof(T);
            var modelType = GetModelTypeOf(wantedType);
            return (T) Activator.CreateInstance(modelType, args);
        }

        /// <summary>
        ///     Returns the Type of the Interface implementation class
        /// </summary>
        /// <param name="wantedType">The interface type</param>
        /// <returns>The implementation class</returns>
        private Type GetModelTypeOf(Type wantedType)
        {
            var modelTypes = GetTypesInNamespace(Assembly.LoadFrom(_dllLocation), _implementationsNamespace);

            foreach (var t in modelTypes)
            {
                // Looking for an implementation of the requested type and not of the same type.
                if (wantedType.IsAssignableFrom(t) && wantedType != t)
                {
                    return t;
                }
            }

            throw new SystemException($"The requested Type {wantedType} has not been found.");
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
    }
}
