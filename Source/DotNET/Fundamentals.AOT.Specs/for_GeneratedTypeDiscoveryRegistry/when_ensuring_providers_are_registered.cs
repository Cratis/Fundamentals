// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Types;

namespace Cratis.Fundamentals.AOT.Specs.for_GeneratedTypeDiscoveryRegistry;

public class when_ensuring_providers_are_registered : Specification
{
    Exception _exception;

    void Because() => _exception = Catch.Exception(() =>
    {
        GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();
        GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();
    });

    [Fact] void should_not_throw() => _exception.ShouldBeNull();
    [Fact] void should_have_providers() => GeneratedTypeDiscoveryRegistry.Providers.ShouldNotBeEmpty();
}
