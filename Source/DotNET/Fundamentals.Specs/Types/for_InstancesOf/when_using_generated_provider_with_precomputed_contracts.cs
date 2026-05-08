// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_InstancesOf;

public class when_using_generated_provider_with_precomputed_contracts : Specification
{
    object[] _instances;

    void Because()
    {
        var implementation = new for_ContractToImplementorsMap.ImplementationOfInterface();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(_ => _.GetService(typeof(for_ContractToImplementorsMap.ImplementationOfInterface))).Returns(implementation);

        _instances =
        [
            ..new InstancesOf<for_ContractToImplementorsMap.IInterface>(
                new Types([new for_GeneratedTypeDiscoveryRegistry.a_provider()]),
                serviceProvider.Object)
        ];
    }

    [Fact] void should_resolve_instances_for_the_precomputed_implementor() => _instances.Single().ShouldBeOfExactType<for_ContractToImplementorsMap.ImplementationOfInterface>();
}