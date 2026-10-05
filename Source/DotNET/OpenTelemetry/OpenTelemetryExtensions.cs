// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry;

/// <summary>
/// Shared host setup for Cratis telemetry with opt-in OTLP export.
/// </summary>
public static class OpenTelemetryExtensions
{
    static readonly string[] _signals = ["TRACES", "METRICS", "LOGS"];

    /// <summary>
    /// Adds Cratis sources, meters, host instrumentation, scoped logs and resource defaults.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Optional application defaults and extra instrumentation.</param>
    /// <returns>The application builder for continuation.</returns>
    public static IHostApplicationBuilder AddCratisOpenTelemetry(this IHostApplicationBuilder builder, Action<CratisOpenTelemetryOptions>? configure = null)
    {
        builder.Services.AddOpenTelemetry().WithCratis(builder.Configuration, configure);

        return builder;
    }

    /// <summary>
    /// Adds Cratis telemetry, resolving application configuration from the service provider or using standard environment variables.
    /// </summary>
    /// <param name="builder">The OpenTelemetry builder.</param>
    /// <param name="configure">Optional application defaults and extra instrumentation.</param>
    /// <returns>The OpenTelemetry builder for continuation.</returns>
    public static OpenTelemetryBuilder WithCratis(this OpenTelemetryBuilder builder, Action<CratisOpenTelemetryOptions>? configure = null)
        => WithCratis(builder, provider => provider.GetRequiredService<IConfiguration>(), configure);

    /// <summary>
    /// Adds Cratis telemetry with standard OTEL configuration keys supplied by the application.
    /// </summary>
    /// <param name="builder">The OpenTelemetry builder.</param>
    /// <param name="configuration">Standard OTEL keys, such as OTEL_SERVICE_NAME and OTEL_EXPORTER_OTLP_ENDPOINT.</param>
    /// <param name="configure">Optional application defaults and extra instrumentation.</param>
    /// <returns>The OpenTelemetry builder for continuation.</returns>
    /// <exception cref="InvalidOpenTelemetryEndpoint">An enabled OTLP endpoint is invalid.</exception>
    public static OpenTelemetryBuilder WithCratis(this OpenTelemetryBuilder builder, IConfiguration configuration, Action<CratisOpenTelemetryOptions>? configure = null)
    {
        if (IsDisabled(configuration))
        {
            // Register every provider before wrapping its factory so later SDK calls
            // retain the effective disablement setting rather than host configuration.
            builder.WithTracing(_ => { }).WithMetrics(_ => { }).WithLogging(_ => { });
            ConfigureProviderFactory<TracerProvider>(builder.Services, _ => configuration);
            ConfigureProviderFactory<MeterProvider>(builder.Services, _ => configuration);
            ConfigureProviderFactory<LoggerProvider>(builder.Services, _ => configuration);
            return builder;
        }

        ValidateEndpoints(configuration);

        return WithCratis(builder, _ => configuration, configure);
    }

    static OpenTelemetryBuilder WithCratis(OpenTelemetryBuilder builder, Func<IServiceProvider, IConfiguration> resolveConfiguration, Action<CratisOpenTelemetryOptions>? configure)
    {
        var options = new CratisOpenTelemetryOptions();
        configure?.Invoke(options);
        ConfigureResourceDefaults(builder.Services, options);
        builder.WithLogging(_ => { }, logging =>
        {
            logging.IncludeScopes = true;
            logging.IncludeFormattedMessage = true;
        }).WithTracing(_ => { }).WithMetrics(_ => { });
        ConfigureProviderFactory<TracerProvider>(builder.Services, resolveConfiguration);
        ConfigureProviderFactory<MeterProvider>(builder.Services, resolveConfiguration);
        ConfigureProviderFactory<LoggerProvider>(builder.Services, resolveConfiguration);
        builder.Services.AddOptions<OtlpExporterOptions>();
        builder.Services.AddOptions<PeriodicExportingMetricReaderOptions>().Configure<IServiceProvider>((reader, provider) => ConfigureReader(reader, resolveConfiguration(provider)));
        builder.Services.ConfigureOpenTelemetryTracerProvider((provider, tracing) =>
        {
            var configuration = resolveConfiguration(provider);
            if (IsDisabled(configuration))
            {
                tracing.SetSampler(new AlwaysOffSampler());
                return;
            }
            ValidateEndpoints(configuration);
            tracing.ConfigureResource(resource => ConfigureResource(resource, configuration));
            ConfigureSampler(tracing, configuration);
            tracing.AddCratisInstrumentation();
            if (ShouldExport(configuration, "TRACES"))
            {
                var exporter = ExporterOptions(provider, configuration, "TRACES", "traces");
                var batch = exporter.BatchExportProcessorOptions;
                tracing.AddProcessor(exporter.ExportProcessorType is ExportProcessorType.Simple
                    ? new SimpleActivityExportProcessor(new OtlpTraceExporter(exporter))
                    : new BatchActivityExportProcessor(new OtlpTraceExporter(exporter), batch.MaxQueueSize, batch.ScheduledDelayMilliseconds, batch.ExporterTimeoutMilliseconds, batch.MaxExportBatchSize));
            }
        });
        builder.Services.ConfigureOpenTelemetryMeterProvider((provider, metrics) =>
        {
            var configuration = resolveConfiguration(provider);
            if (IsDisabled(configuration))
            {
                return;
            }
            ValidateEndpoints(configuration);
            metrics.ConfigureResource(resource => ConfigureResource(resource, configuration));
            metrics.AddCratisInstrumentation();
            if (ShouldExport(configuration, "METRICS"))
            {
                var reader = provider.GetRequiredService<IOptions<PeriodicExportingMetricReaderOptions>>().Value;
                metrics.AddReader(new PeriodicExportingMetricReader(new OtlpMetricExporter(ExporterOptions(provider, configuration, "METRICS", "metrics")), reader.ExportIntervalMilliseconds ?? 60_000, reader.ExportTimeoutMilliseconds ?? 30_000));
            }
        });
        builder.Services.ConfigureOpenTelemetryLoggerProvider((provider, logging) =>
        {
            var configuration = resolveConfiguration(provider);
            if (IsDisabled(configuration))
            {
                return;
            }
            ValidateEndpoints(configuration);
            logging.ConfigureResource(resource => ConfigureResource(resource, configuration));
            if (ShouldExport(configuration, "LOGS"))
            {
                logging.AddProcessor(new BatchLogRecordExportProcessor(new OtlpLogExporter(ExporterOptions(provider, configuration, "LOGS", "logs"))));
            }
        });

        // Instrumentation extensions register services, so they must run before DI
        // is built. Keep application callbacks after the shared provider defaults.
        builder.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
            options.ConfigureTracing?.Invoke(tracing);
        }).WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation();
            options.ConfigureMetrics?.Invoke(metrics);
        });

        return builder;
    }

    static void ConfigureProviderFactory<TProvider>(IServiceCollection services, Func<IServiceProvider, IConfiguration> resolveConfiguration)
    {
        var descriptor = services.LastOrDefault(service => service.ServiceType == typeof(TProvider));
        if (descriptor?.ImplementationFactory is not { } factory)
        {
            return;
        }

        // The SDK checks disablement before invoking provider callbacks. Give only
        // that factory the effective setting; leave application DI configuration intact.
        services[services.IndexOf(descriptor)] = new ServiceDescriptor(
            typeof(TProvider),
            provider =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddConfiguration(provider.GetRequiredService<IConfiguration>())
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["OTEL_SDK_DISABLED"] = IsDisabled(resolveConfiguration(provider)) ? "true" : "false"
                    }).Build();
                return factory(new TelemetryServiceProvider(provider, configuration));
            },
            descriptor.Lifetime);
    }

    static OtlpExporterOptions ExporterOptions(IServiceProvider provider, IConfiguration configuration, string signal, string path)
    {
        var exporter = provider.GetRequiredService<IOptionsFactory<OtlpExporterOptions>>().Create(Options.DefaultName);
        ConfigureExporter(exporter, configuration, signal, path);

        return exporter;
    }

    static bool IsDisabled(IConfiguration configuration) => string.Equals(Read(configuration, "OTEL_SDK_DISABLED"), "true", StringComparison.OrdinalIgnoreCase);

    static void ConfigureReader(PeriodicExportingMetricReaderOptions reader, IConfiguration configuration)
    {
        if (int.TryParse(Read(configuration, "OTEL_METRIC_EXPORT_INTERVAL"), CultureInfo.InvariantCulture, out var interval) && interval > 0)
        {
            reader.ExportIntervalMilliseconds = interval;
        }
        if (int.TryParse(Read(configuration, "OTEL_METRIC_EXPORT_TIMEOUT"), CultureInfo.InvariantCulture, out var timeout) && timeout >= 0)
        {
            reader.ExportTimeoutMilliseconds = timeout;
        }
    }

    static void ConfigureSampler(TracerProviderBuilder tracing, IConfiguration configuration)
    {
        var ratio = double.TryParse(Read(configuration, "OTEL_TRACES_SAMPLER_ARG"), CultureInfo.InvariantCulture, out var value) && value is >= 0 and <= 1 ? value : 1;
        Sampler? sampler = Read(configuration, "OTEL_TRACES_SAMPLER")?.Trim().ToLowerInvariant() switch
        {
            "always_on" => new AlwaysOnSampler(),
            "always_off" => new AlwaysOffSampler(),
            "traceidratio" => new TraceIdRatioBasedSampler(ratio),
            "parentbased_always_on" => new ParentBasedSampler(new AlwaysOnSampler()),
            "parentbased_always_off" => new ParentBasedSampler(new AlwaysOffSampler()),
            "parentbased_traceidratio" => new ParentBasedSampler(new TraceIdRatioBasedSampler(ratio)),
            _ => null
        };
        if (sampler is not null)
        {
            tracing.SetSampler(sampler);
        }
    }

    static string? Read(IConfiguration configuration, string key)
    {
        var environment = Environment.GetEnvironmentVariable(key);
        var value = string.IsNullOrWhiteSpace(environment) ? configuration[key] : environment;

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    static void ConfigureResource(ResourceBuilder resource, IConfiguration configuration)
    {
        var attributes = Read(configuration, "OTEL_RESOURCE_ATTRIBUTES");
        if (!string.IsNullOrWhiteSpace(attributes))
        {
            resource.AddAttributes(attributes.Split(',')
                .Select(attribute => attribute.Split('=', 2))
                .Where(parts => parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                .Select(parts => new KeyValuePair<string, object>(parts[0].Trim(), Uri.UnescapeDataString(parts[1].Trim()))));
        }
        var serviceName = Read(configuration, "OTEL_SERVICE_NAME");
        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            resource.AddAttributes([new("service.name", serviceName)]);
        }
    }

    static bool ShouldExport(IConfiguration configuration, string signal) =>
        !string.IsNullOrWhiteSpace(Read(configuration, $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT") ?? Read(configuration, "OTEL_EXPORTER_OTLP_ENDPOINT")) &&
        (Read(configuration, $"OTEL_{signal}_EXPORTER") is not { } exporters || exporters.Split(',').Any(exporter => exporter.Trim().Equals("otlp", StringComparison.OrdinalIgnoreCase)));

    static void ValidateEndpoints(IConfiguration configuration)
    {
        foreach (var signal in _signals.Where(signal => ShouldExport(configuration, signal)))
        {
            var signalKey = $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT";
            var key = Read(configuration, signalKey) is null ? "OTEL_EXPORTER_OTLP_ENDPOINT" : signalKey;
            _ = Endpoint(Read(configuration, key)!, key);
        }
    }

    static OtlpExportProtocol Protocol(IConfiguration configuration, string? signal)
    {
        var protocol = (signal is null ? null : Read(configuration, $"OTEL_EXPORTER_OTLP_{signal}_PROTOCOL")) ?? Read(configuration, "OTEL_EXPORTER_OTLP_PROTOCOL");

        return string.Equals(protocol?.Trim(), "http/protobuf", StringComparison.Ordinal) ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
    }

    static void ConfigureExporter(OtlpExporterOptions exporter, IConfiguration configuration, string signal, string path)
    {
        var signalEndpoint = Read(configuration, $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT");
        var endpoint = signalEndpoint ?? Read(configuration, "OTEL_EXPORTER_OTLP_ENDPOINT")!;
        exporter.Protocol = Protocol(configuration, signal);
        var uri = Endpoint(endpoint, signalEndpoint is null ? "OTEL_EXPORTER_OTLP_ENDPOINT" : $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT");
        exporter.Endpoint = exporter.Protocol is OtlpExportProtocol.HttpProtobuf && signalEndpoint is null
            ? new UriBuilder(uri) { Path = $"{uri.AbsolutePath.TrimEnd('/')}/v1/{path}" }.Uri
            : uri;
        exporter.Headers = Read(configuration, $"OTEL_EXPORTER_OTLP_{signal}_HEADERS") ?? Read(configuration, "OTEL_EXPORTER_OTLP_HEADERS");
    }

    static Uri Endpoint(string value, string key)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var endpoint) || endpoint.Scheme is not ("http" or "https"))
        {
            throw new InvalidOpenTelemetryEndpoint(key);
        }

        return endpoint;
    }

    static void ConfigureResourceDefaults(IServiceCollection services, CratisOpenTelemetryOptions options)
    {
        // The callback interfaces are internal to the SDK. Register through its
        // public API, then move the new descriptors before application callbacks.
        // Never build resources before the SDK supplies DI to resource detectors.
        var count = services.Count;
        services.ConfigureOpenTelemetryTracerProvider((_, tracing) => tracing.ConfigureResource(AddDefaults));
        services.ConfigureOpenTelemetryMeterProvider((_, metrics) => metrics.ConfigureResource(AddDefaults));
        services.ConfigureOpenTelemetryLoggerProvider((_, logging) => logging.ConfigureResource(AddDefaults));
        foreach (var descriptor in services.Skip(count).Reverse().ToArray())
        {
            services.Remove(descriptor);
            services.Insert(0, descriptor);
        }

        void AddDefaults(ResourceBuilder resource) => resource.AddService(options.ServiceName, serviceVersion: options.ServiceVersion);
    }

    sealed class TelemetryServiceProvider(IServiceProvider provider, IConfiguration configuration) : IKeyedServiceProvider, ISupportRequiredService
    {
        public object? GetService(Type serviceType) => serviceType == typeof(IConfiguration) ? configuration : provider.GetService(serviceType);

        public object GetRequiredService(Type serviceType) => serviceType == typeof(IConfiguration) ? configuration : provider.GetRequiredService(serviceType);

        public object? GetKeyedService(Type serviceType, object? serviceKey) => provider is IKeyedServiceProvider keyedProvider
            ? keyedProvider.GetKeyedService(serviceType, serviceKey)
            : throw new InvalidOperationException("This service provider doesn't support keyed services.");

        public object GetRequiredKeyedService(Type serviceType, object? serviceKey) => provider.GetRequiredKeyedService(serviceType, serviceKey);
    }
}
