// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace Cratis.Concepts;

public static partial class GenerateValue
{
    /// <summary>
    /// Creates a ULID containing the current Unix-epoch milliseconds and 80 random bits.
    /// </summary>
    /// <returns>26 uppercase Crockford base32 characters, with a first character from 0 to 7.</returns>
    /// <remarks>Values expose creation time and are not monotonic within a millisecond.</remarks>
    public static string Ulid() => Ulid(new GenerationContext(DateTimeOffset.UtcNow, GenerationContext.DefaultTenant));

    /// <summary>
    /// Creates a ULID containing the operation's Unix-epoch milliseconds and 80 random bits.
    /// </summary>
    /// <param name="context">The operation's captured time and tenant.</param>
    /// <returns>26 uppercase Crockford base32 characters, with a first character from 0 to 7.</returns>
    /// <exception cref="MissingGenerationTime">The context has no captured time.</exception>
    /// <exception cref="GenerationTimeBeforeUnixEpoch">The captured time precedes the Unix epoch.</exception>
    public static string Ulid(GenerationContext context)
    {
        ValidateGenerationTime(context);
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes[6..]);
        WriteTimestamp(bytes, context.Now.ToUnixTimeMilliseconds());

        return Base32Encoder.Encode(bytes, Base32Encoder.CrockfordUpper, leadingPadding: true);
    }

    /// <summary>
    /// Creates a TypeID with a validated prefix and a UUID v7 suffix using the current time.
    /// </summary>
    /// <param name="prefix">Zero to 63 lowercase letters or underscores, starting and ending with a letter unless empty.</param>
    /// <returns>The prefix and underscore followed by 26 lowercase Crockford base32 characters, or only the suffix for an empty prefix.</returns>
    /// <exception cref="InvalidTypeIdPrefix">The prefix does not match the TypeID prefix rules.</exception>
    public static string TypeId(string prefix) => TypeId(prefix, new GenerationContext(DateTimeOffset.UtcNow, GenerationContext.DefaultTenant));

    /// <summary>
    /// Creates a TypeID with a validated prefix and a UUID v7 suffix using the operation's time.
    /// </summary>
    /// <param name="prefix">Zero to 63 lowercase letters or underscores, starting and ending with a letter unless empty.</param>
    /// <param name="context">The operation's captured time and tenant.</param>
    /// <returns>The prefix and underscore followed by 26 lowercase Crockford base32 characters, or only the suffix for an empty prefix.</returns>
    /// <exception cref="InvalidTypeIdPrefix">The prefix does not match the TypeID prefix rules.</exception>
    /// <exception cref="MissingGenerationTime">The context has no captured time.</exception>
    /// <exception cref="GenerationTimeBeforeUnixEpoch">The captured time precedes the Unix epoch.</exception>
    public static string TypeId(string prefix, GenerationContext context)
    {
        ValidateTypeIdPrefix(prefix);
        var bytes = UuidV7(context).ToByteArray(bigEndian: true);
        var suffix = Base32Encoder.Encode(bytes, Base32Encoder.CrockfordLower, leadingPadding: true);

        return prefix.Length == 0 ? suffix : $"{prefix}_{suffix}";
    }

    static void ValidateTypeIdPrefix(string prefix)
    {
        if (prefix is null || prefix.Length > 63)
        {
            throw new InvalidTypeIdPrefix(prefix);
        }

        if (prefix.Length == 0)
        {
            return;
        }

        if (prefix[0] is < 'a' or > 'z' || prefix[^1] is < 'a' or > 'z' || prefix.Any(character => character is not (>= 'a' and <= 'z') and not '_'))
        {
            throw new InvalidTypeIdPrefix(prefix);
        }
    }
}
