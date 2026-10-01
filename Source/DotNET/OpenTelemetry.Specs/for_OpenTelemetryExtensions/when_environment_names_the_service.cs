// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_environment_names_the_service : given.a_clean_environment
{
    IHost _host;
    IEnumerable<KeyValuePair<string, object>> _attributes;

    void Establish()
    {
        Environment.SetEnvironmentVariable("OTEL_SERVICE_NAME", "environment-service");
        Environment.SetEnvironmentVariable("OTEL_RESOURCE_ATTRIBUTES", "service.name=resource-service,service.namespace=customer,deployment.environment.name=production");
    }

    void Because()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration["OTEL_SERVICE_NAME"] = "configuration-service";
        builder.AddCratisOpenTelemetry(options => options.ServiceName = "code-service");
        _host = builder.Build();
        _attributes = _host.Services.GetRequiredService<TracerProvider>().GetResource().Attributes;
    }

    void Destroy() => _host.Dispose();

    [Fact] void should_prefer_the_environment_over_configuration_and_code() => _attributes.Single(attribute => attribute.Key == "service.name").Value.ShouldEqual("environment-service");
    [Fact] void should_preserve_the_resource_namespace() => _attributes.Single(attribute => attribute.Key == "service.namespace").Value.ShouldEqual("customer");
    [Fact] void should_preserve_the_deployment_environment() => _attributes.Single(attribute => attribute.Key == "deployment.environment.name").Value.ShouldEqual("production");
    [Fact] void should_supply_an_assembly_version() => _attributes.Any(attribute => attribute.Key == "service.version" && !string.IsNullOrEmpty(attribute.Value.ToString())).ShouldBeTrue();
}
