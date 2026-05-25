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

## Resolve the named activity source in consumers

```csharp
public class OrderService([FromKeyedServices("orders")] IActivitySource<OrderService> activitySource)
{
}
```

`IActivitySource<T>` remains available as a non-keyed service as well, where the underlying source name defaults to the type name.
