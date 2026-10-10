// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v7;

public class with_different_timestamps : Specification
{
    Guid _earlier;
    Guid _later;

    void Because()
    {
        _earlier = GenerateValue.UuidV7();
        var earlierTimestamp = _earlier.ToByteArray(bigEndian: true).AsSpan(0, 6);
        for (var attempt = 0; attempt < 1_000_000; attempt++)
        {
            _later = GenerateValue.UuidV7();
            if (_later.ToByteArray(bigEndian: true).AsSpan(0, 6).SequenceCompareTo(earlierTimestamp) > 0)
            {
                break;
            }
        }
    }

    [Fact] void should_observe_a_later_millisecond() => _later.ToByteArray(bigEndian: true).AsSpan(0, 6).SequenceCompareTo(_earlier.ToByteArray(bigEndian: true).AsSpan(0, 6)).ShouldBeGreaterThan(0);
    [Fact] void should_sort_the_later_value_after_the_earlier_value() => string.CompareOrdinal(_later.ToString(), _earlier.ToString()).ShouldBeGreaterThan(0);
}
