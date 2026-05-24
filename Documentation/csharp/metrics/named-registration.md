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

## Resolve the named meter in consumers

```csharp
public class OrderService([FromKeyedServices("orders")] IMeter<OrderService> meter)
{
}
```

`IMeter<T>` remains available as a non-keyed service as well, where the underlying meter name defaults to the type name.
