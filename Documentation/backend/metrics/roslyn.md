# Metrics Roslyn Source Generator

The `Cratis.Metrics.Roslyn` package provides a powerful source generator that automatically implements metrics methods for you. This eliminates boilerplate code and ensures consistent, efficient metrics collection across your application.

## Table of Contents

- [Installation](#installation)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Method Signature Requirements](#method-signature-requirements)
- [Generated Code](#generated-code)
- [Error Handling](#error-handling)
- [Advanced Usage](#advanced-usage)
- [Troubleshooting](#troubleshooting)

## Installation

Add the `Cratis.Metrics.Roslyn` NuGet package to your project:

```xml
<PackageReference Include="Cratis.Metrics.Roslyn" Version="[version]" />
```

Or via the .NET CLI:

```bash
dotnet add package Cratis.Metrics.Roslyn
```

## How It Works

The source generator analyzes your code at compile time and looks for:

1. **Partial classes** containing partial methods
2. **Methods decorated** with `[Counter<T>]` or `[Gauge<T>]` attributes
3. **Proper method signatures** that follow the required pattern

For each qualifying method, it generates a complete implementation that:

- Creates the appropriate metrics instrument (Counter or Gauge)
- Handles tag collection from method parameters and scopes
- Calls the underlying .NET metrics APIs
- Includes proper null checks and error handling

## Getting Started

### 1. Create a Partial Class

Create a partial class to contain your metrics methods:

```csharp
using Cratis.Metrics;

namespace MyApplication.Metrics;

public partial class UserMetrics
{
    // Metrics methods will be defined here
}
```

### 2. Define Partial Methods with Attributes

Add partial method declarations with the appropriate attributes:

```csharp
public partial class UserMetrics
{
    [Counter<int>("user_registrations", "Number of user registrations")]
    static partial void CountUserRegistration(IMeter<UserService> meter, string source, string country);

    [Gauge<double>("active_sessions", "Current number of active sessions")]
    static partial void RecordActiveSessions(IMeter<UserService> meter, double value, string region);
}
```

### 3. Use in Your Services

Inject the meter and use your metrics methods:

```csharp
public class UserService
{
    private readonly IMeter<UserService> _meter;

    public UserService(IMeter<UserService> meter)
    {
        _meter = meter;
    }

    public async Task RegisterUserAsync(string email, string country, string source)
    {
        // ... registration logic ...
        
        UserMetrics.CountUserRegistration(_meter, source, country);
    }
}
```

## Method Signature Requirements

All metrics methods must follow specific signature requirements:

### Required Structure

```csharp
[Counter<T>|Gauge<T>("name", "description")]
static partial void MethodName(
    IMeter<TService> meter | IMeterScope<TService> scope,  // Required first parameter
    [value parameter],                                      // Optional for counters, required for gauges
    [tag parameters...]                                     // Optional additional parameters become tags
);
```

### First Parameter Rules

The first parameter must be either:

- `IMeter<T>` - For basic metrics without scoped context
- `IMeterScope<T>` - For metrics within a scope (includes scope tags automatically)

### Value Parameter Rules

- **Counters**: Value parameter is optional. If not provided, defaults to incrementing by 1
- **Gauges**: Value parameter is required and must match the generic type `T` in the attribute
- **Type matching**: The value parameter type must exactly match `T` in `[Counter<T>]` or `[Gauge<T>]`

### Tag Parameters

- All parameters after the first (and value parameter if present) become metric tags
- Parameter names become tag names
- Parameter values become tag values
- Keep tag cardinality reasonable for performance

### Examples

```csharp
// Counter without value parameter (increments by 1)
[Counter<int>("http_requests", "HTTP requests received")]
static partial void CountHttpRequest(IMeter<WebService> meter, string method, string endpoint);

// Counter with value parameter
[Counter<long>("bytes_processed", "Bytes processed by operation")]
static partial void CountBytesProcessed(IMeter<DataService> meter, long bytes, string operation);

// Gauge (value parameter required)
[Gauge<double>("cpu_usage", "Current CPU usage percentage")]
static partial void RecordCpuUsage(IMeter<SystemService> meter, double percentage, string core);

// Using scoped meter
[Counter<int>("scoped_operations", "Operations within a scope")]
static partial void CountScopedOperation(IMeterScope<ProcessingService> scope, string operation, int duration);
```

## Generated Code

The source generator creates efficient implementations. Here's what gets generated for a counter:

### Your Declaration

```csharp
[Counter<int>("user_logins", "User login attempts")]
static partial void CountUserLogin(IMeter<AuthService> meter, string result, string userId);
```

### Generated Implementation

```csharp
static Counter<int>? CountUserLoginMetric;

[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Cratis.Metrics.Roslyn", "1.0.0")]
static partial void CountUserLogin(IMeter<AuthService> meter, string result, string userId)
{
    if (CountUserLoginMetric is null && meter.ActualMeter is not null)
    {
        CountUserLoginMetric = meter.ActualMeter.CreateCounter<int>("user_logins", "User login attempts");
    }

    var tags = new TagList(new ReadOnlySpan<KeyValuePair<string, object?>>(new KeyValuePair<string, object?>[]
    {
        new("result", result),
        new("userId", userId)
    }));

    CountUserLoginMetric?.Add(1, tags);
}
```

### Key Features of Generated Code

1. **Lazy initialization** - Metrics instruments are created only when first used
2. **Null safety** - Proper null checks prevent exceptions
3. **Efficient tagging** - Uses `TagList` and `ReadOnlySpan` for performance
4. **Scope integration** - Automatically merges scope tags when using `IMeterScope<T>`
5. **Generated code attributes** - Clearly marks generated code for debugging

## Error Handling

The source generator provides compile-time validation with helpful error messages:

### METRICS001: Missing First Parameter

```csharp
// ❌ Error: Missing required first parameter
[Counter<int>("test", "test")]
static partial void BadMethod();
```

**Fix**: Add `IMeter<T>` or `IMeterScope<T>` as first parameter.

### METRICS002: Invalid First Parameter Type

```csharp
// ❌ Error: Wrong first parameter type
[Counter<int>("test", "test")]
static partial void BadMethod(string invalidParameter);
```

**Fix**: Use `IMeter<T>` or `IMeterScope<T>` as first parameter.

### METRICS003: Missing Value Parameter

```csharp
// ❌ Error: Gauge missing required value parameter
[Gauge<double>("test", "test")]
static partial void BadGauge(IMeter<Service> meter, string tag);
```

**Fix**: Add a parameter of type `double` for the gauge value.

## Advanced Usage

### Working with Scoped Metrics

When using `IMeterScope<T>`, the generator automatically includes scope tags:

```csharp
public partial class OrderMetrics
{
    [Counter<int>("order_steps", "Steps in order processing")]
    static partial void CountOrderStep(IMeterScope<OrderService> scope, string step);
}

// Usage
public async Task ProcessOrderAsync(Order order)
{
    using var scope = _meter.BeginScope(new Dictionary<string, object>
    {
        ["order_id"] = order.Id,
        ["customer_type"] = order.CustomerType
    });

    // This will include both the scope tags (order_id, customer_type) 
    // and the method tag (step)
    OrderMetrics.CountOrderStep(scope, "validation");
    await ValidateOrder(order);

    OrderMetrics.CountOrderStep(scope, "processing");
    await ProcessOrder(order);
}
```

### Complex Tag Scenarios

```csharp
public partial class ApiMetrics
{
    // Multiple tags with different types
    [Counter<long>("api_request_size", "Size of API requests")]
    static partial void CountRequestSize(
        IMeter<ApiService> meter,
        long bytes,
        string endpoint,
        string method,
        int statusCode,
        bool isAuthenticated);
}

// Usage
ApiMetrics.CountRequestSize(_meter, request.ContentLength, "/users", "POST", 201, true);
```

### Organizing Metrics

Group related metrics in focused partial classes:

```csharp
// Authentication metrics
public partial class AuthMetrics
{
    [Counter<int>("login_attempts", "Login attempts by result")]
    static partial void CountLogin(IMeter<AuthService> meter, string result);

    [Counter<int>("token_validations", "Token validation attempts")]
    static partial void CountTokenValidation(IMeter<AuthService> meter, string result);
}

// Database metrics
public partial class DatabaseMetrics
{
    [Counter<int>("queries", "Database queries executed")]
    static partial void CountQuery(IMeter<DatabaseService> meter, string operation, string table);

    [Gauge<double>("connection_pool_usage", "Database connection pool usage")]
    static partial void RecordConnectionPoolUsage(IMeter<DatabaseService> meter, double percentage);
}
```

## Troubleshooting

### Source Generator Not Running

If methods aren't being generated:

1. **Check build output** for any compilation errors
2. **Verify package reference** includes `Cratis.Metrics.Roslyn`
3. **Ensure methods are partial** and in partial classes
4. **Check method signatures** match the required pattern

### Metrics Not Appearing

If metrics aren't showing up in your monitoring:

1. **Verify meter registration** in dependency injection
2. **Check your metrics collection** configuration
3. **Ensure ActivitySource** is properly configured for your application

### Build Errors

For compilation issues:

1. **Review error messages** - the generator provides specific error codes
2. **Check parameter types** match attribute generic types
3. **Verify namespace imports** include `Cratis.Metrics`

### Debugging Generated Code

To see the generated code:

1. **Enable source generators** in your IDE
2. **Check the Dependencies > Analyzers** node in Solution Explorer
3. **Look for generated files** under `Cratis.Metrics.Roslyn`

## Best Practices

1. **Use descriptive names** for both metrics and method names
2. **Keep tag cardinality low** to avoid performance issues  
3. **Group related metrics** in focused partial classes
4. **Use meaningful descriptions** that explain what the metric measures
5. **Consider scoped metrics** for contextual measurements
6. **Follow naming conventions** for consistency across your application

## Performance Considerations

The generated code is optimized for performance:

- **Lazy initialization** prevents unnecessary metric creation
- **Efficient tagging** uses `TagList` and `ReadOnlySpan`
- **Minimal allocations** through careful memory management
- **Null safety** prevents exceptions without performance overhead

The source generator runs at compile time, so there's no runtime performance impact from the generation process itself.
