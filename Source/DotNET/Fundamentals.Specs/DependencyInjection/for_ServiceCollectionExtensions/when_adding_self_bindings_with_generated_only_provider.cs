// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.DependencyInjection.for_ServiceCollectionExtensions;

public class when_adding_self_bindings_with_generated_only_provider : Specification
{
    IServiceCollection _services;

    void Establish()
    {
        _services = new ServiceCollection();
        GeneratedTypeDiscoveryRegistry.Register(new Types.for_GeneratedTypeDiscoveryRegistry.a_provider());
    }

    void Because() => _services.AddSelfBindings();

    [Fact] void should_register_the_generated_implementation_as_self_binding() => _services.Any(_ =>
        _.ServiceType == typeof(Types.for_ContractToImplementorsMap.ImplementationOfInterface) &&
        _.ImplementationType == typeof(Types.for_ContractToImplementorsMap.ImplementationOfInterface)).ShouldBeTrue();
}