// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// An equatable per-reference analysis input, excluding unrelated metadata snapshots.
/// </summary>
/// <param name="metadata">The reference's own metadata facts.</param>
/// <param name="references">Its transitive metadata dependency closure, in original reference order.</param>
internal sealed class ReferenceInput(ReferenceMetadata metadata, ImmutableArray<MetadataReference> references) : IEquatable<ReferenceInput>
{
    static readonly MetadataReferenceComparer _comparer = new();

    /// <summary>Gets the reference being analyzed.</summary>
    public MetadataReference Reference { get; } = metadata.Reference;

    /// <summary>Gets only the metadata snapshots needed to bind this reference.</summary>
    public ImmutableArray<MetadataReference> References { get; } = references;

    /// <summary>Selects stable per-reference inputs from small header facts.</summary>
    /// <param name="metadata">All metadata reference headers.</param>
    /// <returns>One independently equatable input per reference.</returns>
    public static ImmutableArray<ReferenceInput> CreateAll(ImmutableArray<ReferenceMetadata> metadata)
    {
        var byName = metadata.ToLookup(static item => item.Name, StringComparer.Ordinal);
        return [.. metadata.Select(item => Create(item, metadata, byName))];
    }

    /// <inheritdoc/>
    public bool Equals(ReferenceInput? other) => other is not null && _comparer.Equals(Reference, other.Reference) &&
        References.SequenceEqual(other.References, _comparer);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReferenceInput other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _comparer.GetHashCode(Reference);

    static ReferenceInput Create(ReferenceMetadata root, ImmutableArray<ReferenceMetadata> metadata, ILookup<string, ReferenceMetadata> byName)
    {
        var names = new HashSet<string>(StringComparer.Ordinal) { root.Name };
        var pending = new Queue<ReferenceMetadata>(byName[root.Name]);
        while (pending.Count > 0)
        {
            foreach (var name in pending.Dequeue().Dependencies.Where(names.Add))
            {
                foreach (var dependency in byName[name])
                {
                    pending.Enqueue(dependency);
                }
            }
        }

        // Simple names intentionally retain all version/alias candidates for Roslyn's binding rules.
        return new(root, [.. metadata.Where(item => names.Contains(item.Name)).Select(static item => item.Reference)]);
    }
}
