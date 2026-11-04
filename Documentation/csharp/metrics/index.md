---
description: Track application metrics in .NET with a typed API over System.Diagnostics.Metrics, with optional source generation for metric methods.
---
# Metrics

When a login succeeds or fails, record that outcome without wiring metric instruments into the authentication logic. Cratis Fundamentals provides typed meters over .NET's System.Diagnostics.Metrics; optional source generation supplies the recording methods.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Examples](#examples)
- [Core Concepts](#core-concepts)
- [Available Metrics](#available-metrics)
- [Using Source Generation](#using-source-generation)

## Overview

Choose a meter, record an outcome and configure how the host collects it. The pieces used in that task are:

- **Type-safe metrics** through generic interfaces
- **Source generation** for automatic metrics implementation
- **Scoped metrics** for contextual measurements
- **Tag support** for dimensional metrics
- **Counter, Gauge, and Histogram instruments** with standardized APIs

## Getting Started

Choose unkeyed registration for a service type, or keyed named registration when consumers should share a logical source. Explicit registration and convention-based registration are alternative unkeyed setup paths, not consecutive requirements. Before recording, subscribe the chosen meter name in your provider; see [Set up OpenTelemetry](../opentelemetry.md).

Unkeyed `IMeter<T>` uses `typeof(T).FullName` as its meter name. These per-type meters are not subscribed by default: explicitly subscribe the name in your telemetry provider, or use a subscribed [named meter](named-registration.md).

`Cratis.*` only subscribes matching names; it does not automatically include application type names. Registration, recording, provider collection and export are separate steps.

Then prepare the generated counter example:

1. Add a reference to the `Cratis.Fundamentals` package
2. Add the `Cratis.Observability.SourceGenerators` source generator package (optional; required for the generated examples below)
3. Declare your metrics methods if you choose source generation
4. Resolve the typed meter for the service you instrument

```csharp
using Cratis.Metrics;
using Microsoft.Extensions.DependencyInjection;

// Add to your service registration
services.AddSingleton<IMeter<MyService>, UnkeyedMeter<MyService>>();
```

`IMeter<T>` is an interface, so registering it alone cannot be resolved; pair it with the `UnkeyedMeter<T>` implementation. If you already call `AddBindingsByConvention()` or `AddSelfBindings()` (both from `Cratis.DependencyInjection`), the open generic pair `IMeter<>` to `UnkeyedMeter<>` is registered for you and you do not need the explicit registration.

For keyed named meter configuration, see [Named Registration](named-registration.md).

## Examples

### Basic Counter Usage

Use the generated counter from the service after registering its typed meter and subscribing that meter name in your provider. This excerpt uses `AuthService` rather than the `MyService` registration placeholder above; it assumes the surrounding authentication methods and namespaces.

```csharp
public class AuthService
{
    private readonly IMeter<AuthService> _meter;

    public AuthService(IMeter<AuthService> meter)
    {
        _meter = meter;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var success = await ValidateCredentialsAsync(username, password);
            
            // Count successful logins
            if (success)
            {
                AuthMetrics.CountLogin(_meter, "success", "password");
            }
            else
            {
                AuthMetrics.CountLogin(_meter, "failure", "password");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            AuthMetrics.CountLogin(_meter, "error", "password");
            throw;
        }
    }
}

public static partial class AuthMetrics
{
    [Counter<int>("auth_attempts", "Authentication attempts by result")]
    internal static partial void CountLogin(IMeter<AuthService> meter, string result, string method);
}
```

Each outcome records the authentication-attempt counter with result and method tags. A recording call is not proof that a backend received data: meter subscription, collection and exporter configuration still need to be in place.

### Using Scoped Metrics

Use a scope when several calls need the same bounded tags. Disposing the scope does not flush measurements or dispose its meter. Keep the scope local to the operation; these excerpts do not establish thread-safe sharing.

```csharp
public class OrderService
{
    private readonly IMeter<OrderService> _meter;

    public OrderService(IMeter<OrderService> meter)
    {
        _meter = meter;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Using anonymous type for cleaner syntax
        using var scope = _meter.BeginScope(new
        {
            operation = "process",
            order_type = order.Type
        });

        // All metrics within this scope will include the operation and order_type tags
        OrderMetrics.StartProcessing(scope);
        
        try
        {
            await ValidateOrderAsync(order);
            OrderMetrics.CountValidation(scope, "success");
            
            await SaveOrderAsync(order);
            OrderMetrics.CountSave(scope, "success");
            
            OrderMetrics.CompleteProcessing(scope);
        }
        catch (ValidationException)
        {
            OrderMetrics.CountValidation(scope, "failure");
            throw;
        }
        catch (Exception)
        {
            OrderMetrics.CountSave(scope, "failure");
            throw;
        }
    }
}

public static partial class OrderMetrics
{
    [Counter<int>("order_processing_started", "Orders that started processing")]
    internal static partial void StartProcessing(IMeterScope<OrderService> scope);

    [Counter<int>("order_processing_completed", "Orders that completed processing")]
    internal static partial void CompleteProcessing(IMeterScope<OrderService> scope);

    [Counter<int>("order_validation", "Order validation attempts")]
    internal static partial void CountValidation(IMeterScope<OrderService> scope, string result);

    [Counter<int>("order_save", "Order save attempts")]
    internal static partial void CountSave(IMeterScope<OrderService> scope, string result);
}
```

### Gauge Metrics

These generated gauges record values when called; they do not register observable callbacks for provider polling. The timer below schedules those calls. Its excerpt omits timer disposal and is not a service-lifetime template. Choose units to match recorded values; the gauge declarations below omit the optional unit.

```csharp
public class MemoryMonitorService
{
    private readonly IMeter<MemoryMonitorService> _meter;
    private readonly Timer _timer;

    public MemoryMonitorService(IMeter<MemoryMonitorService> meter)
    {
        _meter = meter;
        _timer = new Timer(RecordMemoryUsage, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void RecordMemoryUsage(object? state)
    {
        var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64;
        var privateMemory = process.PrivateMemorySize64;

        SystemMetrics.RecordWorkingSet(_meter, workingSet);
        SystemMetrics.RecordPrivateMemory(_meter, privateMemory);
    }
}

public static partial class SystemMetrics
{
    [Gauge<long>("memory_working_set", "Current working set memory in bytes")]
    internal static partial void RecordWorkingSet(IMeter<MemoryMonitorService> meter, long bytes);

    [Gauge<long>("memory_private", "Current private memory in bytes")]
    internal static partial void RecordPrivateMemory(IMeter<MemoryMonitorService> meter, long bytes);
}
```

## Core Concepts

### Meters

A meter is a factory for creating and managing metrics instruments. Each meter is typed to a specific class or service:

```csharp
public interface IMeter<T>
{
    Meter ActualMeter { get; }
}
```

### Meter Scopes

Meter scopes provide contextual metrics with associated tags:

```csharp
public interface IMeterScope<T> : IDisposable
{
    Meter Meter { get; }
    IDictionary<string, object> Tags { get; }
}
```

Use scopes to add context to your metrics. You can use either a dictionary or an anonymous type:

```csharp
// Using a dictionary
using var scope = meter.BeginScope(new Dictionary<string, object>
{
    ["result"] = "success",
    ["operation"] = "login"
});

// Using an anonymous type (recommended for cleaner syntax)
using var scope = meter.BeginScope(new
{
    result = "success",
    operation = "login"
});
```

The anonymous type approach provides a more concise syntax and supports property name inference:

```csharp
// Property names are inferred from the source properties
using var scope = meter.BeginScope(new
{
    request.Method,
    Result = "success"
});
```

### Tags

Tags allow you to add dimensions to your metrics for better filtering and analysis. Tags can be:

- Added at the scope level (applied to all metrics within the scope)
- Added at the individual metric level through method parameters

Choose bounded tags such as result, HTTP method or a route template, not user, tenant, request or entity IDs. Scope tags have the same cardinality cost as method tags. See [metric views and cardinality](../opentelemetry.md#metric-views-and-cardinality) for budgets, allowed tags and overflow handling. The excerpts below assume the illustrated service and domain types; use bounded vocabularies for their dimension values.

## Available Metrics

### Counters

Counters track cumulative values that only increase. Use for counting events, requests, errors, etc.

```csharp
[Counter<int>("user_logins", "Number of user login attempts")]
static partial void LogUserLogin(IMeter<AuthService> meter, string method, string result);
```

### Gauges

Gauges track values that can go up and down. Use for measuring current state like memory usage, active connections, etc.

```csharp
[Gauge<double>("memory_usage_bytes", "Current memory usage in bytes")]
static partial void RecordMemoryUsage(IMeter<MemoryService> meter, double value);
```

### Histograms

Histograms record distributions of measurements such as request durations or payload sizes. A measurement parameter matching the attribute's generic type is required; the optional third argument declares a UCUM unit.

```csharp
[Histogram<double>("request_duration", "Request duration", "s")]
static partial void RecordRequestDuration(IMeter<WebService> meter, double seconds, string outcome);
```

Use `IMeterScope<T>` to include scope tags. Omitting the unit leaves the instrument's unit `null`.

## Using Source Generation

The `Cratis.Observability.SourceGenerators` source generator automatically implements your metrics methods. To use it:

1. Create a static partial class
2. Define partial methods with appropriate attributes
3. The source generator will implement the methods

### Method Signature Requirements

All metrics methods must follow these rules:

1. **First parameter** must be either `IMeter<T>` or `IMeterScope<T>`
2. **Value parameter** (required for gauges and histograms) must match the generic type in the attribute
3. **Additional parameters** become tags automatically
4. Methods must be `static partial`
5. The containing class must be declared `static partial`. The generator skips classes that are not both `static` and `partial`.

### Example Implementation

```csharp
using Cratis.Metrics;

namespace MyApplication.Services;

public static partial class UserMetrics
{
    [Counter<int>("user_operations", "Track user operations")]
    static partial void CountUserOperation(
        IMeter<UserService> meter,
        string operationType,
        string result,
        int count = 1);

    [Gauge<double>("active_sessions", "Number of active user sessions")]
    static partial void RecordActiveSessions(
        IMeter<UserService> meter,
        double value,
        string region);

    // Using with scopes
    [Counter<long>("scoped_operations", "Operations within a scope")]
    static partial void CountScopedOperation(
        IMeterScope<UserService> scope,
        string operation,
        long duration);
}
```

## Best Practices

1. **Use meaningful names** for your metrics that clearly describe what they measure
2. **Declare UCUM units** with the optional third attribute argument (e.g., `"By"` for bytes or `"s"` for seconds), rather than putting units only in descriptions
3. **Keep tag cardinality low** to avoid performance issues
4. **Use scopes** for contextual metrics to reduce tag duplication
5. **Group related metrics** in the same static partial class for organization
6. **Use appropriate metric types** - counters for cumulative values, gauges for current state, histograms for distributions

## See Also

- [Roslyn Source Generator](roslyn.md) - Detailed guide on the source generation capabilities
- [.NET Metrics Documentation](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/metrics) - Official Microsoft documentation
