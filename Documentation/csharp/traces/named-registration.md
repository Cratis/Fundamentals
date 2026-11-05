---
title: Cratis Fundamentals named activity source registration
description: Register named activity sources in Cratis Fundamentals so typed .NET services share a tracing scope. Resolve each source with a dependency-injection key.
---
Named activity source registration lets several .NET services report spans under the same source name, instead of a name derived from each service type. Cratis Fundamentals uses keyed dependency injection, where a name selects the service to inject. Register separate names when you need separate tracing scopes.

## Register a named activity source

Reference `Cratis.Fundamentals` and register the source in your application's service collection:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddNamedActivitySource("orders");
```

The call registers a singleton `System.Diagnostics.ActivitySource` named `orders` and keyed `IActivitySource<T>` wrappers that use it. A consumer requesting the `orders` key receives that source, regardless of its generic type argument.

`AddNamedActivitySource` adds only keyed services. If your application also needs unkeyed `IActivitySource<T>`, keep its `AddBindingsByConvention()` or `AddSelfBindings()` setup, or explicitly register `IActivitySource<>` to `UnkeyedActivitySource<>`.

## Register an instrumentation scope version

If you version your instrumentation, use this call instead of the unversioned registration above. Choose the version when you first register the name:

```csharp
services.AddNamedActivitySource("orders", version: "1.2.3");
```

The underlying source exposes `Version == "1.2.3"`. The overload without a version remains supported and uses an empty-string version. Registering `orders` a second time does not update its version.

## Resolve the named activity source in consumers

Use the registration name in `[FromKeyedServices]` on the consumer's constructor parameter:

```csharp
using Cratis.Traces;
using Microsoft.Extensions.DependencyInjection;

public class OrderService([FromKeyedServices("orders")] IActivitySource<OrderService> activitySource)
{
}
```

When dependency injection creates `OrderService`, its typed source wraps the registered `orders` source: `activitySource.ActualSource.Name` is `orders`. Use this injected source with the span methods described in the [tracing overview](index.md).

Your telemetry provider must subscribe to `orders` to collect its spans. The shared Cratis OpenTelemetry setup subscribes to `Cratis.*`, which does not match `orders`. Add `orders` explicitly to your provider's source subscription, or choose a `Cratis.<Product>` source name that the provider already subscribes to. Follow [Set up OpenTelemetry](../opentelemetry.md) to configure the provider and export destination.

Unkeyed sources remain separate: they use `typeof(T).FullName`, falling back to `typeof(T).Name`, rather than the named source. Those names also need a matching provider subscription; removing `[FromKeyedServices("orders")]` does not select the `orders` source.

## Registration boundaries

The name is both the diagnostics source name and the dependency-injection key. Use the same string when registering and resolving the source, and ensure the telemetry provider's subscription matches it. A null, empty, or whitespace-only name is rejected.

Different names create separate singleton sources. Registration uses `TryAddKeyedSingleton`, so registering the same name again preserves its existing services. Source registration alone neither subscribes a telemetry provider nor configures an exporter.

## See also

- [Overview](index.md) — typed traces APIs and usage
- [Roslyn](roslyn.md) — generate instrumentation methods
- [Named meters](../metrics/named-registration.md) — configure the other diagnostics signal
