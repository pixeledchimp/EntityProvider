using System;
using System.Collections.Generic;

namespace EntityProvider
{
    public class Scope
    {
        private EP _sep;

        public Scope(string dllLocation, string implementationNamespace)
        {
            _sep = EpFactory.GetProvider(dllLocation, implementationNamespace);
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
}
