// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Cratis.Concepts;

/// <summary>
/// Parses the shared finite printable-ASCII generation language.
/// </summary>
/// <param name="pattern">The authored pattern.</param>
internal sealed partial class GenerationPatternParser(string pattern)
{
    int _offset;
    bool _hasRandom;

    /// <summary>
    /// Parses every token and validates repetition, alphabet and length limits.
    /// </summary>
    /// <returns>The bounded token sequence.</returns>
    /// <exception cref="InvalidValuePattern">The pattern violates the grammar or length limit.</exception>
    internal IReadOnlyList<GenerationPatternToken> Parse()
    {
        if (pattern.Length == 0)
        {
            Fail("empty pattern");
        }

        if (pattern.Any(character => character is < ' ' or > '~'))
        {
            Fail("patterns require printable ASCII");
        }

        var tokens = new List<GenerationPatternToken>();
        var maximumLength = 0;
        while (_offset < pattern.Length)
        {
            var token = Token();
            if (_offset < pattern.Length && pattern[_offset] == '{' && _offset + 1 < pattern.Length && char.IsAsciiDigit(pattern[_offset + 1]))
            {
                token = Repeat(token);
            }

            maximumLength += token.Width * token.Maximum;
            if (maximumLength > GenerationPattern.LengthLimit)
            {
                Fail("final expanded length exceeds 256 characters including check digits");
            }

            tokens.Add(token);
        }

        if (!_hasRandom)
        {
            Fail("pattern requires a random token (#, ?, *, or a class)");
        }

        return tokens;
    }

    GenerationPatternToken Token()
    {
        var character = pattern[_offset++];
        switch (character)
        {
            case '#':
                _hasRandom = true;
                return new("0123456789");
            case '?':
                _hasRandom = true;
                return new("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            case '*':
                _hasRandom = true;
                return new("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            case '[':
                _hasRandom = true;
                return new(CharacterClass());
            case '\\':
                return new(Escape("#?*[]{}\\").ToString());
            case '{':
                return Segment();
            case ']':
            case '}':
                Fail("unescaped closing delimiter");
                break;
        }

        return new(character.ToString());
    }

    GenerationPatternToken Segment()
    {
        var end = pattern.IndexOf('}', _offset);
        if (end < 0)
        {
            Fail("unclosed date segment");
        }

        var segment = pattern[_offset..end];
        if (segment.Length > 0 && char.IsAsciiDigit(segment[0]))
        {
            Fail("repeat has no preceding token");
        }

        if (segment is not ("yyyy" or "yy" or "MM" or "dd"))
        {
            Fail("unknown date segment; use yyyy, yy, MM or dd");
        }

        _offset = end + 1;

        return new(string.Empty, Segment: segment);
    }

    GenerationPatternToken Repeat(GenerationPatternToken token)
    {
        _offset++;
        var end = pattern.IndexOf('}', _offset);
        if (end < 0)
        {
            Fail("unclosed repeat");
        }

        var parts = pattern[_offset..end].Split(',');
        if (parts.Length is < 1 or > 2 || parts.Any(part => part.Length == 0 || part.Any(character => !char.IsAsciiDigit(character))))
        {
            Fail("malformed repeat; use {n} or {m,n}");
        }

        var validMinimum = int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var minimum);
        var validMaximum = int.TryParse(parts[^1], NumberStyles.None, CultureInfo.InvariantCulture, out var maximum);
        if (!validMinimum || !validMaximum)
        {
            Fail("repeat bound exceeds 64");
        }

        if (minimum < 1)
        {
            Fail("repeat minimum must be at least 1");
        }

        if (minimum > maximum)
        {
            Fail("repeat minimum exceeds maximum");
        }

        if (maximum > 64)
        {
            Fail("repeat maximum exceeds 64");
        }

        _offset = end + 1;

        return token with { Minimum = minimum, Maximum = maximum };
    }

    char Escape(string allowed)
    {
        if (_offset == pattern.Length)
        {
            Fail("dangling escape");
        }

        var character = pattern[_offset++];
        if (!allowed.Contains(character, StringComparison.Ordinal))
        {
            Fail("illegal escape");
        }

        return character;
    }

    [DoesNotReturn]
    void Fail(string reason) => throw new InvalidValuePattern(pattern, reason);
}
