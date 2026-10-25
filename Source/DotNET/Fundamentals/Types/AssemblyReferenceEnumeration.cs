// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cratis.Types;

/// <summary>
/// Probes whether the runtime supports enumerating assembly references.
/// </summary>
/// <remarks>
/// NativeAOT refuses reference enumeration and runs every compiled module initializer before Main.
/// Probe the capability itself rather than dynamic code support, which can also be disabled under JIT.
/// </remarks>
internal static class AssemblyReferenceEnumeration
{
    static readonly Lazy<bool> _isSupported = new(Probe);

    /// <summary>
    /// Gets whether this process supports enumerating assembly references.
    /// </summary>
    internal static bool IsSupported => _isSupported.Value;

    /// <summary>
    /// Checks reference enumeration without retaining its results.
    /// </summary>
    /// <param name="enumerate">The runtime operation to probe.</param>
    /// <returns>Whether enumeration is supported.</returns>
    internal static bool Probe(Func<AssemblyName[]> enumerate)
    {
        try
        {
            _ = enumerate();
            return true;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Capability probe only: the reference list is discarded, and only PlatformNotSupportedException indicates unsupported enumeration.")]
    static bool Probe() => Probe(static () => typeof(AssemblyReferenceEnumeration).Assembly.GetReferencedAssemblies());
}
