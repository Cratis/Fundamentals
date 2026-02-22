// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_bindings_by_convention_with_scoped_implementation : Specification
{
    IServiceCollection _services;
    ServiceDescriptor _serviceDescriptor;

    void Establish() => _services = new ServiceCollection();

    void Because()
    {
        _services.AddBindingsByConvention();
        _serviceDescriptor = _services.Single(_ => _.ServiceType == typeof(IMyScopedBinding));
    }

    [Fact] void should_register_the_implementation() => _serviceDescriptor.ImplementationType.ShouldEqual(typeof(MyScopedBinding));
    [Fact] void should_register_with_scoped_lifetime() => _serviceDescriptor.Lifetime.ShouldEqual(ServiceLifetime.Scoped);
}

public interface IMyScopedBinding;

[Scoped]
public class MyScopedBinding : IMyScopedBinding;
