// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry;

public class when_registering_the_same_provider_type_twice : Specification
{
    int _countAfterFirstRegister;
    int _countAfterSecondRegister;

    void Because()
    {
        var provider = new a_provider();
        GeneratedTypeDiscoveryRegistry.Register(provider);
        _countAfterFirstRegister = GeneratedTypeDiscoveryRegistry.Providers.OfType<a_provider>().Count();
        GeneratedTypeDiscoveryRegistry.Register(provider);
        _countAfterSecondRegister = GeneratedTypeDiscoveryRegistry.Providers.OfType<a_provider>().Count();
    }

    [Fact] void should_only_add_the_provider_type_once() => _countAfterSecondRegister.ShouldEqual(_countAfterFirstRegister);
}