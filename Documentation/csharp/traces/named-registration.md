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

`IActivitySource<T>` remains available as a non-keyed service as well, where the underlying source name defaults to `typeof(T).FullName`. Per-type sources are not subscribed by default; explicitly subscribe the source name in your telemetry provider, or use a named source that your provider subscribes to.

## Registration boundaries

The name is both the diagnostics source name and the dependency-injection key. Use the same string when registering, resolving, and configuring your telemetry provider. A null, empty, or whitespace-only name is rejected. Different names create separate singleton sources; typed consumers resolved with the same key share the underlying activity source even when their generic type arguments differ.

Registration uses `TryAddKeyedSingleton`. Registering the same name again does not replace its existing services, so choose the version on the first registration rather than trying to update it with a later call. Registration alone does not configure an exporter or subscribe a telemetry provider.

## See also

- [Overview](index.md) — typed traces APIs and usage
- [Roslyn](roslyn.md) — generate instrumentation methods
- [Named meters](../metrics/named-registration.md) — configure the other diagnostics signal
