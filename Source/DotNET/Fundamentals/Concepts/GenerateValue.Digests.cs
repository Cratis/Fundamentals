// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace Cratis.Concepts;

public static partial class GenerateValue
{
    static readonly UTF8Encoding _sha256Encoding = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// Computes the SHA-256 digest (FIPS 180-4) of the UTF-8 text deterministically, as 64 lowercase hexadecimal characters.
    /// </summary>
    /// <param name="value">The well-formed UTF-16 text to encode as UTF-8.</param>
    /// <returns>The deterministic digest as 64 lowercase hexadecimal characters.</returns>
    /// <remarks>
    /// Text is encoded as UTF-8 without BOM insertion, normalization or case folding; callers canonicalize first if needed.
    /// A literal leading U+FEFF is preserved. This is a fast, unkeyed hash, suitable as a fingerprint or a stored verifier
    /// for server-generated tokens with at least 128 bits of entropy. Not for passwords or other low-entropy input;
    /// use a password hashing function for those, with a salt and a slow algorithm, including for PINs and short codes.
    /// Anyone who knows the input can recompute the digest. A digest of personal data is still personal data.
    /// The temporary UTF-8 buffer is cleared, but strings cannot be erased.
    /// Compare digests directly with <see cref="CryptographicOperations.FixedTimeEquals(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/> over equal-length bytes.
    /// Lookup by digest index is not constant-time and is acceptable only for tokens with at least 128 bits of entropy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    /// <exception cref="InvalidSha256Input">The value contains an unpaired surrogate.</exception>
    public static string Sha256(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        int byteCount;
        try
        {
            byteCount = _sha256Encoding.GetByteCount(value);
        }
        catch (EncoderFallbackException)
        {
            throw new InvalidSha256Input();
        }

        var rented = byteCount > 256 ? ArrayPool<byte>.Shared.Rent(byteCount) : null;
        var buffer = rented is null ? stackalloc byte[byteCount] : rented.AsSpan(0, byteCount);
        try
        {
            _sha256Encoding.GetBytes(value.AsSpan(), buffer);
            Span<byte> digest = stackalloc byte[32];
            SHA256.HashData(buffer, digest);

            return Convert.ToHexString(digest).ToLowerInvariant();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(buffer);
            if (rented is not null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }
}
