// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_counting_pattern_support;

public class with_an_exhausted_operation_budget : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => _ = new GenerationPatternSupport(GenerationPattern.Parse("#"), budget: new GenerationWorkBudget("#", 0)));

    [Fact] void should_fail_with_the_pattern_and_budget_reason() => _error.ShouldBeOfExactType<InvalidValuePattern>();
    [Fact] void should_name_the_budget() => _error.Message.ShouldContain("Generation support operation budget (0 operations) exceeded.");
}
