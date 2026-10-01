// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_an_endpoint_is_invalid : given.a_clean_environment
{
    [Theory]
    [InlineData("/relative", "OTEL_EXPORTER_OTLP_ENDPOINT")]
    [InlineData("ftp://collector:4318", "OTEL_EXPORTER_OTLP_ENDPOINT")]
    [InlineData("not a URI", "OTEL_EXPORTER_OTLP_ENDPOINT")]
    [InlineData("/relative", "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT")]
    [InlineData("/relative", "OTEL_EXPORTER_OTLP_METRICS_ENDPOINT")]
    [InlineData("/relative", "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT")]
    void should_fail_setup_with_the_endpoint_error(string endpoint, string key)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [key] = endpoint
        }).Build();
        var error = Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(configuration));
        error.ShouldBeOfExactType<InvalidOpenTelemetryEndpoint>();
        error.Message.ShouldEqual($"{key} must be an absolute HTTP or HTTPS URI.");
    }
}
