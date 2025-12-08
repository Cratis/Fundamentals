// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_timespan_to_same_type : Specification
{
    TimeSpan input;
    TimeSpan result;

    void Establish() => input = TimeSpan.FromMinutes(42);

    void Because() => result = (TimeSpan)TypeConversion.Convert(typeof(TimeSpan), input);

    [Fact] void should_return_same_value() => result.ShouldEqual(input);
}
