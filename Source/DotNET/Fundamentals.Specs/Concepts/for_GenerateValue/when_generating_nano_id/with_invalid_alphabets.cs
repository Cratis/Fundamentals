// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_nano_id;

public class with_invalid_alphabets : Specification
{
    string[] _alphabets;
    Exception[] _errors;

    void Establish() => _alphabets = [string.Empty, "a", "aba", "aé", "a\n", "a\u007f", new string('a', 257)];
    void Because() => _errors = [.. _alphabets.Select(alphabet => Catch.Exception(() => GenerateValue.NanoId(alphabet: alphabet)))];

    [Fact] void should_reject_every_invalid_alphabet() => _errors.All(error => error is InvalidNanoIdAlphabet).ShouldBeTrue();
}
