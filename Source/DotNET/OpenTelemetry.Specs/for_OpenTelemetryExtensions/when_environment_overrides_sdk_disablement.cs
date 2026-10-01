// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cratis.OpenTelemetry.for_OpenTelemetryExtensions;

public class when_environment_overrides_sdk_disablement : given.a_recording_exporter
{
    [Theory]
    [InlineData(false, "host", false)]
    [InlineData(true, "host", false)]
    [InlineData(false, "composition", false)]
    [InlineData(true, "composition", false)]
    [InlineData(false, "separate", false)]
    [InlineData(true, "separate", false)]
    [InlineData(false, "host", true)]
    [InlineData(true, "host", true)]
    [InlineData(false, "composition", true)]
    [InlineData(true, "composition", true)]
    [InlineData(false, "separate", true)]
    [InlineData(true, "separate", true)]
    void should_apply_the_environment_decision_to_all_providers_without_changing_application_configuration(bool web, string setup, bool disabled)
    {
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", disabled ? "true" : "false");
        IHostApplicationBuilder builder = web ? WebApplication.CreateBuilder() : Host.CreateApplicationBuilder();
        foreach (var descriptor in _services)
        {
            builder.Services.Add(descriptor);
        }
        builder.Services.AddOpenTelemetry().WithTracing(_ => { }).WithMetrics(_ => { }).WithLogging(_ => { });
        builder.Configuration.AddConfiguration(_configuration);
        builder.Configuration["OTEL_SDK_DISABLED"] = disabled ? "false" : "true";
        if (setup == "host")
        {
            builder.AddCratisOpenTelemetry();
        }
        else if (setup == "composition")
        {
            builder.Services.AddOpenTelemetry().WithCratis();
        }
        else
        {
            _configuration["OTEL_SDK_DISABLED"] = disabled ? "false" : "true";
            builder.Services.AddOpenTelemetry().WithCratis(_configuration);
        }
        using var host = web ? ((WebApplicationBuilder)builder).Build() : ((HostApplicationBuilder)builder).Build();
        ExportSignals(host.Services);
        ReferenceEquals(host.Services.GetRequiredService<IConfiguration>(), builder.Configuration).ShouldBeTrue();
        builder.Configuration["OTEL_SDK_DISABLED"].ShouldEqual(disabled ? "false" : "true");
        _clientsCreated.ShouldEqual(disabled ? 0 : 3);
        if (disabled)
        {
            _requests.ShouldBeEmpty();
        }
        else
        {
            _requests.Select(request => request.Endpoint).Distinct().Count().ShouldEqual(3);
        }
    }
}
