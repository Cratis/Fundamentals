// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

internal sealed partial class GenerationPatternParser
{
    string CharacterClass()
    {
        if (_offset < pattern.Length && pattern[_offset] == '^')
        {
            Fail("negated classes are not supported");
        }

        var characters = new SortedSet<char>();
        while (_offset < pattern.Length && pattern[_offset] != ']')
        {
            var first = ClassCharacter();
            if (_offset < pattern.Length && pattern[_offset] == '-')
            {
                _offset++;
                if (_offset == pattern.Length || pattern[_offset] == ']')
                {
                    Fail("class range needs an endpoint; escape a literal hyphen");
                }

                var last = ClassCharacter();
                if (last < first)
                {
                    Fail("descending class range");
                }

                for (var value = (int)first; value <= last; value++)
                {
                    characters.Add((char)value);
                }
            }
            else
            {
                characters.Add(first);
            }
        }

        if (_offset == pattern.Length)
        {
            Fail("unclosed class");
        }

        if (characters.Count == 0)
        {
            Fail("empty class");
        }

        _offset++;

        return new([.. characters]);
    }

    char ClassCharacter()
    {
        var character = pattern[_offset++];
        if (character == '\\')
        {
            return Escape("]-\\");
        }

        if (character is '-' or '[')
        {
            Fail("escape a literal class delimiter or hyphen");
        }

        return character;
    }
}
