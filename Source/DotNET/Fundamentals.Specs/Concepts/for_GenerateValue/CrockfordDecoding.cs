// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Cratis.Concepts.for_GenerateValue;

internal static class CrockfordDecoding
{
    internal const string UpperAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    internal const string LowerAlphabet = "0123456789abcdefghjkmnpqrstvwxyz";

    internal static byte[] Decode(string value)
    {
        var number = value.ToUpperInvariant().Aggregate(BigInteger.Zero, (current, character) => (current * 32) + UpperAlphabet.IndexOf(character));
        var decoded = number.ToByteArray(isUnsigned: true, isBigEndian: true);
        var bytes = new byte[16];
        decoded.CopyTo(bytes, bytes.Length - decoded.Length);

        return bytes;
    }

    internal static long Timestamp(byte[] bytes) => bytes.Take(6).Aggregate(0L, (current, next) => (current << 8) | next);
}
