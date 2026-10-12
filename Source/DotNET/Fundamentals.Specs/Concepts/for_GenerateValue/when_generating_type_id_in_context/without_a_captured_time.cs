// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_type_id_in_context;

public class without_a_captured_time : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.TypeId("order", default));

    [Fact] void should_reject_the_missing_time() => _error.ShouldBeOfExactType<MissingGenerationTime>();
}
