// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.given;

public class a_recording_exporter : a_clean_environment
{
    protected ServiceCollection _services;
    protected IConfiguration _configuration;
    protected readonly ConcurrentQueue<(string Endpoint, string? Header)> _requests = new();
    protected int _clientsCreated;
    protected bool _tracesFlushed;
    protected bool _metricsFlushed;
    protected bool _logsFlushed;
    ServiceProvider _provider;

    void Establish()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318/base",
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            ["OTEL_EXPORTER_OTLP_HEADERS"] = "test-key=common-value"
        }).Build();
        _services.Configure<OtlpExporterOptions>(options => options.HttpClientFactory = () =>
        {
            Interlocked.Increment(ref _clientsCreated);
            return new HttpClient(new recording_http(_requests));
        });
    }

    protected void Export()
    {
        _services.AddOpenTelemetry().WithCratis(_configuration);
        _provider = _services.BuildServiceProvider();
        ExportSignals(_provider);
    }

    protected void ExportSignals(IServiceProvider provider)
    {
        var traces = provider.GetRequiredService<TracerProvider>();
        var metrics = provider.GetRequiredService<MeterProvider>();
        var logs = provider.GetRequiredService<LoggerProvider>();
        using var source = new ActivitySource("Cratis.Test.Exporter");
        using (source.StartActivity("cratis.test.export"))
        {
        }
        using var meter = new Meter("Cratis.Test.Exporter");
        meter.CreateCounter<long>("cratis.test.count").Add(1);
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Cratis.Test.Exporter");
        LoggerMessage.Define(LogLevel.Information, new EventId(1), "Export a log")(logger, null);
        _tracesFlushed = traces.ForceFlush(5_000);
        _metricsFlushed = metrics.ForceFlush(5_000);
        _logsFlushed = logs.ForceFlush(5_000);
    }

    void Destroy() => _provider?.Dispose();

    sealed class recording_http(ConcurrentQueue<(string Endpoint, string? Header)> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(Record(request));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => Record(request);

        HttpResponseMessage Record(HttpRequestMessage request)
        {
            var header = request.Headers.TryGetValues("test-key", out var values) ? values.Single() : null;
            requests.Enqueue((request.RequestUri!.AbsoluteUri, header));

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([]) };
        }
    }
}
