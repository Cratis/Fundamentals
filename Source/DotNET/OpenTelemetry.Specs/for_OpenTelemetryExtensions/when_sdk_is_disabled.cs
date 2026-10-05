// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_sdk_is_disabled : given.a_clean_environment
{
    HostApplicationBuilder _builder;
    bool _activityRecorded;

    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", "true");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317");
        _builder = Host.CreateApplicationBuilder();
    }

    void Because()
    {
        _builder.AddCratisOpenTelemetry();
        using var host = _builder.Build();
        host.Services.GetRequiredService<TracerProvider>();
        using var source = new ActivitySource("Cratis.Test.Disabled");
        using var activity = source.StartActivity("cratis.test");
        _activityRecorded = activity is not null;
    }

    [Fact] void should_not_record_an_activity() => _activityRecorded.ShouldBeFalse();
}
