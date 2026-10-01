// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_all_exporters_are_disabled : given.a_recording_exporter
{
    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_TRACES_EXPORTER", "none");
        Environment.SetEnvironmentVariable("OTEL_METRICS_EXPORTER", "none");
        Environment.SetEnvironmentVariable("OTEL_LOGS_EXPORTER", "none");
    }

    void Because() => Export();

    [Fact] void should_not_create_exporter_clients() => _clientsCreated.ShouldEqual(0);
    [Fact] void should_not_export_any_signal() => _requests.ShouldBeEmpty();
}
