// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Cratis.Concepts;

/// <summary>
/// Holds completion counts written during construction and published only after the graph is complete.
/// Sampling never mutates this graph.
/// </summary>
/// <param name="accepts">Whether the empty completion is admissible at this node.</param>
internal sealed class GenerationCompletion(bool accepts)
{
    /// <summary>
    /// Gets whether this node accepts the empty completion.
    /// </summary>
    internal bool Accepts { get; } = accepts;

    /// <summary>
    /// Gets or sets the exact number of distinct accepted completions.
    /// </summary>
    internal BigInteger Count { get; set; } = accepts ? BigInteger.One : BigInteger.Zero;

    /// <summary>
    /// Gets the character branches in ordinal order, populated before publication.
    /// </summary>
    internal List<(char Character, GenerationCompletion Target)> Branches { get; } = [];
}
