// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Cratis.Concepts;

public static partial class GenerateValue
{
    /// <summary>
    /// Creates an unbiased integer in an inclusive Int64 range using cryptographically secure randomness.
    /// </summary>
    /// <param name="minimum">The inclusive lower bound.</param>
    /// <param name="maximum">The inclusive upper bound.</param>
    /// <returns>A uniformly distributed integer between the bounds, including both endpoints.</returns>
    /// <exception cref="InvalidIntegerRange">The minimum exceeds the maximum.</exception>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Integer is the agreed name in the shared generation contract used by Screenplay and Stage.")]
    public static long Integer(long minimum, long maximum)
    {
        if (minimum > maximum)
        {
            throw new InvalidIntegerRange(minimum, maximum);
        }

        var span = unchecked((ulong)maximum - (ulong)minimum + 1);

        // A zero span represents all 2^64 possible bit patterns.
        var threshold = span == 0 ? 0 : unchecked(0UL - span) % span;
        Span<byte> bytes = stackalloc byte[8];
        ulong random;
        do
        {
            RandomNumberGenerator.Fill(bytes);
            random = BinaryPrimitives.ReadUInt64LittleEndian(bytes);
        }
        while (random < threshold);

        return unchecked((long)((ulong)minimum + (span == 0 ? random : random % span)));
    }
}
