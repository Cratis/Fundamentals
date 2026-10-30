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

## Metric views and cardinality

Metric shaping is **off by default** (`EnableMetricViews = false`). The shared setup leaves SDK boundaries, cardinality and consumer `AddView` registrations unchanged, including views registered before or after `WithCratis` or through `ConfigureMetrics`. Metric policy fields alone do not enable shaping and are not applied while it is off; settings are still validated at registration.

Set `EnableMetricViews = true` to give Cratis explicit ownership of matching metric shaping through one composed view. `MetricCardinalityLimit` defaults to **2000** distinct tag sets per stream, the SDK's existing default. A per-instrument `MetricPolicies` entry can override the limit, allowed tags and histogram boundaries. Keys are exact, case-insensitive instrument names, without wildcards or meter qualifiers.

When enabled, histograms from `Cratis.*` meters with unit `s` and no instrument advice get second-based boundaries:

```text
0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
```

Other units and third-party instruments retain their buckets. Boundary precedence is policy boundaries, then instrument advice, then Cratis seconds buckets, then SDK defaults. Policy boundaries are ignored for non-histograms. Set `SecondsHistogramBoundaries = null` to opt out of automatic seconds buckets and retain advice or SDK defaults; explicit policy boundaries still apply. You can also supply a different ascending, finite bucket array.

This complete worker `Program.cs` opts into owned shaping, chooses a smaller budget and shapes one instrument:

```csharp
using Cratis.OpenTelemetry;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.AddCratisOpenTelemetry(options =>
{
    options.EnableMetricViews = true;
    options.MetricCardinalityLimit = 500;
    options.MetricPolicies["cratis.my_app.request.duration"] = new()
    {
        TagKeys = ["outcome"],
        Boundaries = [0, 0.01, 0.05, 0.1, 0.5, 1, 5],
        CardinalityLimit = 100
    };
});
await builder.Build().RunAsync();
```

A policy limit takes precedence over `MetricCardinalityLimit`. Whether shaping is enabled or disabled, limits must be positive; boundaries must be finite and strictly ascending; tag keys must not be null or whitespace. Invalid settings throw `InvalidMetricPolicy` at registration. Null policy properties keep the defaults; `TagKeys = []` removes all tags, while null keeps all tags.

Registration deep-copies metric settings before validating them: the ownership flag, global limit, seconds boundaries, policy dictionary, policy objects and their tag/boundary arrays are fixed for that registration. Later mutations to retained options, entries or arrays do not reconfigure existing or late-discovered instruments, even before provider resolution. `ConfigureMetrics` is captured once at registration as well. Configure everything inside the registration callback, not by mutating retained references.

Choose bounded dimensions such as `outcome`, HTTP method or a route template. Do not use user, tenant, request or entity identifiers as metric tags, even in scopes. Use traces or logs for per-request context instead.

When a stream exceeds its budget, the SDK aggregates excess tag sets into a point with `otel.metric.overflow=true`. Alert when that point has measurements (for a cumulative counter, alert on its increase/rate; for a histogram, its observation count), then inspect the dimensions or deliberately adjust the budget. The overflow point and the zero-tag point do not count against the limit.

When opting in, move matching consumer views into `MetricPolicies`, or leave shaping off and keep consumer ownership. The SDK provides no public matching-view inspection: Cratis does not detect, merge, suppress or deduplicate arbitrary consumer views. Every matching SDK view creates a stream regardless of registration order, so another matching `AddView` before, after or inside `ConfigureMetrics` can produce duplicate streams. Cratis composes only its own boundaries, tag filtering and cardinality into one owned view; it returns no configuration for instruments without a policy, automatic seconds buckets or a changed limit.

### Migrating dashboards

- Upgrading alone does not change shaping. Opting into `EnableMetricViews` changes the `le` bucket labels for Cratis seconds histograms without advice. Quantile panels can shift; do not compare old buckets with new buckets across that change. Leave shaping off, or use the seconds-bucket opt-out above, to defer migration.
- `TagKeys` merges series that differ only in dropped tags. Update dashboard filters and grouping accordingly.
- Lowering a cardinality limit can add an overflow series. Include an overflow alert rather than silently excluding it.
- Before opting in, move product-specific views targeting shaped instruments into `MetricPolicies` to avoid a second stream; opt-in does not merge external views.

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
