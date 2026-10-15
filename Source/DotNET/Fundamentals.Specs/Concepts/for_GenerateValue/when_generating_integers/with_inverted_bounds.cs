// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_integers;

public class with_inverted_bounds : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.Integer(2, 1));

    [Fact] void should_reject_the_range() => _error.ShouldBeOfExactType<InvalidIntegerRange>();
}
