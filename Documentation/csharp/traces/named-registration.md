---
description: Register several named activity sources in one .NET application and resolve each explicitly through keyed services.
---
# Named activity source registration

Use named registration when you want multiple logical activity sources in the same application and resolve them explicitly with keyed services.

## Register a named activity source

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddNamedActivitySource("orders");
```

This snippet registers only named (keyed) services. Unkeyed `IActivitySource<T>` requires an existing `AddBindingsByConvention()` or `AddSelfBindings()` setup, or an equivalent explicit unkeyed registration.

This registers:

- keyed `System.Diagnostics.ActivitySource` with the name `orders`
- keyed `IActivitySource<T>` that resolves to that same named `ActivitySource`

## Register an instrumentation scope version

Pass `version` to identify the instrumentation scope version:

```csharp
services.AddNamedActivitySource("orders", version: "1.2.3");
```

The keyed source exposes `Version == "1.2.3"`. The existing overload without a version remains supported and keeps an empty-string version.

## Resolve the named activity source in consumers

```csharp
using Cratis.Traces;
using Microsoft.Extensions.DependencyInjection;

public class OrderService([FromKeyedServices("orders")] IActivitySource<OrderService> activitySource)
{
}
```

Unkeyed `IActivitySource<T>` is available only when your application also uses `AddBindingsByConvention()` or `AddSelfBindings()` (or registers the unkeyed `IActivitySource<>` to `UnkeyedActivitySource<>` pair explicitly); `AddNamedActivitySource` adds only the keyed services. Where unkeyed registration exists, the underlying source name defaults to `typeof(T).FullName`. Per-type sources are not subscribed by `Cratis.*` unless the source name matches that configured wildcard. Explicitly subscribe the source name in your telemetry provider, or use a named source under a `Cratis.<Product>` scope that your provider subscribes to. Registration alone does not configure a provider; see [Set up OpenTelemetry](../opentelemetry.md).

## Registration boundaries

The name is both the diagnostics source name and the dependency-injection key. Use the same string when registering, resolving, and configuring your telemetry provider. A null, empty, or whitespace-only name is rejected. Different names create separate singleton sources; typed consumers resolved with the same key share the underlying activity source even when their generic type arguments differ.

Registration uses `TryAddKeyedSingleton`. Registering the same name again does not replace its existing services, so choose the version on the first registration rather than trying to update it with a later call. Registration alone does not configure an exporter or subscribe a telemetry provider.

## See also

- [Overview](index.md) — typed traces APIs and usage
- [Roslyn](roslyn.md) — generate instrumentation methods
- [Named meters](../metrics/named-registration.md) — configure the other diagnostics signal
