// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid;

public class with_the_rfc_variant : Specification
{
    Guid _result;

    void Because() => _result = GenerateValue.Uuid();

    [Fact] void should_have_the_rfc_variant() => (_result.ToByteArray(bigEndian: true)[8] >> 6).ShouldEqual(2);
}
