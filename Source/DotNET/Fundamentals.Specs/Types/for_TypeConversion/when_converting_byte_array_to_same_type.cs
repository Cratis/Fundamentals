// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_byte_array_to_same_type : Specification
{
    byte[] input;
    byte[] result;

    void Establish() => input = [1, 2, 3];

    void Because() => result = (byte[])TypeConversion.Convert(typeof(byte[]), input);

    [Fact] void should_return_the_same_bytes() => result.ShouldContainOnly(input);
}
