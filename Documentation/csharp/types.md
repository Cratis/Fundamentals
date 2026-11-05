---
title: Discover .NET implementations with Cratis.Types
description: Cratis.Types finds implementations across assemblies using generated providers or reflection, and reports which assemblies contributed to discovery.
---

<a id="types"></a>

`Cratis.Types` in Cratis Fundamentals finds types across assemblies so you can look up implementations of an interface or base type without maintaining a list by hand. Discovery prefers registered generated providers and falls back to reflection when none are registered. Use `ITypes` for queries or `IImplementationsOf<T>` when a consumer needs the implementations of one contract.

Discovery builds a snapshot of the available types. Later lookups use that snapshot; they do not scan again for every query. If an implementation is missing, first check which discovery strategy and assemblies built it, as shown in [Knowing what was discovered](#knowing-what-was-discovered).

For direct use outside a configured host, construct `Types` from the `Cratis.Types` namespace. It implements `ITypes` and uses the default provider selection unless you pass providers explicitly.

## Assembly Prefixes

The reflection fallback includes project-referenced assemblies. Its `PackageReferencedAssemblies` provider includes package assemblies by name prefix; the default prefix is `Cratis`. To include other packages through that provider, add their prefixes to the shared `PackageReferencedAssemblies.Instance` before it first initializes:

```csharp
using Cratis.Types;

PackageReferencedAssemblies.Instance.AddAssemblyPrefixesToInclude("Microsoft", "SomeOther");
```

A prefix matches the start of an assembly name: `Microsoft` includes `Microsoft.*` packages. Use the shared singleton to avoid creating separate providers that repeat the package scan.

This setting configures the reflection provider, not generated discovery. The generator applies its own `Cratis` prefix check for package discovery, and default provider selection prefers registered generated providers. To use your configured reflection provider, pass it explicitly to `new Types(...)` or `AddTypeDiscovery(providers)`.

Changing a provider does not refresh a snapshot already built from it. Configure prefixes before the provider initializes, and choose providers before constructing the `Types` instance that will serve your lookups.

## Type Discovery

Use `ITypes.FindMultiple<T>()` to find implementations of a known interface or base type. The overload taking a `Type` supports the same lookup when the contract is known only at runtime:

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

If a consumer always works with one contract, inject `IImplementationsOf<T>` instead. It exposes the matching `Type` objects for enumeration, not instances of those types:

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

For criteria other than an interface or base type, filter `ITypes.All`. To resolve objects rather than inspect their types, use [`IInstancesOf<T>`](#as-instances).

## Knowing what was discovered

When a lookup returns fewer types than expected, inspect `ITypes.DiscoveryMode` and `ITypes.Assemblies`. They report how the snapshot was built and which assemblies contributed types, so you can distinguish an absent implementation from an assembly that never reached discovery:

```csharp
using Cratis.Types;

public class MySystem
{
    public MySystem(ITypes types)
    {
        // Which strategy built the universe...
        Console.WriteLine($"Type discovery: {types.DiscoveryMode}");

        // ... and which assemblies reached it.
        foreach (var assembly in types.Assemblies)
        {
            Console.WriteLine($"  {assembly.GetName().Name}");
        }
    }
}
```

`DiscoveryMode` is one of:

| Mode | Meaning |
|---|---|
| `Unknown` | The `ITypes` implementation does not report one |
| `Generated` | Compile-time generated providers were registered and used exclusively |
| `Reflected` | No generated providers were registered, so the reference closures were walked by reflection |
| `Explicit` | The providers were passed to the `Types` constructor by the caller |

Generated providers register through module initializers. Under the just-in-time (JIT) runtime, call `GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered()` before constructing `Types` to load reachable referenced assemblies and run their initializers. Constructing `Types` alone reads the providers already registered.

For a container, `AddTypeDiscovery()` ensures providers are registered before selecting its snapshot. Outside a container, `TypesServiceCollectionExtensions.CurrentTypeUniverse()` provides the same snapshot. Both reuse it while the provider set is unchanged and rebuild it when that set grows. An earlier `ITypes` instance remains unchanged, so check `Assemblies` on the instance your consumer actually uses.

A project reference to Fundamentals does not bring in the generator the way the NuGet package's analyzer does. Reference the generator explicitly from projects that consume Fundamentals through a project reference. Otherwise, Fundamentals can register its own provider while the consuming project's types remain absent: discovery may report `Generated` with only `Cratis.Fundamentals` in `Assemblies`.

## Native AOT

Native ahead-of-time compilation (NativeAOT) runs compiled module initializers before `Main`. The generated provider registry therefore does not need to load assemblies or run initializers again. `GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered()` skips its reference walk when assembly-reference enumeration is unsupported.

Trimming can remove an assembly that contributes only through reflection and is never referenced by compiled code. Generated registration does not itself keep its containing assembly in the executable. Keep such assemblies in the executable explicitly with `TrimmerRootAssembly` or `DynamicDependency`; discovery cannot recover code removed by trimming. The type-discovery sample demonstrates discovery in a native executable.

Generated providers also create enum JSON converters for enums declared in their assemblies, allowing `EnumConverterFactory` to use those converters under NativeAOT. Rebuild assemblies compiled with a generator version that did not emit them.

For dependency injection, generated convention and self bindings preserve public instance constructors through `DynamicDependency` attributes on the module initializer. This covers bound, non-static, non-abstract classes and structs, including open generic definitions. Struct constructors must be explicitly declared: an implicit parameterless struct constructor has no IL member to preserve.

Types with no public instance constructor, along with enums, interfaces, and delegates, receive no constructor-preservation attribute. These checks affect preservation, not membership in the binding lists. Rebuild assemblies compiled with a generator version that did not emit constructor-preservation attributes.

If you write an `ICanProvideConventionsForDependencyInjection` provider yourself, preserve the implementation types' public constructors too. For example, put `[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(MyImplementation))]` on a method kept when the provider is used. A `typeof(MyImplementation)` reference in a binding alone does not preserve its constructors.

## Watching discovery happen

Default discovery can run from a static field initializer, before the host has a container or logger. Its event source lets you observe the result without relying on host logging:

```bash
dotnet-trace collect --providers Cratis.Fundamentals.TypeDiscovery
```

`UniverseBuilt` reports the discovery mode and assemblies each time a snapshot is built. `UniverseContainsOnlyThisPackage` warns when the only contributing assembly is `Cratis.Fundamentals`. In an application, this usually means its own types are missing from discovery; check the generator reference and provider selection before relying on convention-based lookups.

To include these events in host logs, bridge the event source with an `EventListener`.

## Asking what discovery missed

If an expected assembly is absent from `ITypes.Assemblies`, use `TypeDiscoveryDiagnostics.FindMissingContributors(types)`. It compares the snapshot with project-referenced libraries in the entry assembly's dependency model:

```csharp
using Cratis.Types;

foreach (var missing in TypeDiscoveryDiagnostics.FindMissingContributors(types))
{
    Console.WriteLine($"Referenced but never discovered: {missing}");
}
```

Run this diagnostic where you need the comparison, such as a health check or startup self-test. It consults the dependency model only when called; default startup does not perform this comparison.

The diagnostic reads library names without loading assemblies. A reported library may legitimately contribute no types, so treat the result as a lead to investigate, not proof of a missing implementation.

## As Instances

Use `IInstancesOf<T>` when you need objects rather than `Type` metadata. It asks the dependency injection container to resolve each discovered concrete type, including its dependencies. Those concrete types and their dependencies must be registered; discovering a type does not register it. See [Dependency Injection](./dependency_injection.md) for convention and self-binding rules.

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

Resolution happens during enumeration. `IInstancesOf<T>` does not cache the results: enumerating again asks the container again, and the service registrations' lifetimes determine whether it returns new or shared instances.
