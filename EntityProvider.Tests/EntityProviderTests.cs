using System;
using System.Reflection;
using Xunit;

namespace EntityProvider.Tests
{
    public class EntityProviderTests
    {
        private EP _ep;

        public EntityProviderTests()
        {
            _ep = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests");
        }

        [Fact]
        public void GetImplementationFromCurrentAssemblySuccess()
        {
            // Arrange
            var ep = EpFactory.GetProvider();

            // Act
            var implementedModel = ep.GetTransient<IInterfaceModel>();

            // Assert
            Assert.IsAssignableFrom<IInterfaceModel>(implementedModel);
        }

        /// <summary>
        ///     Tests that the returned object is of the requested type
        /// </summary>
        [Fact]
        public void GetImplementation_Test_Success()
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
        public void ConstructorArguments_Test_Success()
        {
            // Arrange
            const string dontPanic = "Dont' Panic!";
            const int answerToLifeTheUniverseAndEverything = 42;

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
        public void ScopeTest_Test_Success()
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
        public void Singleton_Test_Success()
        {
            // Singleton entities are always the same object
            var singletonObject = _ep.GetSingleton<IInterfaceModel>();
            var singletonSame = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests").GetSingleton<IInterfaceModel>();
            {
                var singletonObjectIndifferentScope = _ep.GetSingleton<IInterfaceModel>();

                // Assert

                // Singletons of same
                Assert.Equal(singletonObject, singletonSame);

                // Singletons gotten in different scopes are the same object
                Assert.Equal(singletonObject, singletonObjectIndifferentScope);
            }

            // EntityProviders must return different singletons from different namespaces
            var otherEp = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests.New");

            var singletonObjectNew = otherEp.GetSingleton<IInterfaceModel>();

            Assert.NotEqual(singletonObject, singletonObjectNew);

            // Entity providers cannot provide Singletons from Namespaces where there is no imlementation
            var noImplementationEp = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests.NotImplementedEntities");
            Assert.Throws<NotImplementedException>(() => noImplementationEp.GetSingleton<IInterfaceModel>());
        }

        [Fact]
        public void SingletonConfiguration_Test_Success()
        {
            var conf = "<EP><Singletons xmlns:epns=\"EntityProvider.Tests\"><epns:Type>IInterfaceModel</epns:Type></Singletons></EP>";
            var localEpWithSingletonsConf = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests", conf);

            // Singleton entities are always the same object
            var singletonObject = _ep.GetSingleton<IInterfaceModel>();
            var singletonSame = localEpWithSingletonsConf.New<IInterfaceModel>();
            {
                var singletonObjectIndifferentScope = localEpWithSingletonsConf.New<IInterfaceModel>();

                // Assert

                // Singletons of same
                Assert.Equal(singletonObject, singletonSame);

                // Singletons gotten in different scopes are the same object
                Assert.Equal(singletonObject, singletonObjectIndifferentScope);
            }

            // EntityProviders must return different singletons from different namespaces
            var otherEp = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests.New", conf);

            var singletonObjectNew = otherEp.New<IInterfaceModel>();

            Assert.NotEqual(singletonObject, singletonObjectNew);

            // Entity providers cannot provide Singletons from Namespaces where there is no imlementation
            var noImplementationEp = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests.NotImplementedEntities", conf);
            Assert.Throws<NotImplementedException>(() => noImplementationEp.New<IInterfaceModel>());
        }

        [Fact]
        public void BadSingletonConfigurationTests_Test_Fail()
        {
            var conf = "<EP><Singletons xmlns:epns=\"EntityProvider.Tests\"><epns:Type>IInterfaceModelX</epns:Type></Singletons></EP>";
            var aProvider = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests", conf);
            Assert.Throws<TypeAccessException>(() => aProvider.GetSingleton<IInterfaceModel>());
        }

        [Fact]
        public void StrongMapsTest_Test_Success()
        {
            var conf = "<EP><StrongMaps><Map implementation=\"OtherImplementedModel\">EntityProvider.Tests.IInterfaceModel</Map></StrongMaps></EP>";
            var Ep = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests", conf);
            var impl = Ep.New<IInterfaceModel>();
            Assert.Equal("EntityProvider.Tests.OtherImplementedModel", impl.SomeProp1);
        }

        [Fact]
        public void FullyFeaturedConf_Test_Success()
        {
            var conf = $@"<EP xmlns:epns=""EntityProvider.Tests"" dll=""{Assembly.GetExecutingAssembly().Location}"">
                            <StrongMaps>
                                <Map implementation=""ImplementedModel"">
                                    EntityProvider.Tests.IInterfaceModel
                                </Map>
                            </StrongMaps>
                            <Singletons>
                                <epns:Type>IInterfaceModel</epns:Type>
                            </Singletons>
                         </EP>";
            var Ep = EpFactory.GetProvider(conf);
            var impl = Ep.New<IInterfaceModel>();

            // Singleton entities are always the same object
            var singletonObject = _ep.GetSingleton<IInterfaceModel>();
            var singletonSame = Ep.New<IInterfaceModel>();
            {
                var singletonObjectIndifferentScope = Ep.New<IInterfaceModel>();

                // Assert

                // Singletons of same
                Assert.Equal(singletonObject, singletonSame);

                // Singletons gotten in different scopes are the same object
                Assert.Equal(singletonObject, singletonObjectIndifferentScope);
            }

            // EntityProviders must return different singletons from different namespaces
            var otherEp = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests.New", conf);

            var singletonObjectNew = otherEp.New<IInterfaceModel>();

            Assert.NotEqual(singletonObject, singletonObjectNew);

            // Entity providers cannot provide Singletons from Namespaces where there is no imlementation
            var noImplementationEp = EpFactory.GetProvider(Assembly.GetExecutingAssembly().Location, "EntityProvider.Tests.NotImplementedEntities", conf);
            Assert.Throws<NotImplementedException>(() => noImplementationEp.New<IInterfaceModel>());
        }

        [Fact]
        public void GetProviderusingLoadedAssembly_Tests()
        {
            var ep = EpFactory.GetProvider("EntityProvider.Tests", "EntityProvider.Tests");
            const string greeting = "Hello World";
            var model = ep.GetTransient<IInterfaceModel>(greeting, 42);
            Assert.Equal(greeting, model.SomeProp1);
        }
    }
}