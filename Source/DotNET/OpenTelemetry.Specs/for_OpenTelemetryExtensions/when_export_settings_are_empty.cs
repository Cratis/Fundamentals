// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_export_settings_are_empty : given.a_recording_exporter
{
    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", " ");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", " ");
        Environment.SetEnvironmentVariable("OTEL_TRACES_EXPORTER", " ");
        _configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = string.Empty;
        _configuration["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"] = " ";
        _configuration["OTEL_TRACES_EXPORTER"] = string.Empty;
        _configuration["OTEL_METRICS_EXPORTER"] = " ";
        _configuration["OTEL_EXPORTER_OTLP_TRACES_HEADERS"] = " ";
        _configuration["OTEL_EXPORTER_OTLP_TRACES_PROTOCOL"] = " ";
    }

    void Because() => Export();

    [Fact] void should_export_traces_using_common_settings() => _requests.ShouldContain(("http://collector:4318/base/v1/traces", "common-value"));
    [Fact] void should_export_metrics_using_common_settings() => _requests.ShouldContain(("http://collector:4318/base/v1/metrics", "common-value"));
    [Fact] void should_export_logs_using_common_settings() => _requests.ShouldContain(("http://collector:4318/base/v1/logs", "common-value"));
}
