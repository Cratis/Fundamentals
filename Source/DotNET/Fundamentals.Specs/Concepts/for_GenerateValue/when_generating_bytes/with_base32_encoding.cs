// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_bytes;

public class with_base32_encoding : Specification
{
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    string[] _values;

    void Because() => _values = [.. Enumerable.Range(1, 512).Select(count => GenerateValue.Bytes(count, ValueEncoding.Base32))];

    [Fact] void should_have_the_unpadded_rfc_length() => _values.Select((value, index) => value.Length == ((8 * (index + 1)) + 4) / 5).All(matches => matches).ShouldBeTrue();
    [Fact] void should_use_the_uppercase_rfc_alphabet() => _values.All(value => value.All(Alphabet.Contains)).ShouldBeTrue();
    [Fact] void should_decode_to_the_requested_byte_count() => _values.Select((value, index) => Decode(value).Length == index + 1).All(matches => matches).ShouldBeTrue();
    [Fact] void should_roundtrip_with_zero_trailing_padding_bits() => _values.All(value => Encode(Decode(value)) == value).ShouldBeTrue();

    static byte[] Decode(string value)
    {
        var bits = string.Concat(value.Select(character => Convert.ToString(Alphabet.IndexOf(character), 2).PadLeft(5, '0')));

        return [.. Enumerable.Range(0, bits.Length / 8).Select(index => Convert.ToByte(bits.Substring(index * 8, 8), 2))];
    }

    static string Encode(byte[] bytes)
    {
        var bits = string.Concat(bytes.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));
        bits = bits.PadRight((bits.Length + 4) / 5 * 5, '0');

        return new string([.. Enumerable.Range(0, bits.Length / 5).Select(index => Alphabet[Convert.ToInt32(bits.Substring(index * 5, 5), 2)])]);
    }
}
