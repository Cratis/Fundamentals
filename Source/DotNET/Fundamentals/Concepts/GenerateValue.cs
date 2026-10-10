// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace Cratis.Concepts;

/// <summary>
/// Generates UUID values using cryptographically secure randomness.
/// </summary>
public static class GenerateValue
{
    /// <summary>
    /// Creates an RFC 9562 version 4 UUID using cryptographically secure randomness.
    /// </summary>
    /// <returns>A random version 4 UUID with the RFC variant.</returns>
    public static Guid Uuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);

        return new Guid(bytes, bigEndian: true);
    }

    /// <summary>
    /// Creates an RFC 9562 version 7 UUID containing the current Unix-epoch milliseconds.
    /// </summary>
    /// <returns>A time-ordered version 7 UUID with the RFC variant and cryptographically secure random bits.</returns>
    /// <remarks>
    /// Values expose their creation time and are not monotonic within a millisecond.
    /// Clock adjustments can also affect their chronological order.
    /// </remarks>
    public static Guid UuidV7()
    {
#if NET9_0_OR_GREATER
        return Guid.CreateVersion7();
#else
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        for (var index = 0; index < 6; index++)
        {
            bytes[index] = (byte)(timestamp >> (40 - (index * 8)));
        }

        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x70);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);

        return new Guid(bytes, bigEndian: true);
#endif
    }
}
