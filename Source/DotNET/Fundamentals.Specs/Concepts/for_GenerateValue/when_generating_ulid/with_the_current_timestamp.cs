// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_ulid;

public class with_the_current_timestamp : Specification
{
    long _before;
    long _after;
    long _timestamp;

    void Because()
    {
        _before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _timestamp = CrockfordDecoding.Timestamp(CrockfordDecoding.Decode(GenerateValue.Ulid()));
        _after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    [Fact] void should_encode_the_creation_time() => (_timestamp >= _before && _timestamp <= _after).ShouldBeTrue();
}
