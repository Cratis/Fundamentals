// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ImplementationsOf;

public class when_using_generated_provider_with_precomputed_contracts : Specification
{
    Type[] _instances;

    void Because() => _instances =
    [
        ..new ImplementationsOf<for_ContractToImplementorsMap.IInterface>(new Types([new for_GeneratedTypeDiscoveryRegistry.a_provider()]))
    ];

    [Fact] void should_get_the_precomputed_implementation() => _instances.ShouldContainOnly([typeof(for_ContractToImplementorsMap.ImplementationOfInterface)]);
}