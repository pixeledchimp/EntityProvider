using Xunit;
using EntityProvider;
using EntityProvider.Tests;
using System.ComponentModel;

namespace EntityProvider.Tests
{
    public class EntityProviderTests
    {
        private EP _ep;

        public EntityProviderTests()
        {
            _ep = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests");
        }

        [Fact]
        [Description("Tests that the returned object is of the requested type")]
        public void EntityProvider_GetImplementation_Success()
        {
            // Arrange
            // Act
            var implementedModel = _ep.GetTransient<IInterfaceModel>();

            // Assert
            Assert.IsAssignableFrom<IInterfaceModel>(implementedModel);
        }

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
    }
}