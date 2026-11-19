// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_a_value_to_a_type_without_a_known_conversion : Specification
{
    Exception result;

    void Because() => result = Catch.Exception(() => TypeConversion.Convert(typeof(Version), "1.0"));

    [Fact] void should_throw_invalid_cast_exception() => result.ShouldBeOfExactType<InvalidCastException>();
}
