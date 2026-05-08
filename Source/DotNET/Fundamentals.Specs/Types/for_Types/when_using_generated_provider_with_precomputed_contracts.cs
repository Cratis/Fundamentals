// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_Types;

public class when_using_generated_provider_with_precomputed_contracts : Specification
{
    Type[] _implementors;

    void Because() => _implementors =
    [
        ..new Types([new for_GeneratedTypeDiscoveryRegistry.a_provider()])
            .FindMultiple<for_ContractToImplementorsMap.IInterface>()
    ];

    [Fact] void should_resolve_implementors_from_precomputed_contracts() => _implementors.ShouldContainOnly(typeof(for_ContractToImplementorsMap.ImplementationOfInterface));
}