// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_an_http_endpoint_has_a_query_string : given.a_recording_exporter
{
    void Establish() => _configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318/base/?tenant=abc&region=west";

    void Because() => Export();

    [Fact] void should_append_the_trace_path_and_preserve_the_query() => _requests.ShouldContain(("http://collector:4318/base/v1/traces?tenant=abc&region=west", "common-value"));
    [Fact] void should_append_the_metric_path_and_preserve_the_query() => _requests.ShouldContain(("http://collector:4318/base/v1/metrics?tenant=abc&region=west", "common-value"));
    [Fact] void should_append_the_log_path_and_preserve_the_query() => _requests.ShouldContain(("http://collector:4318/base/v1/logs?tenant=abc&region=west", "common-value"));
}
