---
description: Create spans in .NET with typed IActivitySource and IActivityScope instead of raw ActivitySource and Activity, optionally source generated.
---
# Traces

When order processing calls validation, you want to see which work belongs to the same operation without passing activities through every method. Cratis Fundamentals wraps `.NET` activities with `IActivitySource<T>` and `IActivityScope<T>` instead of raw `ActivitySource` and `Activity`. Optional span generation with `Cratis.Observability.SourceGenerators` keeps declarations separate from service calls.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Using Source Generation](#using-source-generation)
- [Examples](#examples)
- [Core Concepts](#core-concepts)
- [Best Practices](#best-practices)

## Overview

Declare the processing and validation spans, call them from the service, and dispose each scope when its work ends. These pieces connect that work to its spans:

- **Typed activity sources** through `IActivitySource<T>`
- **Disposable activity scopes** through `IActivityScope<T>`
- **Source-generated spans** with `[Span]`
- **Automatic tags** from method parameters
- **Ambient parent-child relationships** through `Activity.Current`

## Getting Started

Before instrumenting work, configure how your host listens to activity sources.

When you resolve unkeyed `IActivitySource<T>` from dependency injection, Fundamentals creates an `ActivitySource` named after `T`. Per-type sources are not subscribed by `Cratis.*` unless the source name matches that configured wildcard. Explicitly subscribe to the source name in your telemetry provider, or use a named source under a `Cratis.<Product>` scope that your provider subscribes to. Registering the source does not configure a provider; see [Set up OpenTelemetry](../opentelemetry.md).

For the unkeyed OrderService example below, the source name is `MyApplication.Orders.OrderService`; it does not match `Cratis.*`. Subscribe to that application source explicitly if you want your provider to collect its spans.

Then prepare the service:

1. Add a reference to `Cratis.Fundamentals`
2. Add a reference to `Cratis.Observability.SourceGenerators` if you want source-generated span methods
3. Register your services with `AddBindingsByConvention()` or `AddSelfBindings()`
4. Inject `IActivitySource<T>` into the service that creates spans

```csharp
var services = new ServiceCollection();

services.AddBindingsByConvention();
```

For keyed named activity source configuration, see [Named Registration](named-registration.md).

## Using Source Generation

If you choose source generation, declare the span methods before calling them from the service. The optional `Cratis.Observability.SourceGenerators` package supplies their implementations; use the [Roslyn Source Generator](roslyn.md) reference for signature rules, tags and diagnostics.

1. Create a static partial class
2. Add `static partial` methods that return `IActivityScope<T>`
3. Decorate them with `[Span]`
4. Use `this IActivitySource<T>` as the first parameter so the generated spans become extension methods

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

The generator:

- starts the activity with the configured name and kind
- uses `[Cratis.Diagnostics.Tag("cratis.…")]` for explicit attribute keys, or converts untagged parameter names to `snake_case` for compatibility
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
        using var processScope = _activitySource.ProcessOrder(order.Id, order.CustomerId);

        await Validate(order);
        await Save(order);
    }

    async Task Validate(Order order)
    {
        using var validationScope = _activitySource.ValidateOrder(order.Id);
        await Task.CompletedTask;
    }

    async Task Save(Order order)
    {
        await Task.CompletedTask;
    }
}

public record Order(string Id, string CustomerId);
```

The processing scope stays open while Validate and Save run. Validate opens and disposes its own scope before returning; disposing the processing scope stops the outer activity. When activities are created, validation appears beneath processing through `Activity.Current`. These snippets show instrumentation, not collected output; provider subscription, sampling and export configuration still determine what you can observe.

### Working with the underlying activity

If you need to add extra tags or status details, use the wrapped activity directly:

```csharp
using var scope = _activitySource.ProcessOrder(order.Id, order.CustomerId);

scope.Activity?.SetTag("retry_count", retryCount);
scope.Activity?.SetStatus(ActivityStatusCode.Ok);
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

`AddNamedActivitySource("Cratis.MyProduct", version: "1.2.3")` and `AddNamedMeter("Cratis.MyProduct", version: "1.2.3")` set the instrumentation scope version. The existing overloads without a version remain supported.

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
using var scope = _activitySource.ProcessOrder(orderId, customerId);
```

### Ambient span relationships

`ActivitySource.StartActivity()` automatically uses `Activity.Current` as the parent. That means nested generated spans form a hierarchy without passing parent spans around manually.

```csharp
using var orderScope = _activitySource.ProcessOrder(order.Id, order.CustomerId);
using var validationScope = _activitySource.ValidateOrder(order.Id);
```

## Best Practices

1. **Always use `using`** when you create an `IActivityScope<T>`
2. **Keep span names stable** so dashboards and queries do not drift
3. **Use low-cardinality tags** unless you explicitly need high-cardinality diagnostics
4. **Create nested spans for meaningful sub-operations**, not every line of code
5. **Prefer extension-method span declarations** so call sites read as `_activitySource.ProcessOrder(...)`
6. **Keep trace declarations together** in a single partial class per area

## See Also

- [Roslyn Source Generator](roslyn.md)
- [.NET distributed tracing documentation](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing)
