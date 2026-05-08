// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_Types;

public class when_using_generated_type_discovery_for_current_assembly : Specification
{
    Type[] result;

    void Because() => result =
    [
        ..new Types(GeneratedTypeDiscoveryRegistry.Providers)
            .FindMultiple<global::Cratis.Types.for_ContractToImplementorsMap.IInterface>()
    ];

    [Fact] void should_find_the_known_implementations() => result.ShouldContainOnly(
    [
        typeof(global::Cratis.Types.for_ContractToImplementorsMap.ImplementationOfInterface),
        typeof(global::Cratis.Types.for_ContractToImplementorsMap.SecondImplementationOfInterface),
        typeof(global::Cratis.Types.for_ContractToImplementorsMap.ImplementationOfAbstractClassWithInterface),
        typeof(global::Cratis.Types.for_ContractToImplementorsMap.SecondImplementationOfAbstractClassWithInterface)
    ]);
}