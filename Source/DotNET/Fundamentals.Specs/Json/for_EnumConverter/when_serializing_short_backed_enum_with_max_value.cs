// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_serializing_short_backed_enum_with_max_value : given.configured_serialization_options
{
    string _result;

    void Because() => _result = JsonSerializer.Serialize(given.ShortBackedEnum.Max, _options);

    [Fact] void should_serialize_as_the_full_number() => _result.ShouldEqual("32767");
}
