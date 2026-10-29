// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v5;

public class with_a_null_name : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.UuidV5(Guid.Empty, null!));

    [Fact] void should_reject_the_null_name() => _error.ShouldBeOfExactType<ArgumentNullException>();
}
