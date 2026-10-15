// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Cratis.Concepts;

/// <summary>
/// Represents one character alphabet or date segment and its bounded repetition.
/// </summary>
/// <param name="Characters">The distinct character alphabet, empty for a date segment.</param>
/// <param name="Minimum">The minimum repetition count.</param>
/// <param name="Maximum">The maximum repetition count.</param>
/// <param name="Segment">The UTC date segment name, or null for a character token.</param>
internal sealed record GenerationPatternToken(string Characters, int Minimum = 1, int Maximum = 1, string? Segment = null)
{
    /// <summary>
    /// Gets the character width of one occurrence.
    /// </summary>
    internal int Width => Segment switch { "yyyy" => 4, null => 1, _ => 2 };

    /// <summary>
    /// Expands one occurrence to character alphabets, substituting a fixed date when supplied.
    /// </summary>
    /// <param name="date">The fixed UTC date, or null for date-width-only shape matching.</param>
    /// <returns>One alphabet per character position.</returns>
    internal string[] Expand(DateOnly? date)
    {
        if (Segment is null)
        {
            return [Characters];
        }

        if (date is null)
        {
            return [.. Enumerable.Repeat("0123456789", Width)];
        }

        // Explicit numeric formatting also fixes year widths for years before 1000.
        var text = Segment switch
        {
            "yyyy" => date.Value.Year.ToString("D4", CultureInfo.InvariantCulture),
            "yy" => (date.Value.Year % 100).ToString("D2", CultureInfo.InvariantCulture),
            "MM" => date.Value.Month.ToString("D2", CultureInfo.InvariantCulture),
            _ => date.Value.Day.ToString("D2", CultureInfo.InvariantCulture)
        };

        return [.. text.Select(character => character.ToString())];
    }
}
