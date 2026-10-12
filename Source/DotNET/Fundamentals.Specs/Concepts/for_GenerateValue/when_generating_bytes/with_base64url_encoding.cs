// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_bytes;

public class with_base64url_encoding : Specification
{
    string[] _values;

    void Because() => _values = [.. Enumerable.Range(1, 512).Select(count => GenerateValue.Bytes(count))];

    [Fact] void should_have_the_unpadded_length() => _values.Select((value, index) => value.Length == ((4 * (index + 1)) + 2) / 3).All(matches => matches).ShouldBeTrue();
    [Fact] void should_use_only_the_url_safe_alphabet() => _values.All(value => value.All("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".Contains)).ShouldBeTrue();
    [Fact] void should_decode_to_the_requested_byte_count() => _values.Select((value, index) => Decode(value).Length == index + 1).All(matches => matches).ShouldBeTrue();
    [Fact] void should_roundtrip_without_padding() => _values.All(value => Convert.ToBase64String(Decode(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_') == value).ShouldBeTrue();

    static byte[] Decode(string value) => Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/').PadRight((value.Length + 3) / 4 * 4, '='));
}
