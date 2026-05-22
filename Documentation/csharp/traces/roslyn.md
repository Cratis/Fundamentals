# Trace Source Generation

The `Cratis.Metrics.Roslyn` package can generate trace span implementations from partial methods decorated with `[Span]`. This keeps raw `ActivitySource` usage out of your business code and applies a consistent convention for span creation and tagging.

## Table of Contents

- [Installation](#installation)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Method Signature Requirements](#method-signature-requirements)
- [Generated Code](#generated-code)
- [Analyzer](#analyzer)
- [Diagnostics](#diagnostics)
- [Best Practices](#best-practices)

## Installation

Add the `Cratis.Metrics.Roslyn` package to your project:

```xml
<PackageReference Include="Cratis.Metrics.Roslyn" Version="[version]" />
```

Or use the `.NET` CLI:

```bash
dotnet add package Cratis.Metrics.Roslyn
```

## How It Works

At compile time, the generator looks for:

1. `static partial` methods in a `static partial` class
2. methods decorated with `[Span]`
3. methods whose first parameter is `IActivitySource<T>`
4. methods that return `IActivityScope<T>`

For each matching method, the generator:

- calls `source.ActualSource.StartActivity()`
- applies additional parameters as tags
- converts tag names to `snake_case`
- returns `ActivityScope<T>`

## Getting Started

Declare your spans in a partial class. Prefer extension methods so call sites stay close to the injected activity source:

```csharp
using Cratis.Traces;
using System.Diagnostics;

namespace MyApplication.Orders;

public static partial class OrderTraces
{
    [Span("order.process", ActivityKind.Server)]
    internal static partial IActivityScope<OrderService> ProcessOrder(
        this IActivitySource<OrderService> source,
        string orderId,
        string customerId);

    [Span("order.validate")]
    internal static partial IActivityScope<OrderService> ValidateOrder(
        this IActivitySource<OrderService> source,
        string orderId);
}
```

Use the generated methods like this:

```csharp
using var processScope = _activitySource.ProcessOrder(order.Id, order.CustomerId);
using var validationScope = _activitySource.ValidateOrder(order.Id);
```

## Method Signature Requirements

Every generated trace method must follow this pattern:

```csharp
[Span("name", ActivityKind.Internal | ActivityKind.Server | ActivityKind.Client | ActivityKind.Producer | ActivityKind.Consumer)]
static partial IActivityScope<TService> MethodName(
    this IActivitySource<TService> source,
    [tag parameters...]);
```

Rules:

1. The first parameter must be `IActivitySource<T>`
2. The return type must be `IActivityScope<T>`
3. Every parameter after the first becomes a tag
4. Methods must be `static partial`
5. The containing class must be `static partial`
6. Prefer marking the first parameter with `this` so the generated method becomes an extension method

### Tag naming

The generator converts parameter names to `snake_case` tag names:

- `orderId` becomes `order_id`
- `customerId` becomes `customer_id`
- `orderID` becomes `order_id`

## Generated Code

Given this declaration:

```csharp
[Span("order.process", ActivityKind.Server)]
internal static partial IActivityScope<OrderService> ProcessOrder(
    this IActivitySource<OrderService> source,
    string orderId,
    string customerId);
```

The generator emits code equivalent to:

```csharp
[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Cratis.Metrics.Roslyn", "1.0.0")]
internal static partial IActivityScope<OrderService> ProcessOrder(
    this IActivitySource<OrderService> source,
    string orderId,
    string customerId)
{
    var activity = source.ActualSource.StartActivity("order.process", ActivityKind.Server);
    activity?.SetTag("order_id", orderId);
    activity?.SetTag("customer_id", customerId);
    return new global::Cratis.Traces.ActivityScope<OrderService>(activity);
}
```

## Analyzer

The package also includes analyzer `CRT0001`.

### CRT0001

`IActivityScope<T>` must be assigned with `using` so the span is stopped correctly.

This produces a diagnostic:

```csharp
var scope = _activitySource.ProcessOrder(order.Id, order.CustomerId);
```

These patterns are valid:

```csharp
using var scope = _activitySource.ProcessOrder(order.Id, order.CustomerId);
```

```csharp
using (_activitySource.ProcessOrder(order.Id, order.CustomerId))
{
    await Process(order);
}
```

## Diagnostics

The trace generator reports these compile-time diagnostics:

### TRACES001

The method declares no parameters, so the required `IActivitySource<T>` first parameter is missing.

```csharp
[Span("order.process")]
internal static partial IActivityScope<OrderService> ProcessOrder();
```

### TRACES002

The first parameter is not `IActivitySource<T>`.

```csharp
[Span("order.process")]
internal static partial IActivityScope<OrderService> ProcessOrder(string source, string orderId);
```

### TRACES003

The method does not return `IActivityScope<T>`.

```csharp
[Span("order.process")]
internal static partial void ProcessOrder(IActivitySource<OrderService> source, string orderId);
```

## Best Practices

1. **Keep generated span declarations close to the service they support**
2. **Use names that describe work, not implementation details**
3. **Prefer a small number of meaningful tags**
4. **Use `ActivityKind` explicitly for public boundaries such as server and client spans**
5. **Prefer extension methods on `IActivitySource<T>` for cleaner call sites**
6. **Treat `CRT0001` as required, not optional**
