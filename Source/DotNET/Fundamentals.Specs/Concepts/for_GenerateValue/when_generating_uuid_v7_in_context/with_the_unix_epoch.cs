// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v7_in_context;

public class with_the_unix_epoch : Specification
{
    Guid _result;

    void Because() => _result = GenerateValue.UuidV7(new GenerationContext(DateTimeOffset.UnixEpoch, GenerationContext.DefaultTenant));

    [Fact] void should_accept_zero_unix_milliseconds() => CrockfordDecoding.Timestamp(_result.ToByteArray(bigEndian: true)).ShouldEqual(0L);
}
