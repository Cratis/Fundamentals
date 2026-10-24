// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.when_preserving_application_propagation;

public class without_configured_propagators : given.a_configured_environment
{
    readonly TextMapPropagator _applicationPropagator = new BaggagePropagator();

    void Establish() => Sdk.SetDefaultTextMapPropagator(_applicationPropagator);

    void Because()
    {
        var builder = CreateBuilder();
        builder.AddCratisOpenTelemetry();
        using var services = builder.Services.BuildServiceProvider();
        using var provider = services.GetRequiredService<TracerProvider>();
    }

    [Fact] void should_preserve_the_application_propagator() => ReferenceEquals(Propagators.DefaultTextMapPropagator, _applicationPropagator).ShouldBeTrue();
}
