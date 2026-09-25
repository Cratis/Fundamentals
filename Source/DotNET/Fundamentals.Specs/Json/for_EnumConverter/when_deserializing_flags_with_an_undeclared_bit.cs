// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

/// <summary>
/// Accepting combinations must not turn the check off: a bit no declared flag accounts for is still garbage.
/// </summary>
public class when_deserializing_flags_with_an_undeclared_bit : given.configured_serialization_options
{
    Exception _result;

    void Because() => _result = Catch.Exception(() => JsonSerializer.Deserialize<MyFlags>("9", _options));

    [Fact] void should_throw_json_exception() => _result.ShouldBeOfExactType<JsonException>();
}
