// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_round_tripping_a_combination_of_sbyte_flags : given.configured_serialization_options
{
    given.SByteFlags _result;
    string _json;

    void Because()
    {
        _json = JsonSerializer.Serialize(given.SByteFlags.Sign | given.SByteFlags.One, _options);
        _result = JsonSerializer.Deserialize<given.SByteFlags>(_json, _options);
    }

    [Fact] void should_serialize_as_the_signed_number() => _json.ShouldEqual("-127");
    [Fact] void should_come_back_unchanged() => _result.ShouldEqual(given.SByteFlags.Sign | given.SByteFlags.One);
}
