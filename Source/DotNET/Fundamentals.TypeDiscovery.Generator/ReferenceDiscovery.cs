// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// The equatable, symbol-free discovery result for one metadata reference.
/// </summary>
/// <param name="name">The simple assembly name used for ambiguity detection.</param>
/// <param name="identity">The referenced assembly identity.</param>
/// <param name="publicTypes">Public type names for collision detection.</param>
/// <param name="packageTypes">Runtime Cratis package type facts.</param>
internal sealed class ReferenceDiscovery(string name, string identity, EquatableArray<string> publicTypes, EquatableArray<DiscoveredType> packageTypes) : IEquatable<ReferenceDiscovery>
{
    /// <summary>Gets the simple assembly name.</summary>
    public string Name { get; } = name;

    /// <summary>Gets the assembly identity.</summary>
    public string Identity { get; } = identity;

    /// <summary>Gets public type expressions.</summary>
    public EquatableArray<string> PublicTypes { get; } = publicTypes;

    /// <summary>Gets runtime package type facts.</summary>
    public EquatableArray<DiscoveredType> PackageTypes { get; } = packageTypes;

    /// <summary>Analyzes one reference with its metadata dependencies.</summary>
    /// <param name="reference">The reference to analyze.</param>
    /// <param name="references">The reference universe, independent of source edits.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Equatable discovery facts, without compiler objects.</returns>
    public static ReferenceDiscovery Create(MetadataReference reference, ImmutableArray<MetadataReference> references, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Public names do not require dependency binding. Only Cratis convention facts do.
        var compilation = CSharpCompilation.Create("TypeDiscoveryReferences", references: [reference]);
        if (!HasGlobalAlias(reference) || compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
        {
            return new(string.Empty, string.Empty, default, default);
        }

        var isPackage = assembly.Name.StartsWith("Cratis", StringComparison.Ordinal) && IsRuntimeAsset(reference) &&
            !assembly.Modules.Any(static module => module.ReferencedAssemblies.Any(static identity => identity.Name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal)));
        if (isPackage)
        {
            compilation = CSharpCompilation.Create("TypeDiscoveryReferences", references: references);
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol runtimeAssembly)
            {
                return new(string.Empty, string.Empty, default, default);
            }

            assembly = runtimeAssembly;
        }

        var symbols = assembly.GlobalNamespace.GetAllNamedTypes().Where(static type => type.DeclaredAccessibility == Accessibility.Public).ToArray();
        var packageTypes = isPackage
            ? new EquatableArray<DiscoveredType>([.. symbols.Where(static type => !type.IsImplicitlyDeclared && !type.IsAnonymousType && !type.IsFileLocal && type.CanBeReferencedByName && !type.IsNestedInGenericType())
                .Select(type => new DiscoveredType(type, compilation.Assembly))])
            : default;
        return new(
            assembly.Name,
            assembly.Identity.ToString(),
            new([.. symbols.Select(static type => type.GetTypeOfExpression()).Distinct().OrderBy(static name => name, StringComparer.Ordinal)]),
            packageTypes);
    }

    /// <inheritdoc/>
    public bool Equals(ReferenceDiscovery? other) => other is not null && Identity == other.Identity &&
        PublicTypes.Equals(other.PublicTypes) && PackageTypes.Equals(other.PackageTypes);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReferenceDiscovery other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Identity.GetHashCode();

    static bool HasGlobalAlias(MetadataReference reference) => reference.Properties.Aliases.IsDefaultOrEmpty || reference.Properties.Aliases.Contains("global");

    static bool IsRuntimeAsset(MetadataReference reference)
    {
        var path = (reference as PortableExecutableReference)?.FilePath?.Replace('\\', '/');
        return path is null || (path.IndexOf("/tasks/", StringComparison.Ordinal) < 0 && path.IndexOf("/analyzers/", StringComparison.Ordinal) < 0);
    }
}
