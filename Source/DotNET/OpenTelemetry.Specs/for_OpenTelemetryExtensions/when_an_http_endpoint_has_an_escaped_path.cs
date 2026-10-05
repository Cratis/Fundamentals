// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_an_http_endpoint_has_an_escaped_path : given.a_recording_exporter
{
    void Establish() => _configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318/base%20path/";

    void Because() => Export();

    [Fact] void should_append_the_trace_path_without_double_escaping() => _requests.ShouldContain(("http://collector:4318/base%20path/v1/traces", "common-value"));
    [Fact] void should_append_the_metric_path_without_double_escaping() => _requests.ShouldContain(("http://collector:4318/base%20path/v1/metrics", "common-value"));
    [Fact] void should_append_the_log_path_without_double_escaping() => _requests.ShouldContain(("http://collector:4318/base%20path/v1/logs", "common-value"));
}
