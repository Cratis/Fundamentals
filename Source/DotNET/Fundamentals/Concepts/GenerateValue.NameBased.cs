// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using System.Text;

namespace Cratis.Concepts;

public static partial class GenerateValue
{
    static readonly UTF8Encoding _uuidV5Encoding = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// Derives an RFC 9562 version 5 UUID deterministically, without randomness.
    /// The same namespace and name always produce the same UUID.
    /// </summary>
    /// <param name="namespace">The namespace that scopes the name.</param>
    /// <param name="name">The well-formed UTF-16 name to encode as UTF-8.</param>
    /// <returns>A deterministic version 5 UUID with the RFC variant.</returns>
    /// <remarks>
    /// Names are encoded as UTF-8 without normalization or case folding; callers canonicalize first if needed.
    /// Anyone who knows the namespace and name can recompute the value, so it is not a secret or an unguessable identifier.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The name is null.</exception>
    /// <exception cref="InvalidUuidV5Name">The name contains an unpaired surrogate.</exception>
    public static Guid UuidV5(Guid @namespace, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        int byteCount;
        try
        {
            byteCount = _uuidV5Encoding.GetByteCount(name);
        }
        catch (EncoderFallbackException)
        {
            throw new InvalidUuidV5Name();
        }

        var length = 16 + byteCount;
        var buffer = length <= 256 ? stackalloc byte[length] : new Span<byte>(new byte[length]);
        @namespace.TryWriteBytes(buffer, bigEndian: true, out _);
        _uuidV5Encoding.GetBytes(name.AsSpan(), buffer[16..]);

        Span<byte> hash = stackalloc byte[20];
#pragma warning disable CA5350 // RFC 9562 version 5 mandates SHA-1; the result is an identifier, not a security boundary
        SHA1.HashData(buffer, hash);
#pragma warning restore CA5350
        hash[6] = (byte)((hash[6] & 0x0f) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3f) | 0x80);

        return new Guid(hash[..16], bigEndian: true);
    }
}
