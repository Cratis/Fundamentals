// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_patterns_in_context;

public class without_a_captured_time : Specification
{
    Exception[] _errors;

    void Because()
    {
        string[] patterns = ["#", "{yyyy}#"];
        _errors = [.. patterns.Select(pattern => Catch.Exception(() => GenerateValue.Pattern(pattern, default(GenerationContext))))];
    }

    [Fact] void should_require_time_even_for_date_free_patterns() => _errors.All(error => error is MissingGenerationTime).ShouldBeTrue();
}
