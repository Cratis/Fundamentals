// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_type_id;

public class with_valid_prefix_boundaries : Specification
{
    string[] _prefixes;
    string[] _results;

    void Establish() => _prefixes = ["a", "a_b", $"a{new string('_', 61)}z"];
    void Because() => _results = [.. _prefixes.Select(GenerateValue.TypeId)];

    [Fact] void should_accept_one_letter_internal_underscores_and_sixty_three_characters() => _results.Select((value, index) => value[..^27] == _prefixes[index]).All(matches => matches).ShouldBeTrue();
}
