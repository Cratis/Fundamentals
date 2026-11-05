---
title: Cratis Fundamentals tracing in .NET
description: Cratis Fundamentals adds typed .NET tracing sources and disposable span scopes. Register a source and generate span methods to instrument your services.
---
Cratis Fundamentals tracing lets you mark operations in a .NET service as spans: timed units of work that can form a trace of a request. Use `IActivitySource<T>` to create them and `IActivityScope<T>` to stop them when the work ends. The examples below instrument order processing and validation with generated span methods.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
- [Using Source Generation](#using-source-generation)
- [Examples](#examples)
- [Best Practices](#best-practices)

## Overview

A span records one operation, while its tags attach details you can use to investigate that operation. The typed API wraps .NET's `ActivitySource` and `Activity`; `[Span]` declarations let a source generator create the start-and-stop plumbing and turn method parameters into tags.

Nested spans use `Activity.Current`, the activity active in the current execution context, to connect a sub-operation to its parent. This lets you follow validation within order processing without passing an activity between methods.

## Getting Started

Reference `Cratis.Fundamentals` in your .NET project. For the generated methods used on this page:

1. Add a reference to `Cratis.Observability.SourceGenerators`.
2. Register your services with `AddBindingsByConvention()` or `AddSelfBindings()`.
3. Inject `IActivitySource<T>` into the service that creates spans.

```csharp
var services = new ServiceCollection();

services.AddBindingsByConvention();
```

Both registration methods make unkeyed `IActivitySource<T>` available through dependency injection. The registration excerpt above uses the `Cratis.DependencyInjection` and `Microsoft.Extensions.DependencyInjection` namespaces.

Registration makes a source available to your code; a telemetry provider must also subscribe to it to collect spans. Follow [Set up OpenTelemetry](../opentelemetry.md) for the host configuration. If several services should report under one source name, use [named activity source registration](named-registration.md).

## Core Concepts

### Activity sources

`IActivitySource<T>` gives code associated with `T` access to the underlying .NET activity source:

```csharp
public interface IActivitySource<T>
{
    ActivitySource ActualSource { get; }
}
```

An unkeyed source uses `typeof(T).FullName` as its name, falling back to `typeof(T).Name` if no full name is available. For `OrderService` below, the source name is `MyApplication.Orders.OrderService`. Subscribe to that name in your telemetry provider. The `Cratis.*` subscription in the shared OpenTelemetry setup does not match it.

Named sources let you choose a shared instrumentation scope instead of a per-type name. `AddNamedActivitySource("Cratis.MyProduct", version: "1.2.3")` sets the scope version; `AddNamedMeter("Cratis.MyProduct", version: "1.2.3")` does the same for metrics. Both APIs also have overloads without a version.

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

## Using Source Generation

The `Cratis.Observability.SourceGenerators` package implements partial span methods at compile time, so service code can call `_activitySource.ProcessOrder(...)` without constructing an activity itself.

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

Calling `ProcessOrder` starts an `order.process` activity with `ActivityKind.Server` and adds `order_id` and `customer_id` tags from the arguments. `ValidateOrder` starts `order.validate` with the default `ActivityKind.Internal`. Both return an `ActivityScope<T>` that stops the activity when disposed.

The generator converts parameter names to `snake_case` for tag keys. Use `[Cratis.Diagnostics.Tag("cratis.…")]` on a parameter when you need an explicit key. See the [Roslyn source generator guide](roslyn.md) for declaration details.

## Examples

### Creating spans in a service

With `OrderTraces` declared above, the service can place a scope around each operation. The `Process` scope remains active across both awaited calls, so the validation span is nested beneath the processing span.

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

### Working with the underlying activity

Use the wrapped activity to add details that are known only while processing, such as a retry count or the final status. `Activity` is nullable, so keep the null-conditional calls when updating it:

```csharp
using var scope = _activitySource.ProcessOrder(order.Id, order.CustomerId);

scope.Activity?.SetTag("retry_count", retryCount);
scope.Activity?.SetStatus(ActivityStatusCode.Ok);
```

## Best Practices

Keep span names stable so dashboards and queries continue to find the same operations. Add nested spans where a sub-operation needs its own timing or diagnostic context; tracing every line makes the useful boundaries harder to see.

Use `using` for every `IActivityScope<T>` so its lifetime follows the operation, including when an exception leaves the method. Keep related declarations in one partial class per area, using extension methods to keep service call sites readable.

Choose tags deliberately. Prefer values with a limited set of possibilities, such as an operation category. The order and customer IDs in these examples have many possible values; include identifiers only when you need that diagnostic detail, and avoid sensitive data.

## See Also

- [Roslyn Source Generator](roslyn.md)
- [.NET distributed tracing documentation](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing)
