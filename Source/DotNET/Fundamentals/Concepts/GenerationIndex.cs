// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Security.Cryptography;

namespace Cratis.Concepts;

/// <summary>
/// Draws unbiased indices using cryptographically secure random bits.
/// </summary>
internal static class GenerationIndex
{
    /// <summary>
    /// Draws an index uniformly below a positive support count.
    /// </summary>
    /// <param name="count">The positive number of admissible values.</param>
    /// <returns>An index in the half-open range from zero to count.</returns>
    internal static BigInteger Draw(BigInteger count)
    {
        if (count.IsOne)
        {
            return BigInteger.Zero;
        }

        var bits = (count - 1).GetBitLength();
        var bytes = new byte[(int)((bits + 7) / 8)];
        var mask = (byte)(0xff >> (int)((8 - (bits % 8)) % 8));
        BigInteger index;
        do
        {
            RandomNumberGenerator.Fill(bytes);
            bytes[^1] &= mask;
            index = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        }
        while (index >= count);

        return index;
    }
}
