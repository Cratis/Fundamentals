// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_boxed_byte_sequence_to_byte_array : Specification
{
    IEnumerable<object> input;
    byte[] result;

    void Establish() => input = [(byte)1, (byte)2, (byte)3];

    void Because() => result = (byte[])TypeConversion.Convert(typeof(byte[]), input);

    [Fact] void should_return_the_same_bytes() => result.ShouldContainOnly((byte[])[1, 2, 3]);
}
