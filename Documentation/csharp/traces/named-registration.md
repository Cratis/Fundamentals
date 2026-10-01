# Named activity source registration

Use named registration when you want multiple logical activity sources in the same application and resolve them explicitly with keyed services.

## Register a named activity source

```csharp
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
public class OrderService([FromKeyedServices("orders")] IActivitySource<OrderService> activitySource)
{
}
```

`IActivitySource<T>` remains available as a non-keyed service as well, where the underlying source name defaults to `typeof(T).FullName`. Per-type sources are not subscribed by default; explicitly subscribe the source name in your telemetry provider, or use a named source that your provider subscribes to.
