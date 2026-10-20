---
description: Register several named meters in one .NET application and resolve each explicitly through keyed services.
---
# Named meter registration

Use named registration when you want multiple logical meter sources in the same application and resolve them explicitly with keyed services.

## Register a named meter

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddNamedMeter("orders");
```

This registers:

- keyed `System.Diagnostics.Metrics.Meter` with the name `orders`
- keyed `IMeter<T>` that resolves to that same named `Meter`

## Register an instrumentation scope version

Pass `version` to identify the instrumentation scope version:

```csharp
services.AddNamedMeter("orders", version: "1.2.3");
```

The keyed meter exposes `Version == "1.2.3"`. The existing overload without a version remains supported and keeps a `null` version.

## Resolve the named meter in consumers

```csharp
using Cratis.Metrics;
using Microsoft.Extensions.DependencyInjection;

public class OrderService([FromKeyedServices("orders")] IMeter<OrderService> meter)
{
}
```

`IMeter<T>` remains available as a non-keyed service as well, where the underlying meter name defaults to `typeof(T).FullName`. These per-type meters are not subscribed by default. Explicitly subscribe that name in your telemetry provider (for example, with `AddMeter` in OpenTelemetry), or use a named meter that your provider subscribes to.

## Registration boundaries

The name is both the diagnostics source name and the dependency-injection key. Use the same string when registering, resolving, and configuring your telemetry provider. A null, empty, or whitespace-only name is rejected. Different names create separate singleton sources; typed consumers resolved with the same key share the underlying meter even when their generic type arguments differ.

Registration uses `TryAddKeyedSingleton`. Registering the same name again does not replace its existing services, so choose the version on the first registration rather than trying to update it with a later call. Registration alone does not configure an exporter or subscribe a telemetry provider.

## See also

- [Overview](index.md) — typed metrics APIs and usage
- [Roslyn](roslyn.md) — generate instrumentation methods
- [Named activity sources](../traces/named-registration.md) — configure the other diagnostics signal
