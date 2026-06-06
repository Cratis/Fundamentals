# Types

The fundamentals package includes `Cratis.Types`, which provides mechanisms for discovering types in the project referenced assemblies.
It examines the entry assembly and finds all its project references at runtime, indexing the types from all of these assemblies.

The type discovery system automates tasks at runtime, for instance removing the need for manual configuration of types to include in a system by discovering them based on your criteria instead.

If you want to bypass any automatic hookup of the system, you can manually create an instance of the `Types` class in the `Cratis.Types` namespace. This class implements the `ITypes` interface.

## Assembly Prefixes

`Types` discovers project-referenced assemblies automatically. For **package**-referenced
assemblies, discovery is opt-in by assembly-name prefix — by default only assemblies whose
name starts with `Cratis` are included. To include additional packages, add their prefixes to
the shared `PackageReferencedAssemblies.Instance` before discovery runs:

```csharp
using Cratis.Types;

PackageReferencedAssemblies.Instance.AddAssemblyPrefixesToInclude("Microsoft", "SomeOther");
```

The strings you pass are treated as prefixes, so a single value matches every assembly whose
name starts with it — for example, `Microsoft` matches all `Microsoft.*` packages.

> Note: Use the shared `PackageReferencedAssemblies.Instance` singleton rather than constructing
> your own — scanning all assemblies in the application has a performance cost.

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
