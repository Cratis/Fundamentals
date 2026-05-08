// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Collects type discovery metadata from a set of named type symbols:
/// the contract-to-implementors map, DI convention service bindings, and self bindings.
/// </summary>
internal static class TypeDiscoveryCollector
{
    /// <summary>
    /// Builds the contract-to-implementors map for all concrete types in <paramref name="symbols"/>.
    /// Each entry maps a contract type expression to the expressions of all types that implement it.
    /// </summary>
    /// <param name="symbols">The set of named type symbols from the assembly.</param>
    /// <returns>One entry per contract with its ordered list of implementors.</returns>
    public static IEnumerable<(string ContractExpression, ImmutableArray<string> ImplementorExpressions)> GetContractsAndImplementors(
        IEnumerable<INamedTypeSymbol> symbols)
    {
        var implementors = symbols.Where(s => s.IsImplementation()).ToArray();
        var contractsAndImplementors = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var implementor in implementors)
        {
            var implementorExpression = implementor.GetTypeOfExpression();

            foreach (var contract in implementor.GetAllBaseAndImplementingSymbols())
            {
                var contractExpression = contract.GetTypeOfExpression();

                if (!contractsAndImplementors.TryGetValue(contractExpression, out var mapped))
                {
                    mapped = [];
                    contractsAndImplementors[contractExpression] = mapped;
                }

                mapped.Add(implementorExpression);
            }
        }

        return contractsAndImplementors.Select(kvp =>
            (kvp.Key, kvp.Value.OrderBy(static _ => _, StringComparer.Ordinal).ToImmutableArray()));
    }

    /// <summary>
    /// Returns all <c>I&lt;X&gt;</c> / <c>&lt;X&gt;</c> convention service bindings
    /// where exactly one implementation of the interface exists in the same namespace.
    /// </summary>
    /// <param name="symbols">The set of named type symbols from the assembly.</param>
    /// <returns>One entry per discovered convention binding.</returns>
    public static IEnumerable<(string ServiceExpression, string ImplementationExpression, string LifetimeExpression)> GetConventionServiceBindings(
        IEnumerable<INamedTypeSymbol> symbols)
    {
        var allTypes = symbols.ToArray();

        foreach (var implementation in allTypes.Where(s => s.IsImplementation()))
        {
            if (implementation.ShouldIgnoreConvention())
            {
                continue;
            }

            var conventionInterfaces = implementation.AllInterfaces
                .Where(i =>
                    i.ContainingNamespace.ToDisplayString() == implementation.ContainingNamespace.ToDisplayString() &&
                    i.MetadataName == $"I{implementation.MetadataName}")
                .ToArray();

            if (conventionInterfaces.Length != 1)
            {
                continue;
            }

            var conventionInterface = conventionInterfaces[0];
            var implementationCount = allTypes.Count(t => t.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, conventionInterface)));

            if (implementationCount != 1)
            {
                continue;
            }

            yield return (
                conventionInterface.GetTypeOfExpression(),
                implementation.GetTypeOfExpression(),
                implementation.GetServiceLifetimeExpression());
        }
    }

    /// <summary>
    /// Returns all self-binding types — concrete classes that should be registered in DI
    /// under their own type as the service type.
    /// Excludes interfaces, abstract classes, static classes, types in System/Microsoft namespaces,
    /// types with unresolvable constructor parameters, record-typed constructor parameters,
    /// exception types, and types marked with <c>[IgnoreConvention]</c>.
    /// </summary>
    /// <param name="symbols">The set of named type symbols from the assembly.</param>
    /// <returns>One entry per discovered self-binding type.</returns>
    public static IEnumerable<(string ImplementationExpression, string LifetimeExpression)> GetConventionSelfBindings(
        IEnumerable<INamedTypeSymbol> symbols)
    {
        foreach (var type in symbols)
        {
            if (type.IsStatic ||
                type.TypeKind == TypeKind.Interface ||
                type.IsAbstract ||
                type.ShouldIgnoreConvention() ||
                type.IsInSystemOrMicrosoftNamespace() ||
                type.HasConstructorWithUnresolvableParameters() ||
                type.HasConstructorWithRecordTypes() ||
                type.IsExceptionType())
            {
                continue;
            }

            yield return (type.GetTypeOfExpression(), type.GetServiceLifetimeExpression());
        }
    }
}
