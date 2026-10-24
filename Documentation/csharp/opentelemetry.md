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

For HTTP/protobuf use `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf` and the collector's HTTP port, usually 4318. The SDK appends the signal path (`/v1/traces`, `/v1/metrics`, or `/v1/logs`) to the common base endpoint and keeps any query string. Signal-specific endpoints contain their full paths.

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

`WithCratis()` resolves `IConfiguration` from the service provider when telemetry providers are built, including configuration registered through a factory by worker and web hosts. You can supply separate telemetry settings with `WithCratis(configuration)`. Neither overload replaces the application's `IConfiguration`; environment precedence applies only to telemetry settings. The host extension always passes the host's configuration.

If you already own the resource, logging, instrumentation, and export pipeline, subscribe only to Cratis with `tracing.AddCratisInstrumentation()` and `metrics.AddCratisInstrumentation()`. These extensions do not add exporters or host instrumentation.

Use `ConfigureTracing` and `ConfigureMetrics` for optional instrumentation. This host registration excerpt adds the kernel's Orleans sources:

```csharp
builder.AddCratisOpenTelemetry(options =>
    options.ConfigureTracing = tracing => tracing.AddSource("Microsoft.Orleans.*"));
```

For gRPC client or MongoDB instrumentation, reference the corresponding instrumentation package and call its provider-builder extension through these callbacks. They are not installed or enabled by default. Call the shared setup once; do not add another `UseOtlpExporter()` alongside it.

ASP.NET Core tracing skips `/health`, `/healthz`, `/ready`, `/readyz`, and `/readiness`, including child paths, by default. A product can replace this filter through `ConfigureTracing`:

```csharp
builder.AddCratisOpenTelemetry(options =>
    options.ConfigureTracing = tracing => tracing.AddAspNetCoreInstrumentation(instrumentation =>
        instrumentation.Filter = context => !context.Request.Path.StartsWithSegments("/my-probes")));
```

This excerpt also requires the `OpenTelemetry.Trace` namespace. Standard `OTEL_TRACES_SAMPLER*` settings still take precedence over samplers selected in application callbacks.

## Exception logs

By default, exception logs export `exception.type`, but not the exception object, message, or stack trace. Formatted exception messages are omitted. Log templates, structured attributes, and scopes can still contain sensitive data; do not put secrets or personal identifiers in them.

To deliberately include exception messages and stack traces, set `IncludeExceptionDetails = true` in the shared setup options. Review the privacy implications before enabling it.

## Standard configuration

Nonempty environment variables take precedence for resource identity and export settings. Empty or whitespace-only values are treated as unset. The same `OTEL_*` keys work in host `IConfiguration`, including appsettings.

| Key | Behavior |
| --- | --- |
| `OTEL_SERVICE_NAME` | Service identity; environment value wins |
| `OTEL_RESOURCE_ATTRIBUTES` | Resource namespace, environment, instance, and other attributes |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Enables export for all signals unless a signal is disabled |
| `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_ENDPOINT` | Enables export for the configured signal only, or overrides the common endpoint |
| `OTEL_EXPORTER_OTLP_PROTOCOL`, `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_PROTOCOL` | `grpc` or `http/protobuf` |
| `OTEL_EXPORTER_OTLP_HEADERS`, `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_HEADERS` | OTLP request headers |
| `OTEL_EXPORTER_OTLP_TIMEOUT`, `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_TIMEOUT` | Export request timeout in milliseconds |
| `OTEL_EXPORTER_OTLP_COMPRESSION`, `OTEL_EXPORTER_OTLP_{TRACES,METRICS,LOGS}_COMPRESSION` | `none` or `gzip`; signal-specific values override common settings |
| `OTEL_BLRP_MAX_QUEUE_SIZE`, `OTEL_BLRP_MAX_EXPORT_BATCH_SIZE` | Log export queue and batch limits |
| `OTEL_BLRP_SCHEDULE_DELAY`, `OTEL_BLRP_EXPORT_TIMEOUT` | Log batch schedule delay and export timeout in milliseconds |
| `OTEL_{TRACES,METRICS,LOGS}_EXPORTER` | `none` disables export for that signal; `otlp` selects OTLP |
| `OTEL_SDK_DISABLED` | `true` turns off every OpenTelemetry provider in the service collection, including providers added after `WithCratis()`; they become no-op providers |
| `OTEL_TRACES_SAMPLER`, `OTEL_TRACES_SAMPLER_ARG` | Sampling; defaults to `parentbased_always_on`; standard settings win over application callbacks |
| `OTEL_PROPAGATORS` | Comma-separated `tracecontext` and `baggage` (the default), or `none`; other names are not installed |
| `OTEL_METRIC_EXPORT_INTERVAL` | SDK metric export interval in milliseconds |

The package does not automatically install exporters other than OTLP. Invalid enabled endpoints fail setup rather than exporting to an SDK fallback address.

`traceidratio` and `parentbased_traceidratio` require `OTEL_TRACES_SAMPLER_ARG` to be a number in the inclusive range `[0, 1]`, using a decimal point. Missing, non-numeric, non-finite, and out-of-range values fail startup with `InvalidOpenTelemetrySamplerArgument`. Its message names `OTEL_TRACES_SAMPLER_ARG` and the invalid value (`<missing>` when absent). There is no fallback ratio. For example, set `OTEL_TRACES_SAMPLER=parentbased_traceidratio` and `OTEL_TRACES_SAMPLER_ARG=0.1` to sample 10% of root traces while preserving the parent's sampling decision.

## Missing spans or metrics

Unkeyed `IActivitySource<T>` and `IMeter<T>` use the type's full name. They are not subscribed by `Cratis.*` unless that name matches the wildcard. Use [named activity sources](traces/named-registration.md) and a `Cratis.<Product>` scope, or add your application source and meter explicitly to your providers.

For product names and attribute conventions, follow the [shared Cratis OpenTelemetry convention](https://github.com/Cratis/Architecture/issues/33).
