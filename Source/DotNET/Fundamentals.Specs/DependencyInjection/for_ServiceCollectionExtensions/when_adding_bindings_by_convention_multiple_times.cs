// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_bindings_by_convention_multiple_times : Specification
{
    ServiceCollection _services;
    Type[] _duplicatedServiceTypes;

    void Establish() => _services = new ServiceCollection();

    void Because()
    {
        _services.AddBindingsByConvention();
        _services.AddBindingsByConvention();

        // Asserted as "no service type twice" rather than comparing counts between the calls -
        // the generated provider registry is process-global and other specs register providers
        // concurrently, so the total count can legitimately grow between the two calls.
        _duplicatedServiceTypes = [.. _services.GroupBy(_ => _.ServiceType).Where(_ => _.Count() > 1).Select(_ => _.Key)];
    }

    [Fact] void should_not_register_any_service_type_more_than_once() => _duplicatedServiceTypes.ShouldBeEmpty();
}
