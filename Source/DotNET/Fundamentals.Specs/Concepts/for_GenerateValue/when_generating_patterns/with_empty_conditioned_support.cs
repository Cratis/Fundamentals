// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns;

public class with_empty_conditioned_support : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.Pattern("000000006-[AB]", Checksum.Mod11));

    [Fact] void should_fail_without_redrawing() => _error.ShouldBeOfExactType<EmptyPatternSupport>();
}
