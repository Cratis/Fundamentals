// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_no_endpoint_is_configured : given.a_recording_exporter
{
    void Establish() => _configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = null;

    void Because() => Export();

    [Fact] void should_not_create_exporter_clients() => _clientsCreated.ShouldEqual(0);
    [Fact] void should_not_export_any_signal() => _requests.ShouldBeEmpty();
}
