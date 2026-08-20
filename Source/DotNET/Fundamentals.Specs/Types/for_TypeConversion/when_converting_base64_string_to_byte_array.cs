// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_TypeConversion;

public class when_converting_base64_string_to_byte_array : Specification
{
    byte[] input;
    string base64;
    byte[] result;

    void Establish()
    {
        input = [1, 2, 3, 255, 0];
        base64 = Convert.ToBase64String(input);
    }

    void Because() => result = (byte[])TypeConversion.Convert(typeof(byte[]), base64);

    [Fact] void should_decode_to_the_same_bytes() => result.ShouldContainOnly(input);
}
