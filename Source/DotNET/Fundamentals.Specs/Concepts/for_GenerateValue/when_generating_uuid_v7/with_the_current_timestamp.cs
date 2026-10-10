// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v7;

public class with_the_current_timestamp : Specification
{
    long _before;
    long _after;
    Guid _result;

    void Because()
    {
        _before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _result = GenerateValue.UuidV7();
        _after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    [Fact] void should_encode_the_creation_time_in_the_first_six_bytes() => (Timestamp(_result) >= _before && Timestamp(_result) <= _after).ShouldBeTrue();

    static long Timestamp(Guid value) => value.ToByteArray(bigEndian: true).Take(6).Aggregate(0L, (timestamp, next) => (timestamp << 8) | next);
}
