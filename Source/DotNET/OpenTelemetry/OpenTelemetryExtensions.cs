// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
    /// Adds Cratis telemetry, using a registered configuration instance or standard environment variables.
    /// </summary>
    /// <param name="builder">The OpenTelemetry builder.</param>
    /// <param name="configure">Optional application defaults and extra instrumentation.</param>
    /// <returns>The OpenTelemetry builder for continuation.</returns>
    public static OpenTelemetryBuilder WithCratis(this OpenTelemetryBuilder builder, Action<CratisOpenTelemetryOptions>? configure = null)
    {
        var configuration = builder.Services.LastOrDefault(descriptor => descriptor.ServiceType == typeof(IConfiguration))?.ImplementationInstance as IConfiguration
            ?? new ConfigurationBuilder().Build();

        return builder.WithCratis(configuration, configure);
    }

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
        if (string.Equals(Read(configuration, "OTEL_SDK_DISABLED"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return builder;
        }

        var options = new CratisOpenTelemetryOptions();
        configure?.Invoke(options);
        builder.Services.TryAddSingleton(configuration);
        builder.ConfigureResource(resource => ConfigureResource(resource, configuration, options)).WithLogging(_ => { }, logging =>
        {
            logging.IncludeScopes = true;
            logging.IncludeFormattedMessage = true;
        }).WithTracing(tracing =>
        {
            tracing.AddCratisInstrumentation().AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
            options.ConfigureTracing?.Invoke(tracing);
        }).WithMetrics(metrics =>
        {
            metrics.AddCratisInstrumentation().AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation();
            options.ConfigureMetrics?.Invoke(metrics);
        });
        ConfigureExport(builder, configuration);

        return builder;
    }

    static string? Read(IConfiguration configuration, string key) => Environment.GetEnvironmentVariable(key) ?? configuration[key];

    static void ConfigureResource(ResourceBuilder resource, IConfiguration configuration, CratisOpenTelemetryOptions options)
    {
        resource.AddService(options.ServiceName, serviceVersion: options.ServiceVersion);
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

    static void ConfigureExport(OpenTelemetryBuilder builder, IConfiguration configuration)
    {
        var traces = ShouldExport(configuration, "TRACES");
        var metrics = ShouldExport(configuration, "METRICS");
        var logs = ShouldExport(configuration, "LOGS");
        var commonEndpoint = Read(configuration, "OTEL_EXPORTER_OTLP_ENDPOINT");
        foreach (var signal in _signals.Where(signal => ShouldExport(configuration, signal)))
        {
            var key = $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT";
            _ = Endpoint(Read(configuration, key) ?? commonEndpoint!, key);
        }
        if (traces && metrics && logs && !string.IsNullOrWhiteSpace(commonEndpoint))
        {
            // The SDK's shared exporter honors per-signal overrides and metric export intervals.
            builder.UseOtlpExporter(Protocol(configuration, null), Endpoint(commonEndpoint, "OTEL_EXPORTER_OTLP_ENDPOINT"));

            return;
        }
        if (traces)
        {
            builder.WithTracing(tracing => tracing.AddOtlpExporter(exporter => ConfigureExporter(exporter, configuration, "TRACES", "traces")));
        }
        if (metrics)
        {
            builder.WithMetrics(meter => meter.AddOtlpExporter(exporter => ConfigureExporter(exporter, configuration, "METRICS", "metrics")));
        }
        if (logs)
        {
            builder.WithLogging(logging => logging.AddOtlpExporter(exporter => ConfigureExporter(exporter, configuration, "LOGS", "logs")));
        }
    }

    static OtlpExportProtocol Protocol(IConfiguration configuration, string? signal)
    {
        var protocol = (signal is null ? null : Read(configuration, $"OTEL_EXPORTER_OTLP_{signal}_PROTOCOL")) ?? Read(configuration, "OTEL_EXPORTER_OTLP_PROTOCOL");

        return string.Equals(protocol, "http/protobuf", StringComparison.Ordinal) ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
    }

    static void ConfigureExporter(OtlpExporterOptions exporter, IConfiguration configuration, string signal, string path)
    {
        var signalEndpoint = Read(configuration, $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT");
        var endpoint = signalEndpoint ?? Read(configuration, "OTEL_EXPORTER_OTLP_ENDPOINT")!;
        exporter.Protocol = Protocol(configuration, signal);
        var uri = Endpoint(endpoint, $"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT");
        exporter.Endpoint = exporter.Protocol is OtlpExportProtocol.HttpProtobuf && signalEndpoint is null
            ? new Uri($"{uri.AbsoluteUri.TrimEnd('/')}/v1/{path}")
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
}
