# EntityProvider
A Dependency injection Helper tool


## 1. Conventions:
For an optimal execution, EP requires you to keep your interfaces and implementations in separate namespaces.

## 2. Usage:

### Instantiate an EntityProvider

```
using EntityProvider; // Include The EntityProvider namespace

namespace YourNamespace
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var _ep = EP.GetProvider("YourImplementations.dll", "YourImplementationsNamespace"); // Get an instance if the EntityProvider
    }
  }
}

```

### Instantiate an entity
```
...
var _ep = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests"); // Get an instance if the EntityProvider

var implementedModel = _ep.GetTransient<IInterfaceModel>(); // Get a fresh instance

```

### Pass arguments to entity constructor
```

var dontPanic = "Dont' Panic!";
var answerToLifeTheUniverseAndEverything = 42;

var implementedModel = _ep.GetTransient<IInterfaceModel>(dontPanic, answerToLifeTheUniverseAndEverything);

```

### Scopes
```
var scope = _ep.GetScope();
var scopedObject = scope.Get<IInterfaceModel>();
var scopedObject2 = scope.Get<IInterfaceModel>();

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
 
```

### Singleton
```
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
```

### Config
Define in the configuration xml the namespace and types to be used as Singleton. Any attempt of creating a singleton that is not in that list will result in a TypeAccessException
```
var conf = "<EP><Singletons xmlns:epns=\"EntityProvider.Tests\"><epns:Type>IInterfaceModel</epns:Type></Singletons></EP>";


             // Singleton entities are always the same object
            var singletonObject = _ep.GetSingleton<IInterfaceModel>();
            var singletonSame = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests", conf).New<IInterfaceModel>();
            {
                var singletonObjectIndifferentScope = _ep.New<IInterfaceModel>();

                // Assert

                // Singletons of same
                Assert.Equal(singletonObject, singletonSame);

                // Singletons gotten in different scopes are the same object
                Assert.Equal(singletonObject, singletonObjectIndifferentScope);
            }

            // EntityProviders must return different singletons from different namespaces
            var otherEp = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests.New", conf);

            var singletonObjectNew = otherEp.New<IInterfaceModel>();

            Assert.NotEqual(singletonObject, singletonObjectNew);

            // Entity providers cannot provide Singletons from Namespaces where there is no imlementation
            var noImplementationEp = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests.NotImplementedEntities", conf);
            Assert.Throws<NotImplementedException>(() => noImplementationEp.New<IInterfaceModel>());
```
```
var conf = "<EP xmlns:epns=\"EntityProvider.Tests\"><epns:Singleton value=\"IInterfaceModelX\"/></EP>";
            var aProvider = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests", conf);
            Assert.Throws<TypeAccessException>(() => aProvider.GetSingleton<IInterfaceModel>());
```


### StrongMaps
By providing a strong mapping of the types you'd be able to keep more than one implementation of the requested type under the same namespace and define which one will be used.
```

var conf = "<EP><StrongMaps><Map implementation=\"EntityProvider.Tests.ImplementedModel\">EntityProvider.Tests.IInterfaceModel</Map></StrongMaps></EP>";
var Ep = EP.GetProvider("EntityProvider.Tests.dll", "EntityProvider.Tests", conf);
```
