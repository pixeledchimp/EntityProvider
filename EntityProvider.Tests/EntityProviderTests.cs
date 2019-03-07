using Xunit;
using EntityProvider;
using EntityProvider.Tests;

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
    }
}