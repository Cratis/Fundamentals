// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Small metadata-only reference facts used to select relevant dependency snapshots.
/// </summary>
/// <param name="reference">The immutable metadata reference.</param>
/// <param name="name">Its assembly name.</param>
/// <param name="dependencies">The assembly names referenced by its modules.</param>
internal sealed class ReferenceMetadata(MetadataReference reference, string name, EquatableArray<string> dependencies) : IEquatable<ReferenceMetadata>
{
    static readonly MetadataReferenceComparer _comparer = new();

    /// <summary>Gets the metadata reference.</summary>
    public MetadataReference Reference { get; } = reference;

    /// <summary>Gets the assembly name.</summary>
    public string Name { get; } = name;

    /// <summary>Gets the dependency assembly names.</summary>
    public EquatableArray<string> Dependencies { get; } = dependencies;

    /// <summary>Reads assembly headers without walking types or retaining compiler objects.</summary>
    /// <param name="reference">The reference to inspect.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The equatable reference facts.</returns>
    public static ReferenceMetadata Create(MetadataReference reference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (reference is CompilationReference compilationReference)
        {
            var assembly = compilationReference.Compilation.Assembly;
            return new(reference, assembly.Name, new([.. assembly.Modules.SelectMany(static module => module.ReferencedAssemblies)
                .Select(static identity => identity.Name).Distinct().OrderBy(static name => name, StringComparer.Ordinal)]));
        }

        if (reference is not PortableExecutableReference portable || portable.GetMetadata() is not AssemblyMetadata metadata)
        {
            return new(reference, string.Empty, default);
        }

        var readers = metadata.GetModules().Select(static module => module.GetMetadataReader()).ToArray();
        return new(
            reference,
            readers[0].GetString(readers[0].GetAssemblyDefinition().Name),
            new([.. readers.SelectMany(GetDependencies).Distinct().OrderBy(static name => name, StringComparer.Ordinal)]));
    }

    /// <inheritdoc/>
    public bool Equals(ReferenceMetadata? other) => other is not null && _comparer.Equals(Reference, other.Reference) &&
        Name == other.Name && Dependencies.Equals(other.Dependencies);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReferenceMetadata other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _comparer.GetHashCode(Reference);

    static IEnumerable<string> GetDependencies(MetadataReader reader) =>
        reader.AssemblyReferences.Select(handle => reader.GetString(reader.GetAssemblyReference(handle).Name));
}
