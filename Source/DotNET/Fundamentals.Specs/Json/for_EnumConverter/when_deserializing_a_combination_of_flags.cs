// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Cratis.Json.for_EnumConverter;

/// <summary>
/// A combination is not a declared member, so Enum.IsDefined answers false for it and the value was rejected -
/// while Write had serialized it happily. Anything carrying a [Flags] enum across a boundary that uses this
/// converter therefore failed to come back, and did so at read time rather than at write time.
/// </summary>
public class when_deserializing_a_combination_of_flags : given.configured_serialization_options
{
    MyFlags _result;

    void Because() => _result = JsonSerializer.Deserialize<MyFlags>("5", _options);

    [Fact] void should_have_the_first_flag() => _result.HasFlag(MyFlags.First).ShouldBeTrue();
    [Fact] void should_have_the_third_flag() => _result.HasFlag(MyFlags.Third).ShouldBeTrue();
    [Fact] void should_not_have_the_second_flag() => _result.HasFlag(MyFlags.Second).ShouldBeFalse();
}
