// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_string_to_timespan : Specification
{
    TimeSpan input;
    TimeSpan result;

    void Establish() => input = TimeSpan.FromHours(2.5);

    void Because() => result = (TimeSpan)TypeConversion.Convert(typeof(TimeSpan), input.ToString());

    [Fact] void should_convert_correctly() => result.ShouldEqual(input);
}
