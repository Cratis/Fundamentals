// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry.when_ensuring_providers_are_registered;

public class and_assembly_references_cannot_be_enumerated : Specification
{
    Type[] _providersBefore;
    Type[] _providersAfter;
    Exception _exception;
    int _calls;

    void Establish() => _providersBefore = [.. GeneratedTypeDiscoveryRegistry.Providers.Select(_ => _.GetType())];

    void Because()
    {
        _exception = Catch.Exception(() => GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered(false, assembly =>
        {
            _calls++;
            return [];
        }));
        _providersAfter = [.. GeneratedTypeDiscoveryRegistry.Providers.Select(_ => _.GetType())];
    }

    [Fact] void should_not_throw() => _exception.ShouldBeNull();
    [Fact] void should_not_ask_any_assembly_for_its_references() => _calls.ShouldEqual(0);
    [Fact] void should_leave_the_registered_providers_unchanged() => _providersAfter.ShouldContainOnly(_providersBefore);
}
