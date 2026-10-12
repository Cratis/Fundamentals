// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v7_in_context;

public class with_a_time_before_the_unix_epoch : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.UuidV7(new GenerationContext(DateTimeOffset.UnixEpoch.AddTicks(-1), GenerationContext.DefaultTenant)));

    [Fact] void should_reject_the_time() => _error.ShouldBeOfExactType<GenerationTimeBeforeUnixEpoch>();
}
