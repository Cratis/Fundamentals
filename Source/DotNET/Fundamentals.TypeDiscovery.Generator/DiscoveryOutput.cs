// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Combines cached discovery facts without retaining a compilation or symbols.
/// </summary>
internal static class DiscoveryOutput
{
    /// <summary>Builds the provider from source and reference facts.</summary>
    /// <param name="currentTypes">Current assembly types.</param>
    /// <param name="references">Referenced assembly facts.</param>
    /// <returns>The generated source.</returns>
    public static string Build(IEnumerable<DiscoveredType> currentTypes, IEnumerable<ReferenceDiscovery> references)
    {
        var referenceArray = references.ToArray();
        var identities = new HashSet<string>(referenceArray.Select(static reference => reference.Identity), StringComparer.Ordinal);
        var referencedNames = new HashSet<string>(referenceArray.SelectMany(static reference => reference.PublicTypes), StringComparer.Ordinal);
        var ambiguousNames = new HashSet<string>(
            referenceArray.SelectMany(reference => reference.PublicTypes.Select(name => (Expression: name, Assembly: reference.Name)))
            .GroupBy(static type => type.Expression).Where(static group => group.Select(static type => type.Assembly).Distinct().Count() > 1)
            .Select(static group => group.Key),
            StringComparer.Ordinal);
        var types = currentTypes.Where(type => !referencedNames.Contains(type.Expression))
            .GroupBy(static type => type.Expression).Select(static group => group.First()).ToArray();
        var packageTypes = referenceArray.SelectMany(static reference => reference.PackageTypes).Where(type => !ambiguousNames.Contains(type.Expression)).ToArray();
        var symbols = types.Concat(packageTypes).ToArray();
        var contracts = GetContracts(types, identities, ambiguousNames);
        var bindings = GetBindings(types, identities).Concat(GetBindings(packageTypes))
            .OrderBy(static binding => binding.ServiceExpression, StringComparer.Ordinal).ThenBy(static binding => binding.ImplementationExpression, StringComparer.Ordinal).ToImmutableArray();
        var selfBindings = symbols.Where(static type => type.IsSelfBinding).Select(static type => (ImplementationExpression: type.Expression, LifetimeExpression: type.Lifetime))
            .OrderBy(static binding => binding.ImplementationExpression, StringComparer.Ordinal).ToImmutableArray();
        var implementations = new HashSet<string>(bindings.Select(static binding => binding.ImplementationExpression).Concat(selfBindings.Select(static binding => binding.ImplementationExpression)), StringComparer.Ordinal);
        var preserved = symbols.Where(type => implementations.Contains(type.Expression) && type.PreserveConstructors)
            .Select(static type => type.Expression).Distinct().OrderBy(static name => name, StringComparer.Ordinal);
        var experimentalIds = symbols.SelectMany(static type => type.ExperimentalIds).Distinct().OrderBy(static id => id, StringComparer.Ordinal).ToImmutableArray();
        return GeneratedSourceBuilder.BuildFromNames(
            [.. types.Select(static type => type.Expression).OrderBy(static name => name, StringComparer.Ordinal)],
            contracts,
            bindings,
            selfBindings,
            preserved,
            experimentalIds);
    }

    static ImmutableArray<(string ContractExpression, ImmutableArray<string> ImplementorExpressions)> GetContracts(
        DiscoveredType[] types, HashSet<string> identities, HashSet<string> ambiguousNames) =>
        [.. types.Where(static type => type.IsImplementation)
            .SelectMany(type => type.Contracts.Where(contract => IsAccessible(contract.Assembly, identities) && !ambiguousNames.Contains(contract.Expression))
                .Select(contract => (Contract: contract.Expression, Implementation: type.Expression)))
            .GroupBy(static entry => entry.Contract)
            .OrderBy(static group => group.Key, StringComparer.Ordinal)
            .Select(static group => (group.Key, group.Select(static entry => entry.Implementation).Distinct().OrderBy(static name => name, StringComparer.Ordinal).ToImmutableArray()))];

    static IEnumerable<(string ServiceExpression, string ImplementationExpression, string LifetimeExpression)> GetBindings(DiscoveredType[] symbols, HashSet<string>? identities = null)
    {
        var eligible = symbols.Where(static type => !type.IgnoreConvention).ToArray();
        var counts = eligible.SelectMany(static type => type.Interfaces).GroupBy(static contract => contract.Key)
            .ToDictionary(static group => group.Key, static group => group.Count());
        foreach (var type in eligible.Where(static type => type.IsImplementation))
        {
            var matches = type.Interfaces.Where(contract => contract.Matches && (identities is null || (contract.Accessible && IsAccessible(contract.Assembly, identities)))).ToArray();
            if (matches.Length == 1 && counts[matches[0].Key] == 1)
            {
                yield return (matches[0].Expression, type.Expression, type.Lifetime);
            }
        }
    }

    static bool IsAccessible(string assembly, HashSet<string> identities) => assembly.Length == 0 || identities.Contains(assembly);
}
