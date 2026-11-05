---
title: Cratis Fundamentals metrics source generator
description: The Cratis Fundamentals metrics source generator implements attributed partial methods so .NET services can record measurements with typed calls.
---
<a id="metrics-source-generator"></a>

The Cratis Fundamentals metrics source generator turns attributed partial methods into .NET metric-recording code at compile time. Use `Cratis.Observability.SourceGenerators` when you want services to call named methods such as `CountUserRegistration` without managing the underlying instruments at each call site.

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

Your project needs `Cratis.Fundamentals` for the meter types and attributes. Add `Cratis.Observability.SourceGenerators` to generate the method implementations. In the package-reference excerpt below, replace `[version]` with the package version you are using.

If your project references `Cratis.Metrics.Roslyn`, replace that reference with `Cratis.Observability.SourceGenerators`. The old package forwards to the new one through the current major. In the next major, the old package will no longer be published, and the generator's assembly and namespace will be renamed. The generator's assembly and namespace are still named `Cratis.Metrics.Roslyn`, so that name also appears when inspecting generated files.

```xml
<PackageReference Include="Cratis.Observability.SourceGenerators" Version="[version]" />
```

Or via the .NET CLI:

```bash
dotnet add package Cratis.Observability.SourceGenerators
```

## How It Works

At compile time, the generator finds static partial classes containing static partial methods marked with `[Counter<T>]`, `[Gauge<T>]`, or `[Histogram<T>]`. It validates the meter and measurement parameters, then generates the method bodies.

At runtime, each generated method collects tags, looks up the instrument for the supplied meter, and records the measurement. It creates the instrument on first use and caches it per `Meter` instance. You still need to register the meter and configure your telemetry provider to collect from it.

## Getting Started

Before declaring methods, [register a typed meter](index.md#getting-started) for your service. The following excerpts assume your application provides `UserService` and its registration logic.

### 1. Create a Static Partial Class

Put your metric methods in a static partial class. The generator skips classes that are not both `static` and `partial`:

```csharp
using Cratis.Metrics;

namespace MyApplication.Metrics;

public static partial class UserMetrics
{
    // Metrics methods will be defined here
}
```

### 2. Define Partial Methods with Attributes

Choose the instrument for each measurement. Here, a counter counts registrations and a gauge records the current number of sessions. `internal` makes the registration method callable from a service in the same assembly:

```csharp
public static partial class UserMetrics
{
    [Counter<int>("user_registrations", "Number of user registrations")]
    internal static partial void CountUserRegistration(IMeter<UserService> meter, string source, string country);

    [Gauge<double>("active_sessions", "Current number of active sessions")]
    static partial void RecordActiveSessions(IMeter<UserService> meter, double value, string region);
}
```

### 3. Use in Your Services

Inject `IMeter<UserService>` and call the registration counter after your registration logic succeeds. The excerpt leaves that application logic as a placeholder:

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

Use a static partial method in a static partial class, with a meter or scope first. The following is a signature schematic showing alternatives, not a copyable declaration:

### Required Structure

```csharp
[Counter<T>|Gauge<T>|Histogram<T>("name", "description", "unit")]
static partial void MethodName(
    IMeter<TService> meter | IMeterScope<TService> scope,  // Required first parameter
    [value parameter],                                      // Optional for counters, required for gauges and histograms
    [tag parameters...]                                     // Optional additional parameters become tags
);
```

### First Parameter Rules

Use `IMeter<T>` for a measurement with method tags only. Use `IMeterScope<T>` when the measurement also needs tags shared by several calls. The generated method reads tags from the scope you pass; there is no ambient scope applied to other meter calls.

`UnkeyedMeter<T>` names its meter with `typeof(T).FullName`, falling back to `typeof(T).Name` when necessary. Subscribe to that name in your telemetry provider, or configure a [named meter](named-registration.md) and subscribe to its name.

### Value Parameter Rules

After the meter or scope, the generator uses the first parameter whose type matches the attribute's generic type as the measurement. For `[Counter<long>]`, for example, a `long bytes` parameter supplies the increment. A counter without a matching parameter adds one.

Gauges and histograms require a matching measurement parameter. Use the same numeric type spelling in the attribute and parameter, as the examples below do. A parameter's name does not decide whether it is a measurement or a tag.

### Tag Parameters

After removing the meter and measurement parameters, the generator turns the remaining parameters into tags. Each parameter name becomes a tag key and its argument becomes the value. Override a key with `[Tag("…")]` from `Cratis.Diagnostics` when you need a stable name independent of the parameter name.

Choose bounded values such as an outcome or HTTP method. Every distinct tag-value combination can create a separate time series; this count is called cardinality.

### Examples

These declarations are excerpts for a static partial class with `Cratis.Metrics` imported and the illustrated service types available.

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

In `CountScopedOperation`, `int duration` is the counter increment because its type matches `[Counter<int>]`; only `operation` becomes a method tag. Use a histogram to measure a duration distribution.

## Generated Code

The following counter example shows how the generator collects method tags and records through the underlying meter:

### Your Declaration

```csharp
[Counter<int>("user_logins", "User login attempts")]
static partial void CountUserLogin(IMeter<AuthService> meter, string result, string method);
```

### Generated Implementation

```csharp
static readonly global::System.Runtime.CompilerServices.ConditionalWeakTable<Meter, Counter<int>> CountUserLoginMetric = new();

[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Cratis.Metrics.Roslyn", "1.0.0")]
static partial void CountUserLogin(IMeter<AuthService> meter, string result, string method)
{
    var tags = new TagList(new ReadOnlySpan<KeyValuePair<string, object?>>(new KeyValuePair<string, object?>[]
    {
        new("result", result),
        new("method", method)
    }));

    if (meter.ActualMeter is { } actualMeter)
    {
        CountUserLoginMetric.GetValue(actualMeter, static m => m.CreateCounter<int>(name: "user_logins", unit: null, description: "User login attempts")).Add(1, tags);
    }
}
```

Each generated method caches its instrument by `Meter` instance. Passing different keyed or versioned meters therefore records to their respective instruments. The cache is a `ConditionalWeakTable`, as shown above.

Counters, gauges, and histograms pass the name, unit, and description as named arguments to the .NET instrument creation APIs. Two-argument attributes leave the unit `null`. Supply a third argument for a UCUM unit, such as `"s"` for seconds or `"By"` for bytes. UCUM is the Unified Code for Units of Measure. Generated string literals preserve quotes, backslashes, and line breaks in names, units, descriptions, and tag keys.

### Explicit tags and histograms

Import `Cratis.Diagnostics` to use `[Tag("…")]` on a metric parameter. An explicit key lets you rename a parameter without changing the tag used by dashboards and queries.

This declaration excerpt requires a static partial class, `Cratis.Metrics`, the `RequestHandler` service type, and `Cratis.Observability.SourceGenerators`:

```csharp
[Histogram<double>("cratis.my_app.request.duration", "Request duration", "s")]
internal static partial void RequestDuration(
    this IMeter<RequestHandler> meter,
    double seconds,
    [Tag("cratis.my_app.request.outcome")] string outcome);
```

Here, `seconds` supplies the measurement and `outcome` supplies the explicitly named tag. Use seconds for durations. You can also accept `IMeterScope<T>` to add the scope's tags alongside the method tags.

Keep scope and method tags bounded. See [metric views and cardinality](../opentelemetry.md#metric-views-and-cardinality) to set budgets, filter tags, and handle overflow.

### Exported Metric Names

Earlier generator versions passed the description as the instrument's unit, leaving its description empty. Exporters that append units to metric names consequently included the description in the exported name. After regenerating with the corrected generator, these names change for every consumer using such an exporter: for example, `chronicle_observer_partitions_failed_Number_of_failed_partitions_per_observer_in_a_given_event_store_and_namespace_total` becomes `chronicle_observer_partitions_failed_total`, and the description becomes the Prometheus HELP text.

Names and descriptions containing HTML-special characters (`&`, `<`, `>`, or quotes) were previously exported HTML-encoded and are now exported exactly as written.

Update dashboards, alert rules, and queries that refer to the old names when upgrading. The attribute declarations do not need to change, but consumers must rebuild to regenerate their instrument creation code.

### Key Features of Generated Code

The generated method assembles a `TagList` for each call and appends scope tags when its first parameter is `IMeterScope<T>`. It then checks the underlying `ActualMeter` or scope `Meter` before recording. That check does not make a null meter or scope argument safe: the method accesses the argument's properties first.

Generated methods carry `GeneratedCodeAttribute("Cratis.Metrics.Roslyn", "1.0.0")`, which identifies the generator when you inspect their implementation.

## Error Handling

The generator reports diagnostics for missing or invalid meter parameters and missing measurements. Match the diagnostic to the declaration before changing the calling code:

### METRICS001: Missing First Parameter

```csharp
// ❌ Error: Missing required first parameter
[Counter<int>("test", "test")]
static partial void BadMethod();
```

Add `IMeter<T>` or `IMeterScope<T>` as the first parameter.

### METRICS002: Invalid First Parameter Type

```csharp
// ❌ Error: Wrong first parameter type
[Counter<int>("test", "test")]
static partial void BadMethod(string invalidParameter);
```

Replace the first parameter with `IMeter<T>` or `IMeterScope<T>`.

<a id="metrics003-missing-value-parameter"></a>

### METRICS0013: Missing Value Parameter

```csharp
// ❌ Error: Gauge missing required value parameter
[Gauge<double>("test", "test")]
static partial void BadGauge(IMeter<Service> meter, string tag);
```

Add a `double` parameter for the gauge value so it matches `[Gauge<double>]`.

## Advanced Usage

### Working with Scoped Metrics

Pass a scope when several measurements need the same context. In this example, each order step gets the shared `operation` and `customer_type` tags plus its own `step` tag. The usage excerpts assume an injected `_meter` and your application's order operations:

```csharp
public static partial class OrderMetrics
{
    [Counter<int>("order_steps", "Steps in order processing")]
    internal static partial void CountOrderStep(IMeterScope<OrderService> scope, string step);
}

// Usage
public async Task ProcessOrderAsync(Order order)
{
    // Using anonymous type for cleaner syntax
    using var scope = _meter.BeginScope(new
    {
        operation = "process",
        customer_type = order.CustomerType
    });

    // This will include both the scope tags (operation, customer_type)
    // and the method tag (step)
    OrderMetrics.CountOrderStep(scope, "validation");
    await ValidateOrder(order);

    OrderMetrics.CountOrderStep(scope, "processing");
    await ProcessOrder(order);
}
```

You can also use property name inference with anonymous types:

```csharp
public async Task ProcessOrderAsync(Order order)
{
    // Property names are inferred from the source
    using var scope = _meter.BeginScope(new
    {
        order.CustomerType,
        Operation = "process"
    });

    OrderMetrics.CountOrderStep(scope, "processing");
}
```

### Complex Tag Scenarios

Tag parameters can have different types. Here, `bytes` supplies the counter increment; the remaining parameters become tags. Use a route template for `endpoint` so individual resource IDs do not create new time series:

```csharp
public static partial class ApiMetrics
{
    // Multiple tags with different types
    [Counter<long>("api_request_size", "Size of API requests")]
    internal static partial void CountRequestSize(
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

Keep related declarations together so a service's instrumentation can be found in one place. These are declaration excerpts; add an access modifier such as `internal` to methods that another class needs to call:

```csharp
// Authentication metrics
public static partial class AuthMetrics
{
    [Counter<int>("login_attempts", "Login attempts by result")]
    static partial void CountLogin(IMeter<AuthService> meter, string result);

    [Counter<int>("token_validations", "Token validation attempts")]
    static partial void CountTokenValidation(IMeter<AuthService> meter, string result);
}

// Database metrics
public static partial class DatabaseMetrics
{
    [Counter<int>("queries", "Database queries executed")]
    static partial void CountQuery(IMeter<DatabaseService> meter, string operation, string table);

    [Gauge<double>("connection_pool_usage", "Database connection pool usage")]
    static partial void RecordConnectionPoolUsage(IMeter<DatabaseService> meter, double percentage);
}
```

## Troubleshooting

### Source Generator Not Running

Start with the compiler output and resolve any errors in the declaration. Confirm the project references `Cratis.Observability.SourceGenerators`, then check that both the class and method are `static partial`. A class missing either modifier is skipped. Compare the first and measurement parameters with the [signature requirements](#method-signature-requirements).

### Metrics Not Appearing

First confirm that your service resolves its meter and reaches the metric method call. Then check that the telemetry provider subscribes to the actual meter name and has an exporter configured. Dependency-injection registration alone does not connect the meter to a collector. Use [named meter registration](named-registration.md) to check name and key selection, and [OpenTelemetry setup](../opentelemetry.md) to configure collection.

### Build Errors

Use the [diagnostics above](#error-handling) to check the declaration's first parameter and measurement type. Confirm `Cratis.Metrics` is imported. If a service cannot call a generated method, check its access modifier: a partial method without one is private to its containing class.

### Debugging Generated Code

In an IDE that exposes generated sources, inspect the project's analyzer-generated files. Look for `Cratis.Metrics.Roslyn`, the generator's assembly name, even though the package reference is `Cratis.Observability.SourceGenerators`. Compare the generated method's tags and instrument name with the declaration.

## Best Practices

Choose instrument names and descriptions that tell a reader what is measured. Keep tag keys stable with explicit `[Tag("…")]` attributes where dashboards depend on them, and choose bounded tag values. Group declarations by the service or operation they describe. Use scopes when several calls need the same context.

## Performance Considerations

Generation happens at compile time, but recording measurements still does work at runtime. The generated method builds its tags and looks up the instrument on each call. Instruments are cached per meter after first use; tag collection still constructs an array of key-value pairs and a `TagList`, then appends any scope tags.

When planning collection, account for how often the method runs and how many distinct tag combinations it emits. The [metrics overview](index.md) explains instrument choice and scope usage; [metric views and cardinality](../opentelemetry.md#metric-views-and-cardinality) covers collection budgets and overflow.
