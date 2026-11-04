// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_sha256;

public class with_a_null_value : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.Sha256(null!));

    [Fact] void should_reject_the_null_value() => _error.ShouldBeOfExactType<ArgumentNullException>();
}
