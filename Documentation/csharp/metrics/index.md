---
title: Cratis Fundamentals metrics for .NET applications
description: Cratis Fundamentals adds typed meters and generated metric methods to .NET so you can count operations, record current values, and measure distributions.
---
<a id="metrics"></a>

Cratis Fundamentals metrics let you instrument .NET services with typed meters built on `System.Diagnostics.Metrics`. Use them to count events such as login attempts, record current values such as memory usage, or measure distributions such as request duration. The optional source generator turns attributed partial methods into calls to the .NET metrics API.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
- [Available Metrics](#available-metrics)
- [Using Source Generation](#using-source-generation)
- [Examples](#examples)

## Overview

Start with the question your monitoring needs to answer. A counter can tell you how many login attempts failed; tags such as authentication method let you break that count down. A tag is a key-value pair attached to a measurement.

Each service receives an `IMeter<T>`, where `T` identifies the service. From that meter, metric methods record measurements through counters, gauges, or histograms. When several measurements need the same tags, pass a meter scope to those methods to carry the shared context.

Recording measurements and exporting them are separate steps. Your telemetry provider must subscribe to the meter and have an exporter configured before the measurements can reach your monitoring system.

## Getting Started

The metric-method examples on this page use source generation:

1. Reference `Cratis.Fundamentals` for the typed meters and metric attributes.
2. Reference `Cratis.Observability.SourceGenerators` to generate the partial method implementations.
3. Register a meter in dependency injection, as shown below.
4. Declare methods in a static partial class, then call them from your service with its injected meter.

If you do not use the generator, access the underlying .NET meter through `IMeter<T>.ActualMeter` to create instruments yourself. The registration below is an excerpt for a service collection named `services` and your application's `MyService` type.

```csharp
using Cratis.Metrics;
using Microsoft.Extensions.DependencyInjection;

// Add to your service registration
services.AddSingleton<IMeter<MyService>, UnkeyedMeter<MyService>>();
```

Register `IMeter<T>` with its `UnkeyedMeter<T>` implementation so dependency injection can resolve it. If you already call `AddBindingsByConvention()` or `AddSelfBindings()` from `Cratis.DependencyInjection`, those methods register the open generic pair `IMeter<>` to `UnkeyedMeter<>` for you. You can then omit the explicit registration.

To give several services a shared meter name, use [named meter registration](named-registration.md). That page also explains how to resolve a meter by its dependency-injection key.

## Core Concepts

### Meters

A meter creates instruments: the counters, gauges, and histograms that record measurements. The typed wrapper associates a meter with a class or service:

```csharp
public interface IMeter<T>
{
    Meter ActualMeter { get; }
}
```

`UnkeyedMeter<T>` uses `typeof(T).FullName` as its meter name, falling back to `typeof(T).Name` if the full name is unavailable. Subscribe to that name in your telemetry provider, or use a [named meter](named-registration.md) whose name your provider subscribes to. Registering a meter alone does not subscribe it.

### Meter Scopes

A meter scope pairs a meter with shared tags. Pass the scope as the first argument to generated metric methods that should include those tags:

```csharp
public interface IMeterScope<T> : IDisposable
{
    Meter Meter { get; }
    IDictionary<string, object> Tags { get; }
}
```

Create a scope from a dictionary or an anonymous object. A scope is passed explicitly; creating one does not change other calls made with the original meter.

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

With an anonymous object, its property names become tag keys. C# property name inference also works:

```csharp
// Property names are inferred from the source properties
using var scope = meter.BeginScope(new 
{ 
    request.Method,
    Result = "success"
});
```

### Tags

Use tags to filter or group measurements, for example by login result or HTTP method. Generated methods collect tags from their parameters and, when passed an `IMeterScope<T>`, from that scope.

Keep each tag's possible values bounded. Result names, HTTP methods, and route templates work better than user, tenant, request, or entity IDs. Each distinct combination of tag values can create a separate time series; the number of combinations is called cardinality. Scope tags contribute just as method tags do. See [metric views and cardinality](../opentelemetry.md#metric-views-and-cardinality) to set budgets and handle overflow.

The following declarations are excerpts for a static partial class. They assume the illustrated service types and a `Cratis.Metrics` import.

## Available Metrics

### Counters

Counters accumulate non-negative increments. Use one when you need the number of events, requests, or errors. Without a measurement parameter, the generated method adds one per call:

```csharp
[Counter<int>("user_logins", "Number of user login attempts")]
static partial void LogUserLogin(IMeter<AuthService> meter, string method, string result);
```

### Gauges

Gauges record current values that can rise or fall, such as memory usage or active connections:

```csharp
[Gauge<double>("memory_usage_bytes", "Current memory usage in bytes")]
static partial void RecordMemoryUsage(IMeter<MemoryService> meter, double value);
```

### Histograms

Histograms record measurements for a distribution, such as request durations or payload sizes. Supply a measurement parameter matching the attribute's generic type. The optional third attribute argument declares a unit using UCUM, the Unified Code for Units of Measure:

```csharp
[Histogram<double>("request_duration", "Request duration", "s")]
static partial void RecordRequestDuration(IMeter<WebService> meter, double seconds, string outcome);
```

Use `IMeterScope<T>` to include scope tags. Omitting the unit leaves the instrument's unit `null`.

## Using Source Generation

The `Cratis.Observability.SourceGenerators` package implements attributed metric methods at compile time. Put the declarations in a static partial class and mark each static partial method with `[Counter<T>]`, `[Gauge<T>]`, or `[Histogram<T>]`. You then call the method from your service instead of creating and looking up the instrument yourself.

### Method Signature Requirements

Both the method and its containing class must be `static partial`. The first parameter must be `IMeter<T>` or `IMeterScope<T>`.

The first remaining parameter whose type matches the attribute's generic type supplies the measurement. Gauges and histograms require it; counters add one when it is absent. All other parameters become tags. In the example below, `count` supplies the `user_operations` increment, and `duration` supplies the `scoped_operations` increment despite its name. Use a histogram when you want a duration distribution.

For callable methods, choose an access modifier that allows the service to call them, as the service examples below do with `internal`.

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

## Examples

These service excerpts show where to record measurements in application code. They assume `Cratis.Metrics` is imported and the generator package is referenced. Supply your application's domain types and operations, such as `Order`, `ValidateCredentialsAsync`, and `SaveOrderAsync`; those implementations are not included here.

### Basic Counter Usage

Record the result after checking the credentials, and count an exception as an error before rethrowing it:

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

### Using Scoped Metrics

Create a scope once for the order operation, then pass it to each metric method. The generated methods combine the shared `operation` and `order_type` tags with any method-specific tags:

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

Record each memory reading as a current value. This excerpt uses a timer to take readings every 30 seconds:

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

## Best Practices

Name each instrument for what it measures, and use its description to explain the measurement. Declare units with the optional third attribute argument, such as `"By"` for bytes or `"s"` for seconds, so collectors receive the unit as metadata.

Group related methods in a static partial class. When the same context belongs on several measurements, use a scope to avoid repeating tags at every call. Keep those tags bounded regardless of whether they come from a scope or a method parameter.

## See Also

Continue with the [metrics source generator guide](roslyn.md) for declaration rules, generated code, and diagnostics. To collect and export your measurements, use the [OpenTelemetry setup](../opentelemetry.md).

For the underlying instrument model, see Microsoft's [.NET metrics documentation](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/metrics).
