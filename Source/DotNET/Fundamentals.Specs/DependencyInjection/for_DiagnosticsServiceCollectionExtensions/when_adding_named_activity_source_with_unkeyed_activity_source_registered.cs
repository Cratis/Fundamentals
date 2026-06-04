// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using DiagnosticsActivitySource = System.Diagnostics.ActivitySource;

namespace Cratis.DependencyInjection.for_DiagnosticsServiceCollectionExtensions;

public class when_adding_named_activity_source_with_unkeyed_activity_source_registered : Specification
{
    const string sourceName = "my-source";
    IServiceProvider _serviceProvider;
    Traces.IActivitySource<when_adding_named_activity_source_with_unkeyed_activity_source_registered> _typedSource;
    DiagnosticsActivitySource _source;

    void Establish()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DiagnosticsActivitySource("unkeyed"));
        services.AddNamedActivitySource(sourceName);
        _serviceProvider = services.BuildServiceProvider();
    }

    void Because()
    {
        _typedSource = _serviceProvider.GetRequiredKeyedService<Traces.IActivitySource<when_adding_named_activity_source_with_unkeyed_activity_source_registered>>(sourceName);
        _source = _serviceProvider.GetRequiredKeyedService<DiagnosticsActivitySource>(sourceName);
    }

    [Fact] void should_resolve_typed_activity_source() => _typedSource.ShouldNotBeNull();
    [Fact] void should_use_the_named_activity_source() => _typedSource.ActualSource.ShouldEqual(_source);
    [Fact] void should_have_the_correct_name() => _typedSource.ActualSource.Name.ShouldEqual(sourceName);
}
