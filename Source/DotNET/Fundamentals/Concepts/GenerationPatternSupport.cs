// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Cratis.Concepts;

/// <summary>
/// Holds exact checksum-conditioned support and maps uniform indices bijectively to values.
/// </summary>
internal sealed class GenerationPatternSupport
{
    readonly GenerationPattern _pattern;
    readonly GenerationCompletion _start;

    /// <summary>
    /// Constructs and counts the admissible payload language for one substituted date.
    /// </summary>
    /// <param name="pattern">The validated pattern.</param>
    /// <param name="date">The UTC date required by date-bearing patterns.</param>
    /// <param name="budget">An optional shared construction budget.</param>
    /// <exception cref="PatternRequiresGenerationContext">A date-bearing pattern has no date.</exception>
    /// <exception cref="InvalidValuePattern">A construction limit is exceeded.</exception>
    internal GenerationPatternSupport(GenerationPattern pattern, DateOnly? date = null, GenerationWorkBudget? budget = null)
    {
        _pattern = pattern;
        if (pattern.HasDates && date is null)
        {
            throw new PatternRequiresGenerationContext(pattern.Text);
        }

        budget ??= new(pattern.Text);
        var conditioned = pattern.Checksum == Checksum.Mod11;
        var automaton = new GenerationPatternAutomaton(pattern, date, conditioned, budget);
        _start = new GenerationCompletionCounts(automaton, conditioned, budget).Build((automaton.Start, 0, 0));
    }

    /// <summary>
    /// Gets the exact number of distinct admissible strings.
    /// </summary>
    internal BigInteger Count => _start.Count;

    /// <summary>
    /// Checks payload and suffix shape without implying checksum arithmetic validation.
    /// </summary>
    /// <param name="pattern">The validated pattern.</param>
    /// <param name="value">The candidate payload and suffix.</param>
    /// <param name="instant">The pinned instant, or null for date-width-only matching.</param>
    /// <returns>Whether the candidate belongs to the pattern's shape.</returns>
    internal static bool Matches(GenerationPattern pattern, string value, DateTimeOffset? instant = null)
    {
        var width = pattern.ChecksumWidth;
        if (value.Length < pattern.MinimumLength || value.Length > pattern.MaximumLength)
        {
            return false;
        }

        if (width > 0 && !value[^width..].All(character => character is >= '0' and <= '9'))
        {
            return false;
        }

        var date = instant is null ? (DateOnly?)null : DateOnly.FromDateTime(instant.Value.UtcDateTime);
        var automaton = new GenerationPatternAutomaton(pattern, date, false, new(pattern.Text));
        var state = automaton.Start;
        foreach (var character in value[..(value.Length - width)])
        {
            state = automaton.Move(state, character);
            if (automaton.IsEmpty(state))
            {
                return false;
            }
        }

        return automaton.Accepts(state);
    }

    /// <summary>
    /// Maps one admissible index to its distinct emitted string without redrawing payloads.
    /// </summary>
    /// <param name="index">The zero-based index below Count.</param>
    /// <returns>The payload and any appended check digits.</returns>
    /// <exception cref="InvalidValuePattern">The index is outside the counted range.</exception>
    internal string Unrank(BigInteger index)
    {
        if (index < 0 || index >= Count)
        {
            throw new InvalidValuePattern(_pattern.Text, "support index is outside the counted range");
        }

        var node = _start;
        var characters = new List<char>();
        while (true)
        {
            // The empty completion comes before all character branches.
            if (node.Accepts)
            {
                if (index.IsZero)
                {
                    break;
                }

                index--;
            }

            foreach (var (character, target) in node.Branches)
            {
                if (index < target.Count)
                {
                    characters.Add(character);
                    node = target;
                    break;
                }

                index -= target.Count;
            }
        }

        if (_pattern.Checksum == Checksum.Mod11)
        {
            characters.Reverse();
        }

        var payload = new string([.. characters]);

        return payload + GenerationChecksum.Calculate(payload, _pattern.Checksum);
    }
}
