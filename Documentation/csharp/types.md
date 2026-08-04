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

## Knowing what was discovered

Discovery happens once, when the type universe is built, and everything downstream resolves against
whatever it captured. Two properties on `ITypes` say what that was, so a consumer — or a spec, or a
readiness probe — can assert the universe is the one it expected instead of inferring it.

```csharp
using Cratis.Types;

public class MySystem
{
    public MySystem(ITypes types)
    {
        // Which strategy built the universe.
        var mode = types.DiscoveryMode;

        // And which assemblies reached it.
        foreach (var assembly in types.Assemblies)
        {
            Console.WriteLine(assembly.GetName().Name);
        }
    }
}
```

`DiscoveryMode` is one of:

| Mode | Meaning |
|---|---|
| `Generated` | Compile-time generated providers were registered and used exclusively |
| `Reflected` | No generated providers were registered, so the reference closures were walked by reflection |
| `Explicit` | The providers were passed to the `Types` constructor by the caller |

This matters because the generated strategy sees exactly the assemblies whose module initializers had
run by the time the universe was built. An assembly that carries a generated provider and arrives later
contributes nothing — and nothing says so, because a shorter `FindMultiple<T>()` result looks the same
as a feature nobody wrote. `Assemblies` is the list of the ones that did contribute, so a missing name
is the answer.

> [!NOTE]
> A project that consumes Fundamentals through a **project reference** rather than the NuGet package
> does not get the generator, because the package flows it as an analyzer and a project reference does
> not. Fundamentals still registers its own provider, so discovery reports `Generated` with only
> `Cratis.Fundamentals` in `Assemblies` — and none of that project's own types are found. Reference the
> generator explicitly from such a project.

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
