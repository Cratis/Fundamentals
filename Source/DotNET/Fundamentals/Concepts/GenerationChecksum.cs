// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Concepts;

/// <summary>
/// Calculates and validates the shared generation-contract checksum algorithms.
/// </summary>
internal static class GenerationChecksum
{
    /// <summary>
    /// Determines the number of appended digits for an algorithm.
    /// </summary>
    /// <param name="algorithm">The algorithm, including None.</param>
    /// <returns>Zero, one or two characters.</returns>
    /// <exception cref="UnknownChecksum">The algorithm is undefined.</exception>
    internal static int Width(Checksum algorithm) => algorithm switch
    {
        Checksum.None => 0,
        Checksum.Luhn or Checksum.Mod11 => 1,
        Checksum.Mod97 => 2,
        _ => throw new UnknownChecksum(algorithm)
    };

    /// <summary>
    /// Calculates check digits over all ASCII digits in a printable payload.
    /// </summary>
    /// <param name="payload">The payload without its check digits.</param>
    /// <param name="algorithm">The algorithm, including None.</param>
    /// <returns>The suffix, an empty string for None, or null for an excluded Mod11 payload.</returns>
    /// <exception cref="UnknownChecksum">The algorithm is undefined.</exception>
    internal static string? Calculate(string payload, Checksum algorithm)
    {
        _ = Width(algorithm);
        var digits = payload.Where(character => character is >= '0' and <= '9').Select(character => character - '0').ToArray();
        var value = algorithm switch
        {
            Checksum.Luhn => Luhn(digits),
            Checksum.Mod11 => Mod11(digits),
            Checksum.Mod97 => Mod97(digits),
            _ => 0
        };

        return algorithm switch
        {
            Checksum.None => string.Empty,
            Checksum.Mod11 when value == 10 => null,
            _ => value.ToString(algorithm == Checksum.Mod97 ? "D2" : "D1", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Validates exact trailing check digits and printable ASCII payload characters.
    /// </summary>
    /// <param name="value">The payload and its check digits.</param>
    /// <param name="algorithm">The checksum algorithm.</param>
    /// <returns>Whether the value has the expected suffix.</returns>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    /// <exception cref="UnknownChecksum">The algorithm is None or undefined.</exception>
    internal static bool Matches(string value, Checksum algorithm)
    {
        ArgumentNullException.ThrowIfNull(value);
        var width = Width(algorithm);
        if (width == 0)
        {
            throw new UnknownChecksum(algorithm);
        }

        if (value.Length < width || !value.All(character => character is >= ' ' and <= '~'))
        {
            return false;
        }

        var suffix = value[^width..];
        if (!suffix.All(character => character is >= '0' and <= '9'))
        {
            return false;
        }

        return string.Equals(suffix, Calculate(value[..^width], algorithm), StringComparison.Ordinal);
    }

    static int Luhn(int[] digits)
    {
        var sum = 0;
        for (var index = 0; index < digits.Length; index++)
        {
            var digit = digits[digits.Length - index - 1] * (index % 2 == 0 ? 2 : 1);
            sum = (sum + (digit > 9 ? digit - 9 : digit)) % 10;
        }

        return (10 - (sum % 10)) % 10;
    }

    static int Mod11(int[] digits)
    {
        var sum = 0;
        for (var index = 0; index < digits.Length; index++)
        {
            sum = (sum + (digits[digits.Length - index - 1] * (2 + (index % 6)))) % 11;
        }

        return (11 - (sum % 11)) % 11;
    }

    static int Mod97(int[] digits)
    {
        var residue = 0;
        foreach (var digit in digits)
        {
            residue = ((residue * 10) + digit) % 97;
        }

        var shifted = residue * 100;
        var remainder = shifted % 97;

        return 98 - remainder;
    }
}
