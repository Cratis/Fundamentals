// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry;

public class when_the_package_itself_is_loaded : Specification
{
    static readonly System.Reflection.Assembly _packageAssembly = typeof(Types).Assembly;

    bool _registeredItsOwnProvider;

    void Because()
    {
        // This assembly is built with the type discovery generator, so loading it runs a module
        // initializer that registers a provider - before anything can reach Types at all. That is what
        // makes TypeDiscoveryMode.Reflected unreachable: the registry is never empty, so the reflection
        // fallback is never taken. If this ever goes red, that fallback has come back to life and its
        // behavior needs covering.
        _registeredItsOwnProvider = GeneratedTypeDiscoveryRegistry.Providers
            .Any(provider => provider.Assemblies.Contains(_packageAssembly));
    }

    [Fact]
    void should_register_a_provider_for_its_own_assembly() => _registeredItsOwnProvider.ShouldBeTrue();
}
