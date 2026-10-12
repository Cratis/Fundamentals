// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Encodes bytes using a base32 alphabet and zero padding bits, without padding characters.
/// </summary>
internal static class Base32Encoder
{
    /// <summary>
    /// The uppercase Crockford alphabet used by ULIDs.
    /// </summary>
    internal const string CrockfordUpper = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    /// <summary>
    /// The lowercase Crockford alphabet used by TypeIDs.
    /// </summary>
    internal const string CrockfordLower = "0123456789abcdefghjkmnpqrstvwxyz";

    /// <summary>
    /// The RFC 4648 alphabet used by encoded random bytes.
    /// </summary>
    internal const string Rfc4648 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// Encodes bytes with zero padding bits at the chosen end of the bit stream.
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <param name="alphabet">The 32-character alphabet.</param>
    /// <param name="leadingPadding">Whether to pad at the beginning instead of the end, as required for 128-bit Crockford identifiers.</param>
    /// <returns>The encoded bytes without padding characters.</returns>
    internal static string Encode(ReadOnlySpan<byte> bytes, string alphabet, bool leadingPadding = false)
    {
        var bitCount = bytes.Length * 8;
        var padding = leadingPadding ? (5 - (bitCount % 5)) % 5 : 0;
        var characters = new char[(bitCount + 4) / 5];
        for (var index = 0; index < characters.Length; index++)
        {
            var value = 0;
            for (var bit = 0; bit < 5; bit++)
            {
                var sourceBit = (index * 5) + bit - padding;
                value <<= 1;
                if (sourceBit >= 0 && sourceBit < bitCount)
                {
                    value |= (bytes[sourceBit / 8] >> (7 - (sourceBit % 8))) & 1;
                }
            }

            characters[index] = alphabet[value];
        }

        return new string(characters);
    }
}
