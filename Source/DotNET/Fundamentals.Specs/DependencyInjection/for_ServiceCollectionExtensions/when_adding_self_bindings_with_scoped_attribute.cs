// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_self_bindings_with_scoped_attribute : Specification
{
    IServiceCollection _services;
    ServiceDescriptor _serviceDescriptor;

    void Establish()
    {
        _services = new ServiceCollection();
        _services.AddSingleton<IExistingService, ExistingService>();
    }

    void Because()
    {
        _services.AddSelfBindings();
        _serviceDescriptor = _services.FirstOrDefault(_ => _.ServiceType == typeof(MyScopedSelfBinding));
    }

    [Fact] void should_register_the_implementation_type() => _serviceDescriptor.ImplementationType.ShouldEqual(typeof(MyScopedSelfBinding));
    [Fact] void should_register_with_scoped_lifetime() => _serviceDescriptor.Lifetime.ShouldEqual(ServiceLifetime.Scoped);
}

public interface IExistingService;

public class ExistingService : IExistingService;

[Scoped]
public class MyScopedSelfBinding;
