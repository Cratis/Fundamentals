// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Represents a validated finite generation pattern before date substitution.
/// </summary>
/// <param name="Text">The original pattern used in diagnostics.</param>
/// <param name="Tokens">The parsed token sequence.</param>
/// <param name="Checksum">The appended check-digit algorithm.</param>
internal sealed record GenerationPattern(string Text, IReadOnlyList<GenerationPatternToken> Tokens, Checksum Checksum)
{
    /// <summary>
    /// The maximum emitted length, including check digits.
    /// </summary>
    internal const int LengthLimit = 256;

    /// <summary>
    /// Gets the width of the appended check digits.
    /// </summary>
    internal int ChecksumWidth => GenerationChecksum.Width(Checksum);

    /// <summary>
    /// Gets the minimum emitted length, including check digits.
    /// </summary>
    internal int MinimumLength => Tokens.Sum(token => token.Width * token.Minimum) + ChecksumWidth;

    /// <summary>
    /// Gets the maximum emitted length, including check digits.
    /// </summary>
    internal int MaximumLength => Tokens.Sum(token => token.Width * token.Maximum) + ChecksumWidth;

    /// <summary>
    /// Gets whether the pattern contains date segments.
    /// </summary>
    internal bool HasDates => Tokens.Any(token => token.Segment is not null);

    /// <summary>
    /// Parses the finite pattern language and checks its maximum emitted length.
    /// </summary>
    /// <param name="pattern">The authored pattern.</param>
    /// <param name="checksum">The appended check-digit algorithm.</param>
    /// <returns>The validated pattern contract.</returns>
    /// <exception cref="InvalidValuePattern">The pattern is null, malformed or too long.</exception>
    /// <exception cref="UnknownChecksum">The algorithm is undefined.</exception>
    internal static GenerationPattern Parse(string pattern, Checksum checksum = Checksum.None)
    {
        _ = GenerationChecksum.Width(checksum);
        if (pattern is null)
        {
            throw new InvalidValuePattern(pattern, "empty pattern");
        }

        var result = new GenerationPattern(pattern, new GenerationPatternParser(pattern).Parse(), checksum);
        if (result.MaximumLength > LengthLimit)
        {
            throw new InvalidValuePattern(pattern, "final expanded length exceeds 256 characters including check digits");
        }

        return result;
    }
}
