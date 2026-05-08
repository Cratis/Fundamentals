// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_Types;

public class when_only_generated_providers_are_registered : Specification
{
    Type[] _all;

    void Because()
    {
        GeneratedTypeDiscoveryRegistry.Register(new for_GeneratedTypeDiscoveryRegistry.a_provider());
        _all = [..new Types().All];
    }

    [Fact] void should_use_generated_provider_types() => _all.ShouldContain(typeof(for_ContractToImplementorsMap.ImplementationOfInterface));
}