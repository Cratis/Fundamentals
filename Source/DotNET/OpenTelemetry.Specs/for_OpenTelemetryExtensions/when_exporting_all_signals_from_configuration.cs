// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_exporting_all_signals_from_configuration : given.a_recording_exporter
{
    void Establish()
    {
        _configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "http://traces:4318/custom";
        _configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"] = "http://logs:4318/custom";
        _configuration["OTEL_EXPORTER_OTLP_LOGS_HEADERS"] = "test-key=logs-value";
        _configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] = "grpc";
        _configuration["OTEL_EXPORTER_OTLP_TRACES_PROTOCOL"] = " http/protobuf ";
        _configuration["OTEL_EXPORTER_OTLP_METRICS_PROTOCOL"] = " http/protobuf ";
        _configuration["OTEL_EXPORTER_OTLP_LOGS_PROTOCOL"] = " http/protobuf ";
    }

    void Because() => Export();

    [Fact] void should_flush_traces() => _tracesFlushed.ShouldBeTrue();
    [Fact] void should_flush_metrics() => _metricsFlushed.ShouldBeTrue();
    [Fact] void should_flush_logs() => _logsFlushed.ShouldBeTrue();
    [Fact] void should_use_the_trace_override_and_common_headers() => _requests.ShouldContain(("http://traces:4318/custom", "common-value"));
    [Fact] void should_append_the_metric_path_and_use_common_headers() => _requests.ShouldContain(("http://collector:4318/base/v1/metrics", "common-value"));
    [Fact] void should_use_the_log_override_and_signal_headers() => _requests.ShouldContain(("http://logs:4318/custom", "logs-value"));
    [Fact] void should_only_send_to_effective_endpoints() => _requests.Select(request => request.Endpoint).Distinct().ShouldContainOnly("http://traces:4318/custom", "http://collector:4318/base/v1/metrics", "http://logs:4318/custom");
}
