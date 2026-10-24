// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions.given;

public class a_configured_environment : a_clean_environment
{
    readonly TextMapPropagator _previous = Propagators.DefaultTextMapPropagator;

    protected static HostApplicationBuilder CreateBuilder(IConfiguration? configuration = null)
    {
        var builder = Host.CreateApplicationBuilder();
        if (configuration is not null) builder.Configuration.AddConfiguration(configuration);
        return builder;
    }

    void Destroy() => Sdk.SetDefaultTextMapPropagator(_previous);
}
