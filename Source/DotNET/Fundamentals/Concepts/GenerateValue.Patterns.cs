// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

public static partial class GenerateValue
{
    static readonly GenerationCache<(string Pattern, Checksum Checksum), GenerationPattern> _patterns = new();
    static readonly GenerationCache<(string Pattern, Checksum Checksum, DateOnly? Date), GenerationPatternSupport> _supports = new();

    /// <summary>
    /// Creates a uniformly selected distinct string from a finite printable-ASCII pattern.
    /// </summary>
    /// <param name="pattern">The pattern containing at least one random token.</param>
    /// <param name="checksum">Optional check digits appended over all payload digits.</param>
    /// <returns>A uniformly sampled admissible value, including any check digits.</returns>
    /// <exception cref="InvalidValuePattern">The pattern is invalid or exceeds a construction budget.</exception>
    /// <exception cref="PatternRequiresGenerationContext">The pattern contains date segments.</exception>
    /// <exception cref="EmptyPatternSupport">No payload has an emittable check digit.</exception>
    /// <exception cref="UnknownChecksum">The checksum is undefined.</exception>
    public static string Pattern(string pattern, Checksum checksum = Checksum.None) => SamplePattern(pattern, null, checksum);

    /// <summary>
    /// Creates a uniformly selected distinct string with date segments fixed to the operation's UTC date.
    /// </summary>
    /// <param name="pattern">The pattern containing at least one random token.</param>
    /// <param name="context">The operation's captured time and tenant; the tenant does not affect this strategy.</param>
    /// <param name="checksum">Optional check digits appended over all payload digits.</param>
    /// <returns>A uniformly sampled admissible value, including any check digits.</returns>
    /// <exception cref="MissingGenerationTime">The context has no captured time, even for a pattern without dates.</exception>
    /// <exception cref="InvalidValuePattern">The pattern is invalid or exceeds a construction budget.</exception>
    /// <exception cref="EmptyPatternSupport">No payload has an emittable check digit for the captured date.</exception>
    /// <exception cref="UnknownChecksum">The checksum is undefined.</exception>
    public static string Pattern(string pattern, GenerationContext context, Checksum checksum = Checksum.None)
    {
        if (context.Now == default)
        {
            throw new MissingGenerationTime();
        }

        return SamplePattern(pattern, DateOnly.FromDateTime(context.Now.UtcDateTime), checksum);
    }

    /// <summary>
    /// Checks exact trailing check digits against all preceding ASCII digits, ignoring printable nondigits.
    /// </summary>
    /// <param name="value">The complete payload and trailing check digits.</param>
    /// <param name="checksum">The checksum algorithm; None is not valid for validation.</param>
    /// <returns>Whether the printable-ASCII value has exactly the expected check digits.</returns>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    /// <exception cref="UnknownChecksum">The checksum is None or undefined.</exception>
    public static bool HasValidChecksum(string value, Checksum checksum) => GenerationChecksum.Matches(value, checksum);

    static string SamplePattern(string pattern, DateOnly? date, Checksum checksum)
    {
        var parsed = _patterns.Get((pattern, checksum), key => GenerationPattern.Parse(key.Pattern, key.Checksum));
        if (parsed.HasDates && date is null)
        {
            throw new PatternRequiresGenerationContext(pattern);
        }

        // A context must not create duplicate support entries for date-free patterns.
        var support = _supports.Get((pattern, checksum, parsed.HasDates ? date : null), key => new(parsed, key.Date));
        if (support.Count.IsZero)
        {
            throw new EmptyPatternSupport(pattern);
        }

        return support.Unrank(GenerationIndex.Draw(support.Count));
    }
}
