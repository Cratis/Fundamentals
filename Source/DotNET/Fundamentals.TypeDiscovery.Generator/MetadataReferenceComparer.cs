// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace Cratis.Fundamentals.TypeDiscovery.Generator;

/// <summary>
/// Compares immutable PE snapshots by module version, retaining alias and asset-path semantics.
/// Compilation references deliberately retain Roslyn's reference equality.
/// </summary>
internal sealed class MetadataReferenceComparer : IEqualityComparer<MetadataReference>
{
    /// <inheritdoc/>
    public bool Equals(MetadataReference? x, MetadataReference? y) => ReferenceEquals(x, y) ||
        (x is PortableExecutableReference left && y is PortableExecutableReference right &&
        left.Properties.Equals(right.Properties) && left.FilePath == right.FilePath &&
        ModuleVersions(left).SequenceEqual(ModuleVersions(right)));

    /// <inheritdoc/>
    public int GetHashCode(MetadataReference obj) => obj is PortableExecutableReference reference
        ? ModuleVersions(reference).FirstOrDefault().GetHashCode()
        : obj.GetHashCode();

    static IEnumerable<Guid> ModuleVersions(PortableExecutableReference reference) => reference.GetMetadata() switch
    {
        AssemblyMetadata assembly => assembly.GetModules().Select(static module => module.GetModuleVersionId()),
        ModuleMetadata module => [module.GetModuleVersionId()],
        _ => []
    };
}
