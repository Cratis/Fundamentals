// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_bindings_by_convention_with_generated_only_provider : Specification
{
    IServiceCollection _services;
    ServiceDescriptor _serviceDescriptor;

    void Establish()
    {
        _services = new ServiceCollection();
        GeneratedTypeDiscoveryRegistry.Register(new generated_only_convention_provider());
    }

    void Because()
    {
        _services.AddBindingsByConvention();
        _serviceDescriptor = _services.Single(_ => _.ServiceType == typeof(IGeneratedOnlyConvention));
    }

    [Fact] void should_register_the_interface_with_the_generated_implementation() => _serviceDescriptor.ImplementationType.ShouldEqual(typeof(GeneratedOnlyConvention));
}

public interface IGeneratedOnlyConvention;

public class GeneratedOnlyConvention : IGeneratedOnlyConvention;

class generated_only_convention_provider : ICanProvideAssembliesForDiscovery, ICanProvideContractToImplementorsForDiscovery, ICanProvideConventionsForDependencyInjection
{
    public IEnumerable<Assembly> Assemblies => [typeof(generated_only_convention_provider).Assembly];

    public IEnumerable<Type> DefinedTypes => [typeof(IGeneratedOnlyConvention), typeof(GeneratedOnlyConvention)];

    public IEnumerable<ConventionServiceBinding> ConventionServiceBindings =>
    [
        new(typeof(IGeneratedOnlyConvention), typeof(GeneratedOnlyConvention), ServiceLifetime.Transient)
    ];

    public IEnumerable<ConventionSelfBinding> SelfBindings => [new(typeof(GeneratedOnlyConvention), ServiceLifetime.Transient)];

    public IDictionary<Type, IEnumerable<Type>> ContractsAndImplementors => new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IGeneratedOnlyConvention)] = [typeof(GeneratedOnlyConvention)]
    };

    public void Initialize()
    {
    }
}