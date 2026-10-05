// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_round_tripping_a_combination_of_ulong_flags : given.configured_serialization_options
{
    given.ULongFlags _result;

    void Because() =>
        _result = JsonSerializer.Deserialize<given.ULongFlags>(
            JsonSerializer.Serialize(given.ULongFlags.High | given.ULongFlags.Low, _options),
            _options);

    [Fact] void should_come_back_unchanged() => _result.ShouldEqual(given.ULongFlags.High | given.ULongFlags.Low);
}
