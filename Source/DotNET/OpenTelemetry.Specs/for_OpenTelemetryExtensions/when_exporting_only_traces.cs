// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_exporting_only_traces : given.a_clean_environment
{
    IHost _host;
    readonly recording_http _http = new();
    bool _flushed;

    void Because()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318/base";
        builder.Configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "http://traces:4318/custom";
        builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf";
        builder.Configuration["OTEL_EXPORTER_OTLP_TRACES_HEADERS"] = "test-key=test-value";
        builder.Configuration["OTEL_METRICS_EXPORTER"] = "none";
        builder.Configuration["OTEL_LOGS_EXPORTER"] = "none";
        builder.Services.Configure<OtlpExporterOptions>(options => options.HttpClientFactory = () => new HttpClient(_http));
        builder.AddCratisOpenTelemetry();
        _host = builder.Build();
        var provider = _host.Services.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Test.Exporter");
        using (source.StartActivity("cratis.test.export"))
        {
        }
        _flushed = provider.ForceFlush(5_000);
    }

    void Destroy() => _host.Dispose();

    [Fact] void should_export_successfully() => _flushed.ShouldBeTrue();
    [Fact] void should_only_send_the_configured_signal() => _http.Requests.ShouldContainOnly("http://traces:4318/custom");
    [Fact] void should_honor_signal_headers() => _http.Header.ShouldEqual("test-value");

    sealed class recording_http : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];
        public string? Header { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(Record(request));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => Record(request);

        HttpResponseMessage Record(HttpRequestMessage request)
        {
            Requests.Add(request.RequestUri!.AbsoluteUri);
            Header = request.Headers.GetValues("test-key").Single();

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([]) };
        }
    }
}
