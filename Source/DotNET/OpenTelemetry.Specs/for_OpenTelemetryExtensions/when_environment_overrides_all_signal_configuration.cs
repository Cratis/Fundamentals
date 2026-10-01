// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_environment_overrides_all_signal_configuration : given.a_recording_exporter
{
    void Establish()
    {
        _configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "http://configuration:4318/traces";
        _configuration["OTEL_EXPORTER_OTLP_TRACES_HEADERS"] = "test-key=configuration-value";
        _configuration["OTEL_EXPORTER_OTLP_TRACES_PROTOCOL"] = "grpc";
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", "http://environment:4318/traces");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_HEADERS", "test-key=environment-value");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_PROTOCOL", "http/protobuf");
    }

    void Because() => Export();

    [Fact] void should_export_traces_to_the_environment_endpoint() => _requests.ShouldContain(("http://environment:4318/traces", "environment-value"));
    [Fact] void should_not_export_to_the_configuration_endpoint() => _requests.Any(request => request.Endpoint == "http://configuration:4318/traces").ShouldBeFalse();
    [Fact] void should_keep_metrics_enabled() => _requests.ShouldContain(("http://collector:4318/base/v1/metrics", "common-value"));
    [Fact] void should_keep_logs_enabled() => _requests.ShouldContain(("http://collector:4318/base/v1/logs", "common-value"));
}
