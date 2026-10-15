// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns;

public class with_an_unknown_checksum : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.Pattern("#", (Checksum)99));

    [Fact] void should_reject_the_algorithm() => _error.ShouldBeOfExactType<UnknownChecksum>();
}
