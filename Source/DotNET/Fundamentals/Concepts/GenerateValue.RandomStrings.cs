// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace Cratis.Concepts;

public static partial class GenerateValue
{
    const string DefaultNanoIdAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";

    /// <summary>
    /// Creates a uniformly random string from a printable ASCII alphabet.
    /// </summary>
    /// <param name="size">The number of characters, from 1 to 256; defaults to 21.</param>
    /// <param name="alphabet">Two to 256 distinct printable ASCII characters, or null for A-Z, a-z, 0-9, underscore and hyphen.</param>
    /// <returns>A string of the requested size drawn uniformly using cryptographically secure randomness.</returns>
    /// <exception cref="InvalidNanoIdSize">The size is outside 1 to 256.</exception>
    /// <exception cref="InvalidNanoIdAlphabet">The alphabet is not distinct printable ASCII or its length is outside 2 to 256.</exception>
    public static string NanoId(int size = 21, string? alphabet = null)
    {
        if (size is < 1 or > 256)
        {
            throw new InvalidNanoIdSize(size);
        }

        alphabet ??= DefaultNanoIdAlphabet;
        if (alphabet.Length is < 2 or > 256 || alphabet.Any(character => character is < ' ' or > '~') || alphabet.Distinct().Count() != alphabet.Length)
        {
            throw new InvalidNanoIdAlphabet();
        }

        return new string(RandomNumberGenerator.GetItems<char>(alphabet.AsSpan(), size));
    }

    /// <summary>
    /// Creates cryptographically secure random bytes encoded as an unpadded string.
    /// </summary>
    /// <param name="count">The number of random bytes, from 1 to 512.</param>
    /// <param name="encoding">The text encoding; defaults to URL-safe base64 without padding.</param>
    /// <returns>The encoded random bytes: URL-safe base64, lowercase hex, or uppercase RFC 4648 base32.</returns>
    /// <exception cref="InvalidByteCount">The count is outside 1 to 512.</exception>
    /// <exception cref="UnknownValueEncoding">The encoding is undefined.</exception>
    public static string Bytes(int count, ValueEncoding encoding = ValueEncoding.Base64Url)
    {
        if (count is < 1 or > 512)
        {
            throw new InvalidByteCount(count);
        }

        if (encoding is not (ValueEncoding.Base64Url or ValueEncoding.Hex or ValueEncoding.Base32))
        {
            throw new UnknownValueEncoding(encoding);
        }

        var bytes = RandomNumberGenerator.GetBytes(count);

        return encoding switch
        {
            ValueEncoding.Base64Url => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_'),
            ValueEncoding.Hex => Convert.ToHexString(bytes).ToLowerInvariant(),
            ValueEncoding.Base32 => Base32Encoder.Encode(bytes, Base32Encoder.Rfc4648),
            _ => throw new UnknownValueEncoding(encoding)
        };
    }
}
