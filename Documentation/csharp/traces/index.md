# Traces

The Cratis Fundamentals library provides a typed tracing API on top of `.NET` activities. You work with `IActivitySource<T>` and `IActivityScope<T>` instead of raw `ActivitySource` and `Activity`, and you can generate spans from partial methods with the `Cratis.Metrics.Roslyn` package.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
- [Using Source Generation](#using-source-generation)
- [Examples](#examples)
- [Best Practices](#best-practices)

## Overview

The tracing system gives you:

- **Typed activity sources** through `IActivitySource<T>`
- **Disposable activity scopes** through `IActivityScope<T>`
- **Source-generated spans** with `[Span]`
- **Automatic tags** from method parameters
- **Ambient parent-child relationships** through `Activity.Current`

## Getting Started

To use traces in your project:

1. Add a reference to `Cratis.Fundamentals`
2. Add a reference to `Cratis.Metrics.Roslyn` if you want source-generated span methods
3. Register your services with `AddBindingsByConvention()` or `AddSelfBindings()`
4. Inject `IActivitySource<T>` into the service that creates spans

```csharp
var services = new ServiceCollection();

services.AddBindingsByConvention();
```

## Core Concepts

### Activity sources

An activity source is the typed entry point for creating spans:

```csharp
public interface IActivitySource<T>
{
    ActivitySource ActualSource { get; }
}
```

When you resolve `IActivitySource<T>` from dependency injection, Fundamentals creates an `ActivitySource` named after `T`.

### Activity scopes

An activity scope wraps the underlying activity and stops it when you dispose it:

```csharp
public interface IActivityScope<T> : IDisposable
{
    Activity? Activity { get; }
}
```

Use `using` so the activity stops even when an exception is thrown:

```csharp
using var scope = OrderTraces.ProcessOrder(_activitySource, orderId, customerId);
```

### Ambient span relationships

`ActivitySource.StartActivity()` automatically uses `Activity.Current` as the parent. That means nested generated spans form a hierarchy without passing parent spans around manually.

```csharp
using var orderScope = OrderTraces.ProcessOrder(_activitySource, order.Id, order.CustomerId);

using var validationScope = OrderTraces.ValidateOrder(_activitySource, order.Id);
```

## Using Source Generation

The `Cratis.Metrics.Roslyn` package can implement your span methods at compile time.

1. Create a partial class
2. Add `static partial` methods that return `IActivityScope<T>`
3. Decorate them with `[Span]`
4. Pass `IActivitySource<T>` as the first parameter

```csharp
using Cratis.Traces;
using System.Diagnostics;

namespace MyApplication.Orders;

public static partial class OrderTraces
{
    [Span("order.process", ActivityKind.Server)]
    internal static partial IActivityScope<OrderService> ProcessOrder(
        IActivitySource<OrderService> source,
        string orderId,
        string customerId);

    [Span("order.validate")]
    internal static partial IActivityScope<OrderService> ValidateOrder(
        IActivitySource<OrderService> source,
        string orderId);
}
```

The generator:

- starts the activity with the configured name and kind
- converts additional parameter names to `snake_case` tags
- returns an `ActivityScope<T>` that stops the activity on disposal

## Examples

### Creating spans in a service

```csharp
using Cratis.Traces;

namespace MyApplication.Orders;

public class OrderService(IActivitySource<OrderService> activitySource)
{
    readonly IActivitySource<OrderService> _activitySource = activitySource;

    public async Task Process(Order order)
    {
        using var processScope = OrderTraces.ProcessOrder(_activitySource, order.Id, order.CustomerId);

        await Validate(order);
        await Save(order);
    }

    async Task Validate(Order order)
    {
        using var validationScope = OrderTraces.ValidateOrder(_activitySource, order.Id);
        await Task.CompletedTask;
    }

    async Task Save(Order order)
    {
        await Task.CompletedTask;
    }
}

public record Order(string Id, string CustomerId);
```

### Working with the underlying activity

If you need to add extra tags or status details, use the wrapped activity directly:

```csharp
using var scope = OrderTraces.ProcessOrder(_activitySource, order.Id, order.CustomerId);

scope.Activity?.SetTag("retry_count", retryCount);
scope.Activity?.SetStatus(ActivityStatusCode.Ok);
```

## Best Practices

1. **Always use `using`** when you create an `IActivityScope<T>`
2. **Keep span names stable** so dashboards and queries do not drift
3. **Use low-cardinality tags** unless you explicitly need high-cardinality diagnostics
4. **Create nested spans for meaningful sub-operations**, not every line of code
5. **Keep trace declarations together** in a single partial class per area

## See Also

- [Roslyn Source Generator](roslyn.md)
- [.NET distributed tracing documentation](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing)
