# Metrics

The Cratis Fundamentals library provides a comprehensive metrics system for tracking and monitoring your application's performance and behavior. This system is built on top of .NET's System.Diagnostics.Metrics and provides a developer-friendly API with source generation capabilities.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
- [Available Metrics](#available-metrics)
- [Using Source Generation](#using-source-generation)
- [Examples](#examples)

## Overview

The metrics system in Cratis Fundamentals offers:

- **Type-safe metrics** through generic interfaces
- **Source generation** for automatic metrics implementation
- **Scoped metrics** for contextual measurements
- **Tag support** for dimensional metrics
- **Counter and Gauge instruments** with standardized APIs

## Getting Started

To start using the metrics system in your project:

1. Add a reference to the `Cratis.Fundamentals` package
2. Add the `Cratis.Metrics.Roslyn` source generator package (optional but recommended)
3. Define your metrics interface
4. Use dependency injection to get access to your metrics

```csharp
// Add to your service registration
services.AddSingleton<IMeter<MyService>>();
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

Use scopes to add context to your metrics:

```csharp
using var scope = meter.BeginScope(new Dictionary<string, object> 
{ 
    ["user_id"] = userId,
    ["operation"] = "login"
});
```

### Tags

Tags allow you to add dimensions to your metrics for better filtering and analysis. Tags can be:

- Added at the scope level (applied to all metrics within the scope)
- Added at the individual metric level through method parameters

## Available Metrics

### Counters

Counters track cumulative values that only increase. Use for counting events, requests, errors, etc.

```csharp
[Counter<int>("user_logins", "Number of user login attempts")]
static partial void LogUserLogin(IMeter<AuthService> meter, string userId, string result);
```

### Gauges

Gauges track values that can go up and down. Use for measuring current state like memory usage, active connections, etc.

```csharp
[Gauge<double>("memory_usage_bytes", "Current memory usage in bytes")]
static partial void RecordMemoryUsage(IMeter<MemoryService> meter, double value);
```

## Using Source Generation

The `Cratis.Metrics.Roslyn` source generator automatically implements your metrics methods. To use it:

1. Create a partial class
2. Define partial methods with appropriate attributes
3. The source generator will implement the methods

### Method Signature Requirements

All metrics methods must follow these rules:

1. **First parameter** must be either `IMeter<T>` or `IMeterScope<T>`
2. **Value parameter** (for gauges) must match the generic type in the attribute
3. **Additional parameters** become tags automatically
4. Methods must be `static partial`

### Example Implementation

```csharp
using Cratis.Metrics;

namespace MyApplication.Services;

public partial class UserMetrics
{
    [Counter<int>("user_operations", "Track user operations")]
    static partial void CountUserOperation(
        IMeter<UserService> meter, 
        string operationType, 
        string userId, 
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

### Basic Counter Usage

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
                AuthMetrics.CountLogin(_meter, "success", username);
            }
            else
            {
                AuthMetrics.CountLogin(_meter, "failure", username);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            AuthMetrics.CountLogin(_meter, "error", username);
            throw;
        }
    }
}

public partial class AuthMetrics
{
    [Counter<int>("auth_attempts", "Authentication attempts by result")]
    static partial void CountLogin(IMeter<AuthService> meter, string result, string username);
}
```

### Using Scoped Metrics

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
        using var scope = _meter.BeginScope(new Dictionary<string, object>
        {
            ["customer_id"] = order.CustomerId,
            ["order_type"] = order.Type
        });

        // All metrics within this scope will include the customer_id and order_type tags
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

public partial class OrderMetrics
{
    [Counter<int>("order_processing_started", "Orders that started processing")]
    static partial void StartProcessing(IMeterScope<OrderService> scope);

    [Counter<int>("order_processing_completed", "Orders that completed processing")]
    static partial void CompleteProcessing(IMeterScope<OrderService> scope);

    [Counter<int>("order_validation", "Order validation attempts")]
    static partial void CountValidation(IMeterScope<OrderService> scope, string result);

    [Counter<int>("order_save", "Order save attempts")]
    static partial void CountSave(IMeterScope<OrderService> scope, string result);
}
```

### Gauge Metrics

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

public partial class SystemMetrics
{
    [Gauge<long>("memory_working_set", "Current working set memory in bytes")]
    static partial void RecordWorkingSet(IMeter<MemoryMonitorService> meter, long bytes);

    [Gauge<long>("memory_private", "Current private memory in bytes")]
    static partial void RecordPrivateMemory(IMeter<MemoryMonitorService> meter, long bytes);
}
```

## Best Practices

1. **Use meaningful names** for your metrics that clearly describe what they measure
2. **Include units** in metric descriptions (e.g., "bytes", "seconds", "count")
3. **Keep tag cardinality low** to avoid performance issues
4. **Use scopes** for contextual metrics to reduce tag duplication
5. **Group related metrics** in the same partial class for organization
6. **Use appropriate metric types** - counters for cumulative values, gauges for current state

## See Also

- [Roslyn Source Generator](roslyn.md) - Detailed guide on the source generation capabilities
- [.NET Metrics Documentation](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/metrics) - Official Microsoft documentation
