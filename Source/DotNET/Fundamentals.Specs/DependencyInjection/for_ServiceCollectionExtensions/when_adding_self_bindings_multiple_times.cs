// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_self_bindings_multiple_times : Specification
{
    IServiceCollection _services;
    int _meterBindings;
    int _activitySourceBindings;

    void Establish() => _services = new ServiceCollection();

    void Because()
    {
        _services.AddSelfBindings();
        _services.AddSelfBindings();

        _meterBindings = _services.Count(_ => _.ServiceType == typeof(Metrics.IMeter<>));
        _activitySourceBindings = _services.Count(_ => _.ServiceType == typeof(Traces.IActivitySource<>));
    }

    [Fact] void should_only_add_one_open_generic_meter_registration() => _meterBindings.ShouldEqual(1);
    [Fact] void should_only_add_one_open_generic_activity_source_registration() => _activitySourceBindings.ShouldEqual(1);
}
