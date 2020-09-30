using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityProvider
{
    public sealed partial class EP
    {
        #region Implementation Getters
        /// <summary>
        ///     Returns a Singleton or Transient instance depending on the configuration passed
        ///     Falling back to transient if no configuration given
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T New<T>(params object[] args)
        {
            if (_singletonTypes.Count > 0 && InSingletonTypes<T>())
                return GetSingleton<T>(args);

            return GetTransient<T>(args);
        }

        /// <summary>
        ///     Returns a singleton instance of the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public T GetSingleton<T>(params object[] args)
        {
            if (_singletonTypes.Any() && !InSingletonTypes<T>() || !_implementationsNamespace.HasValue)
                throw new TypeAccessException("The requested Is not meant to be singleton. Please add it to your configuration if you want it so.");

            PrepareSingletonsCollectionIfNotReadyYet();

            return Get<T>(_singletons[_dllLocation][(NameSpace)_implementationsNamespace], args);
        }

        private void PrepareSingletonsCollectionIfNotReadyYet()
        {
            if (_implementationsNamespace == null) return;

            if (!_singletons.ContainsKey(_dllLocation))
                _singletons.Add(_dllLocation, new Dictionary<NameSpace, IDictionary<Type, object>>());

            if (!_singletons[_dllLocation].ContainsKey((NameSpace)_implementationsNamespace))
                _singletons[_dllLocation].Add((NameSpace)_implementationsNamespace, new Dictionary<Type, object>());
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
        ///     Returns a Scope
        /// </summary>
        /// <returns></returns>
        public Scope GetScope()
        {
            return new Scope(_dllLocation, _implementationsNamespace.ToString());
        }
        #endregion
    }
}
