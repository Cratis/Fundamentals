// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns;

public class without_context_for_dates : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.Pattern("INV-{yyyy}-#{7}"));

    [Fact] void should_require_a_captured_date() => _error.ShouldBeOfExactType<PatternRequiresGenerationContext>();
}
