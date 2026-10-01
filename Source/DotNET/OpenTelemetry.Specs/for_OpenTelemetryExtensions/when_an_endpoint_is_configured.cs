// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_an_endpoint_is_configured : given.a_recording_exporter
{
    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://collector:4318");
        _configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] = " http/protobuf ";
    }

    void Because() => Export();

    [Fact] void should_export_traces_to_the_common_endpoint() => _requests.ShouldContain(("http://collector:4318/v1/traces", "common-value"));
    [Fact] void should_export_metrics_to_the_common_endpoint() => _requests.ShouldContain(("http://collector:4318/v1/metrics", "common-value"));
    [Fact] void should_export_logs_to_the_common_endpoint() => _requests.ShouldContain(("http://collector:4318/v1/logs", "common-value"));
}
