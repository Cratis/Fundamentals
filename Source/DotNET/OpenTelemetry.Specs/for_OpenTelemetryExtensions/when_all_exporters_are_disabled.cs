// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_all_exporters_are_disabled : given.a_clean_environment
{
    readonly ServiceCollection _services = new();

    void Because()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318",
            ["OTEL_TRACES_EXPORTER"] = "none",
            ["OTEL_METRICS_EXPORTER"] = "none",
            ["OTEL_LOGS_EXPORTER"] = "none"
        }).Build();
        _services.AddOpenTelemetry().WithCratis(configuration);
    }

    [Fact] void should_not_register_an_otlp_exporter() => _services.Any(descriptor => descriptor.ServiceType == typeof(IOptionsFactory<OtlpExporterOptions>)).ShouldBeFalse();
}
