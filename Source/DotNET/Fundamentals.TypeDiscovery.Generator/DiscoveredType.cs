// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Symbol-free type facts retained by the incremental pipelines.
/// </summary>
/// <param name="symbol">The type being analyzed, used only during construction.</param>
/// <param name="assembly">The consuming assembly.</param>
internal sealed class DiscoveredType(INamedTypeSymbol symbol, IAssemblySymbol assembly) : IEquatable<DiscoveredType>
{
    /// <summary>Gets the emitted typeof expression.</summary>
    public string Expression { get; } = symbol.GetTypeOfExpression();

    /// <summary>Gets the service lifetime expression.</summary>
    public string Lifetime { get; } = symbol.GetServiceLifetimeExpression();

    /// <summary>Gets whether this is a concrete implementation.</summary>
    public bool IsImplementation { get; } = symbol.IsImplementation();

    /// <summary>Gets whether conventions exclude this type.</summary>
    public bool IgnoreConvention { get; } = symbol.ShouldIgnoreConvention();

    /// <summary>Gets whether this type is self-bound.</summary>
    public bool IsSelfBinding { get; } = TypeDiscoveryCollector.GetConventionSelfBindings([symbol]).Any();

    /// <summary>Gets whether public constructors can be preserved.</summary>
    public bool PreserveConstructors { get; } = ConstructorPreservation.GetImplementationTypes([symbol], [symbol.GetTypeOfExpression()]).Any();

    /// <summary>Gets accessible contracts and their defining assembly identities.</summary>
    public EquatableArray<(string Expression, string Assembly)> Contracts { get; } = new([.. symbol
        .GetAllBaseAndImplementingSymbols(assembly)
        .Select(contract => (contract.GetTypeOfExpression(), ExternalIdentity(contract, assembly)))]);

    /// <summary>Gets interface identities and convention-matching facts.</summary>
    public EquatableArray<(string Key, string Expression, string Assembly, bool Accessible, bool Matches)> Interfaces { get; } = new([.. symbol.AllInterfaces
        .Select(contract => (
            $"{contract.ContainingAssembly.Identity}:{contract.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}",
            contract.GetTypeOfExpression(),
            ExternalIdentity(contract, assembly),
            contract.IsAccessibleFromAssembly(assembly),
            contract.ContainingNamespace.ToDisplayString() == symbol.ContainingNamespace.ToDisplayString() &&
            contract.MetadataName == $"I{symbol.MetadataName}"))]);

    /// <summary>Gets experimental diagnostic identifiers.</summary>
    public EquatableArray<string> ExperimentalIds { get; } = new(symbol.GetExperimentalDiagnosticIds());

    /// <inheritdoc/>
    public bool Equals(DiscoveredType? other) => other is not null &&
        (Expression, Lifetime, IsImplementation, IgnoreConvention, IsSelfBinding, PreserveConstructors).Equals(
            (other.Expression, other.Lifetime, other.IsImplementation, other.IgnoreConvention, other.IsSelfBinding, other.PreserveConstructors)) &&
        Contracts.Equals(other.Contracts) && Interfaces.Equals(other.Interfaces) && ExperimentalIds.Equals(other.ExperimentalIds);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DiscoveredType other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Expression.GetHashCode();

    static string ExternalIdentity(INamedTypeSymbol symbol, IAssemblySymbol assembly) =>
        SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, assembly) ? string.Empty : symbol.ContainingAssembly.Identity.ToString();
}
