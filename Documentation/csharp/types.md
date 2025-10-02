# Types

The fundamentals package includes `Cratis.Types`, which provides mechanisms for discovering types in the project referenced assemblies.
It examines the entry assembly and finds all its project references at runtime, indexing the types from all of these assemblies.

The type discovery system automates tasks at runtime, for instance removing the need for manual configuration of types to include in a system by discovering them based on your criteria instead.

If you want to bypass any automatic hookup of the system, you can manually create an instance of the `Types` class in the `Cratis.Types` namespace. This class implements the `ITypes` interface.

## Assembly Prefixes

By default, the `Types` system will load all referenced project and package assemblies.
You can exclude assemblies by using the static method `Types.AddAssemblyPrefixesToExclude()`.
This takes a `params` of strings representing prefixes of assemblies to exclude.
Out of the box, it ignores assemblies like `System`, `Microsoft`, and `Newtonsoft`.

Alternatively, the constructor for `Types` supports taking an explicit 'opt-in' filter for including assemblies
in type discovery. This allows you to include additional assemblies beyond the defaults.

For both filters, the strings you pass are considered prefixes, meaning that if you want to include
a set of assemblies all starting with the same string, you simply provide the common start.

```csharp
using Cratis.Types;

var types = new Types("Microsoft","SomeOther");
```

> Note: When using the application model, it will exclude even more 3rd parties.

## Type Discovery

There are basically two ways of discovering types:

- Using the APIs found in `ITypes` where you can easily get access to all discovered types or find types based on common base types/interfaces.
- Use the `IImplementationsOf<>` as a dependency and get all implementations of a specific type using generic parameters.

```csharp
using Cratis.Types;

public class MySystem
{
    public MySystem(ITypes types)
    {
        // Find multiple implementors of a specific interface...
        types.FindMultiple<ISomeInterface>();

        // ... or using its Type
        types.FindMultiple(typeof(ISomeInterface));
    }
}
```

An optimization of this would be the `IImplementationsOf<>`:

```csharp
using Cratis.Types;

public class MySystem
{
    public MySystem(IImplementationsOf<ISomeInterface> someInterfaceTypes)
    {
        // Loop through someInterfaceTypes and do stuff
    }
}
```

> Note: The `ITypes` interface also has an `All` property where you can basically filter types based on your own custom criteria.

## As Instances

A common scenario is to discover types where the implementation has dependencies themselves and instances need to be resolved using
the IoC container. The `IImplementationsOf<>` interface provides this mechanism in a convenient way.

```csharp
using Cratis.Types;

public class MySystem
{
    public MySystem(IInstancesOf<ISomeInterface> someInterfaceTypes)
    {
        // Loop through someInterfaceTypes and do stuff
    }
}
```

> Note: The instances are only created when looping through. The instances are not cached and if you enumerate it multiple times, it will ask the IoC again for the instance.
