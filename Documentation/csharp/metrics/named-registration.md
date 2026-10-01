# Named meter registration

Use named registration when you want multiple logical meter sources in the same application and resolve them explicitly with keyed services.

## Register a named meter

```csharp
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
public class OrderService([FromKeyedServices("orders")] IMeter<OrderService> meter)
{
}
```

`IMeter<T>` remains available as a non-keyed service as well, where the underlying meter name defaults to `typeof(T).FullName`. These per-type meters are not subscribed by default. Explicitly subscribe that name in your telemetry provider (for example, with `AddMeter` in OpenTelemetry), or use a named meter that your provider subscribes to.
