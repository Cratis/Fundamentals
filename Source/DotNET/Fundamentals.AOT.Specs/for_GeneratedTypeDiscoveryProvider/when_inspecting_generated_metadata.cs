// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_GeneratedTypeDiscoveryProvider;

public class when_inspecting_generated_metadata : Specification
{
    ICanProvideAssembliesForDiscovery[] assembly_providers;
    ICanProvideContractToImplementorsForDiscovery[] contract_providers;
    ICanProvideConventionsForDependencyInjection[] convention_providers;

    Type[] defined_types;
    (Type Contract, IEnumerable<Type> Implementors)[] contracts_and_implementors;
    ConventionServiceBinding[] convention_service_bindings;
    ConventionSelfBinding[] self_bindings;

    void Because()
    {
        assembly_providers = [..GeneratedTypeDiscoveryRegistry.Providers.OfType<ICanProvideAssembliesForDiscovery>()];
        contract_providers = [..GeneratedTypeDiscoveryRegistry.Providers.OfType<ICanProvideContractToImplementorsForDiscovery>()];
        convention_providers = [..GeneratedTypeDiscoveryRegistry.Providers.OfType<ICanProvideConventionsForDependencyInjection>()];

        defined_types = [..assembly_providers.SelectMany(_ => _.DefinedTypes)];
        contracts_and_implementors = [..contract_providers
            .SelectMany(_ => _.ContractsAndImplementors)
            .Select(_ => (_.Key, _.Value))];
        convention_service_bindings = [..convention_providers.SelectMany(_ => _.ConventionServiceBindings)];
        self_bindings = [..convention_providers.SelectMany(_ => _.SelfBindings)];
    }

    [Fact] void should_include_first_implementation_in_defined_types() => defined_types.ShouldContain(typeof(FirstImplementation));
    [Fact] void should_include_second_implementation_in_defined_types() => defined_types.ShouldContain(typeof(SecondImplementation));
    [Fact] void should_include_conventional_service_in_defined_types() => defined_types.ShouldContain(typeof(ConventionalService));
    [Fact] void should_include_scoped_self_binding_in_defined_types() => defined_types.ShouldContain(typeof(ScopedSelfBinding));
    [Fact] void should_include_generic_demo_in_defined_types() => defined_types.ShouldContain(typeof(GenericDemo));
    [Fact] void should_map_some_interface_to_first_implementation() =>
        contracts_and_implementors.ShouldContain(_ =>
            _.Contract == typeof(ISomeInterface) && _.Implementors.Contains(typeof(FirstImplementation)));
    [Fact] void should_map_some_interface_to_second_implementation() =>
        contracts_and_implementors.ShouldContain(_ =>
            _.Contract == typeof(ISomeInterface) && _.Implementors.Contains(typeof(SecondImplementation)));
    [Fact] void should_map_generic_interface_to_generic_demo() =>
        contracts_and_implementors.ShouldContain(_ =>
            _.Contract == typeof(IGenericDemo<>) && _.Implementors.Contains(typeof(GenericDemo)));
    [Fact] void should_emit_convention_binding_for_conventional_service() =>
        convention_service_bindings.ShouldContain(_ =>
            _.ServiceType == typeof(IConventionalService) &&
            _.ImplementationType == typeof(ConventionalService));
    [Fact] void should_emit_self_binding_for_scoped_self_binding() =>
        self_bindings.ShouldContain(_ =>
            _.ImplementationType == typeof(ScopedSelfBinding) &&
            _.Lifetime == ServiceLifetime.Scoped);
}
