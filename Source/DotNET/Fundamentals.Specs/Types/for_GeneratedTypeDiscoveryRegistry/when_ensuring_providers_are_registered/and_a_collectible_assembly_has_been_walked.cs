// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Cratis.Types.for_GeneratedTypeDiscoveryRegistry.when_ensuring_providers_are_registered;

public class and_a_collectible_assembly_has_been_walked : Specification
{
    const int _maximumCollections = 50;

    WeakReference _collectibleAssembly;
    bool _walkedWhileLoaded;
    bool _collected;

    void Establish() => (_collectibleAssembly, _walkedWhileLoaded) = EmitCollectibleAssemblyAndWalkIt();

    void Because()
    {
        for (var collection = 0; collection < _maximumCollections && _collectibleAssembly.IsAlive; collection++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        _collected = !_collectibleAssembly.IsAlive;
    }

    [Fact] void should_have_walked_the_collectible_assembly_while_it_was_loaded() => _walkedWhileLoaded.ShouldBeTrue();
    [Fact] void should_let_the_collectible_assembly_be_collected() => _collected.ShouldBeTrue();

    /// <summary>
    /// Emits a collectible assembly that declares no provider, so nothing but the registry's record of what it walked
    /// can root it, and runs a walk while it is loaded. Kept out of line so no local of the caller holds the assembly,
    /// the builder or the emitted type once it returns.
    /// </summary>
    /// <returns>A weak reference to the assembly the walk saw in the AppDomain, and whether the walk reached it.</returns>
    /// <remarks>
    /// Another spec class may walk first and process the emitted assembly before this call does, so reaching it is
    /// recorded for any walk that has it in the AppDomain once it is loaded, not just this call's.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    static (WeakReference Assembly, bool Walked) EmitCollectibleAssemblyAndWalkIt()
    {
        var simpleName = $"Cratis.Fundamentals.Specs.Collectible.{Guid.NewGuid():N}";
        var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(simpleName), AssemblyBuilderAccess.RunAndCollect);
        builder.DefineDynamicModule(simpleName).DefineType("CollectibleType", TypeAttributes.Public).CreateType();

        var loaded = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), _ => _.GetName().Name == simpleName);
        var walked = false;
        GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered(true, assembly =>
        {
            if (assembly.GetName().Name == simpleName)
            {
                walked = true;
            }

            return assembly.GetReferencedAssemblies();
        });

        // Nothing walks an assembly twice, so if an earlier walk got there first this one never sees it - and the
        // call returning without walking it is itself the proof that it was processed.
        return (new WeakReference(loaded), loaded is not null && (walked || !NeedsAWalk(simpleName)));
    }

    static bool NeedsAWalk(string simpleName)
    {
        var seen = false;
        GeneratedTypeDiscoveryRegistry.EnsureProvidersRegistered(true, assembly =>
        {
            seen |= assembly.GetName().Name == simpleName;
            return assembly.GetReferencedAssemblies();
        });
        return seen;
    }
}
