// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry.when_ensuring_providers_are_registered;

public class and_assembly_references_cannot_be_enumerated : Specification
{
    Exception _exception;
    int _calls;

    void Because() =>
        _exception = Catch.Exception(() => GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered(false, assembly =>
        {
            _calls++;
            return [];
        }));

    [Fact] void should_not_throw() => _exception.ShouldBeNull();
    [Fact] void should_not_ask_any_assembly_for_its_references() => _calls.ShouldEqual(0);
}
