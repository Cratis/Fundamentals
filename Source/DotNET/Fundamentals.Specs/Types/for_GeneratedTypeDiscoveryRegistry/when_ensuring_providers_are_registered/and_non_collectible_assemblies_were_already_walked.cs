// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry.when_ensuring_providers_are_registered;

public class and_non_collectible_assemblies_were_already_walked : Specification
{
    readonly Assembly _alreadyWalked = typeof(GeneratedTypeDiscoveryRegistry).Assembly;
    int _timesItsReferencesWereEnumerated;

    void Establish() => GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered();

    /// <summary>
    /// Other spec classes load assemblies in parallel, so this call may still walk something new. Whatever it walks,
    /// an assembly a previous walk processed is never walked from again.
    /// </summary>
    void Because() => GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered(true, assembly =>
    {
        if (assembly == _alreadyWalked)
        {
            Interlocked.Increment(ref _timesItsReferencesWereEnumerated);
        }

        return assembly.GetReferencedAssemblies();
    });

    [Fact] void should_not_walk_it_again() => _timesItsReferencesWereEnumerated.ShouldEqual(0);
}
