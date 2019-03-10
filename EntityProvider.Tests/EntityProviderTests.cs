using Xunit;
using EntityProvider;
using EntityProvider.Tests;
using System.ComponentModel;
using System;

namespace EntityProvider.Tests
{
    public class EntityProviderTests
    {
        private EP _ep;

        public EntityProviderTests()
        {
            _ep = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests");
        }

        /// <summary>
        ///     Tests that the returned object is of the requested type
        /// </summary>
        [Fact]
        public void EntityProvider_GetImplementation_Success()
        {
            // Arrange
            // Act
            var implementedModel = _ep.GetTransient<IInterfaceModel>();

            // Assert
            Assert.IsAssignableFrom<IInterfaceModel>(implementedModel);
        }

        /// <summary>
        ///     Tests that the entities can be created naturally passing their defined arguments
        /// </summary>
        [Fact]
        public void EntityProvider_ConstructorArguments_Test_Success()
        {
            // Arrange
            var dontPanic = "Dont' Panic!";
            var answerToLifeTheUniverseAndEverything = 42;

            // Act
            var implementedModel = _ep.GetTransient<IInterfaceModel>(dontPanic, answerToLifeTheUniverseAndEverything);

            // Assert
            Assert.Equal(dontPanic, implementedModel.SomeProp1);
            Assert.Equal(answerToLifeTheUniverseAndEverything, implementedModel.SomeProp2);
        }

        /// <summary>
        ///     Tests that the Scopes and scoped entities are the same or not depending on the scope
        /// </summary>
        [Fact]
        public void EntityProvider_ScopeTest_Success()
        {
            // Arrange
            var scope = _ep.GetScope();
            var scopedObject = scope.Get<IInterfaceModel>();
            var scopedObject2 = scope.Get<IInterfaceModel>();

            // Act
            {
                var innerScope = _ep.GetScope();
                var innerScopedObject = innerScope.Get<IInterfaceModel>();
                var innerScopedObject2 = innerScope.Get<IInterfaceModel>();

                // Assert

                // Two Scopes are never the same object
                Assert.NotEqual(scope, innerScope);

                // Objects of same types from different Scopes in different scopes are not the same object
                Assert.NotEqual(scopedObject, innerScopedObject);

                // Objects of same types from different Scopes in the same scope are the same object
                Assert.Equal(scopedObject, scopedObject2);
                Assert.Equal(innerScopedObject, innerScopedObject2);
            }
        }

        [Fact]
        public void EntityProvider_SingletonTests_Success()
        {
            // Singleton entities are always the same object
            var singletonObject = _ep.GetSingleton<IInterfaceModel>();
            var singletonSame = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests").GetSingleton<IInterfaceModel>();
            {
                var singletonObjectIndifferentScope = _ep.GetSingleton<IInterfaceModel>();

                // Assert

                // Singletons of same
                Assert.Equal(singletonObject, singletonSame);

                // Singletons gotten in different scopes are the same object
                Assert.Equal(singletonObject, singletonObjectIndifferentScope);
            }

            // EntityProviders must return different singletons from different namespaces
            var otherEp = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests.New");

            var singletonObjectNew = otherEp.GetSingleton<IInterfaceModel>();

            Assert.NotEqual(singletonObject, singletonObjectNew);

            // Entity providers cannot provide Singletons from Namespaces where there is no imlementation
            var noImplementationEp = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests.NotImplementedEntities");
            Assert.Throws<NotImplementedException>(() => noImplementationEp.GetSingleton<IInterfaceModel>());
        }
    }
}