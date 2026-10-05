// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

public class when_deserializing_ushort_backed_enum_with_max_value : given.configured_serialization_options
{
    given.UShortBackedEnum _result;

    void Because() => _result = JsonSerializer.Deserialize<given.UShortBackedEnum>("65535", _options);

    [Fact] void should_deserialize_to_the_member() => _result.ShouldEqual(given.UShortBackedEnum.Max);
}
