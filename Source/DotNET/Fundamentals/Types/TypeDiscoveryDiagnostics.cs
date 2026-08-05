// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Cratis.Types;

/// <summary>
/// Answers questions about what type discovery did and did not reach.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately something a host asks rather than something start-up does. Comparing what was
/// discovered against what could have been means consulting the dependency model, and doing that on
/// every start-up would spend part of the cost the generated providers exist to avoid, to catch a case
/// most applications never hit. Asked instead of assumed, the cost lands only on the caller that wants
/// the answer - a health check, a start-up self-test, a spec - and there is no budget to argue about.
/// </para>
/// </remarks>
public static class TypeDiscoveryDiagnostics
{
    /// <summary>
    /// Finds project-referenced libraries that contributed nothing to a type universe.
    /// </summary>
    /// <param name="types">The <see cref="ITypes"/> to check.</param>
    /// <returns>The names of the libraries that contributed nothing, empty when every one of them did.</returns>
    /// <remarks>
    /// <para>
    /// A generated provider registers itself from a module initializer, which runs when the CLR loads
    /// its assembly. An assembly that has not been loaded by the time the universe is built therefore
    /// contributes nothing to it, and says nothing about having done so - the types it owns are simply
    /// absent. Every name returned here is an assembly the application references and the universe never
    /// saw.
    /// </para>
    /// <para>
    /// This reads names from the dependency model and does not load anything, so a name can also come
    /// back for a library that legitimately contributes no types at all.
    /// </para>
    /// </remarks>
    [UnconditionalSuppressMessage("SingleFile", "IL3002", Justification = "Diagnostic API, called on demand by a host that accepts the cost.")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Diagnostic API, called on demand by a host that accepts the cost.")]
    public static IEnumerable<string> FindMissingContributors(ITypes types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var entryAssembly = Assembly.GetEntryAssembly();
        var dependencyModel = entryAssembly is null ? null : DependencyContext.Load(entryAssembly);
        if (dependencyModel is null)
        {
            return [];
        }

        var contributed = types.Assemblies
            .Select(_ => _.GetName().Name)
            .Where(_ => _ is not null)
            .ToHashSet(StringComparer.Ordinal);

        return
        [
            .. dependencyModel.RuntimeLibraries
                .Where(_ => _.Type.Equals("project", StringComparison.Ordinal))
                .Select(_ => _.Name)
                .Where(_ => !contributed.Contains(_))
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
        ];
    }
}
