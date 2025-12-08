// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_TypeExtensions;

public class when_checking_if_non_timespan_is_timespan : Specification
{
    bool result;

    void Because() => result = typeof(string).IsTimeSpan();

    [Fact] void should_return_false() => result.ShouldBeFalse();
}
