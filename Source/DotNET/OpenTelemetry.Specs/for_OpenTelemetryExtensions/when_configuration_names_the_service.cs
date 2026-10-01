// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_configuration_names_the_service : given.a_clean_environment
{
    ServiceProvider _provider;
    object _name;

    void Because()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_SERVICE_NAME"] = "configuration-service"
        }).Build();
        var services = new ServiceCollection();
        services.AddOpenTelemetry().WithCratis(configuration, options => options.ServiceName = "code-service");
        _provider = services.BuildServiceProvider();
        _name = _provider.GetRequiredService<TracerProvider>().GetResource().Attributes.Single(attribute => attribute.Key == "service.name").Value;
    }

    void Destroy() => _provider.Dispose();

    [Fact] void should_prefer_standard_configuration_over_the_fallback() => _name.ShouldEqual("configuration-service");
}
