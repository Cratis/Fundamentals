---
title: Set up OpenTelemetry
description: Capture Cratis traces, metrics, and scoped logs with one setup call and standard OTEL configuration.
---

Use `Cratis.OpenTelemetry` in your host to capture all `Cratis.*` activity sources and meters, including Arc and the Chronicle client. You do not need to maintain a list of product scopes. The package is separate from `Cratis.Fundamentals`; instrumentation libraries do not need to depend on the hosting SDK or exporter.

## Add telemetry to your host

Add the `Cratis.OpenTelemetry` NuGet package. For a .NET worker using `Microsoft.Extensions.Hosting`, this is a complete `Program.cs`:

```csharp
using Cratis.OpenTelemetry;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.AddCratisOpenTelemetry(options => options.ServiceName = "my-app");
await builder.Build().RunAsync();
```

The same call works on an ASP.NET Core `WebApplicationBuilder`. Setup subscribes to `Cratis.*` traces and metrics, adds ASP.NET Core and HttpClient instrumentation, adds runtime metrics, and includes scopes and formatted messages in OpenTelemetry logs.

Without an endpoint, no OTLP exporter is registered. Instrumentation remains available to in-process listeners. Set an endpoint to send traces, metrics, and logs to your collector or local Aspire dashboard:

```bash
export OTEL_SERVICE_NAME=my-app
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
export OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

For HTTP/protobuf use `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf` and the collector's HTTP port, usually 4318. The SDK adds the signal paths to a common endpoint; signal-specific endpoints contain their full paths.

## Resource identity

`OTEL_SERVICE_NAME` in the environment wins over configuration, `OTEL_RESOURCE_ATTRIBUTES`, and the `ServiceName` fallback. Without a configured name, the fallback is the entry assembly name in lowercase. `service.version` defaults to the entry assembly's informational version; you can provide `ServiceVersion` when your host uses a different version source.

Set resource attributes through standard configuration:

```bash
export OTEL_RESOURCE_ATTRIBUTES=service.namespace=customer,deployment.environment.name=production,service.instance.id=pod-name
```

Customer applications choose their own namespace. Cratis-operated services use `service.namespace=cratis`. Tenant identity does not belong in the resource.

## Compose with an existing setup

This registration excerpt requires a `ServiceCollection` named `services` and the `Cratis.OpenTelemetry` and `Microsoft.Extensions.DependencyInjection` namespaces:

```csharp
services.AddOpenTelemetry().WithCratis();
```

`WithCratis()` uses a registered `IConfiguration` instance and standard environment variables. If your configuration is registered through a factory, pass it explicitly with `WithCratis(configuration)`. The host extension always passes the host's configuration.

If you already own the resource, logging, instrumentation, and export pipeline, subscribe only to Cratis with `tracing.AddCratisInstrumentation()` and `metrics.AddCratisInstrumentation()`. These extensions do not add exporters or host instrumentation.

Use `ConfigureTracing` and `ConfigureMetrics` for optional instrumentation. This host registration excerpt adds the kernel's Orleans sources:

```csharp
builder.AddCratisOpenTelemetry(options =>
    options.ConfigureTracing = tracing => tracing.AddSource("Microsoft.Orleans.*"));
```

For gRPC client or MongoDB instrumentation, reference the corresponding instrumentation package and call its provider-builder extension through these callbacks. They are not installed or enabled by default. Call the shared setup once; do not add another `UseOtlpExporter()` alongside it.

## Standard configuration

Environment variables take precedence for resource identity and export selection. The same `OTEL_*` keys work in host `IConfiguration`, including appsettings.

| Key | Behavior |
| --- | --- |
| `OTEL_SERVICE_NAME` | Service identity; environment value wins |
| `OTEL_RESOURCE_ATTRIBUTES` | Resource namespace, environment, instance, and other attributes |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Enables export for all signals unless a signal is disabled |
| `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_ENDPOINT` | Enables export for the configured signal only, or overrides the common endpoint |
| `OTEL_EXPORTER_OTLP_PROTOCOL`, `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_PROTOCOL` | `grpc` or `http/protobuf` |
| `OTEL_EXPORTER_OTLP_HEADERS`, `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_HEADERS` | OTLP request headers |
| `OTEL_{TRACES,METRICS,LOGS}_EXPORTER` | `none` disables export for that signal; `otlp` selects OTLP |
| `OTEL_SDK_DISABLED` | `true` skips the shared SDK setup |
| `OTEL_TRACES_SAMPLER`, `OTEL_TRACES_SAMPLER_ARG` | SDK sampling; defaults to `parentbased_always_on` |
| `OTEL_METRIC_EXPORT_INTERVAL` | SDK metric export interval in milliseconds |

The package does not automatically install exporters other than OTLP. Invalid enabled endpoints fail setup rather than exporting to an SDK fallback address.

## Missing spans or metrics

Unkeyed `IActivitySource<T>` and `IMeter<T>` use the type's full name. They are not subscribed by `Cratis.*` unless that name matches the wildcard. Use [named activity sources](traces/named-registration.md) and a `Cratis.<Product>` scope, or add your application source and meter explicitly to your providers.

For product names and attribute conventions, follow the [shared Cratis OpenTelemetry convention](https://github.com/Cratis/Architecture/blob/main/decisions/0001-opentelemetry-convention.md).
