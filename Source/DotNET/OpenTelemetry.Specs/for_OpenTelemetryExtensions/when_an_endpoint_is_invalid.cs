// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_an_endpoint_is_invalid : given.a_clean_environment
{
    [Theory]
    [InlineData("/relative")]
    [InlineData("ftp://collector:4318")]
    [InlineData("not a URI")]
    void should_fail_setup_with_the_endpoint_error(string endpoint)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint
        }).Build();
        var error = Catch.Exception(() => new ServiceCollection().AddOpenTelemetry().WithCratis(configuration));
        error.ShouldBeOfExactType<InvalidOpenTelemetryEndpoint>();
    }
}
