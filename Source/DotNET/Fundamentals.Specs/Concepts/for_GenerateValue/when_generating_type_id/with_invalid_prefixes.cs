// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_type_id;

public class with_invalid_prefixes : Specification
{
    string[] _prefixes;
    Exception[] _errors;

    void Establish() => _prefixes = ["User", "_user", "user_", "us-er", new string('a', 64), null!, "user1", "user\n"];
    void Because() => _errors = [.. _prefixes.Select(prefix => Catch.Exception(() => GenerateValue.TypeId(prefix)))];

    [Fact] void should_reject_every_invalid_prefix() => _errors.All(error => error is InvalidTypeIdPrefix).ShouldBeTrue();
}
