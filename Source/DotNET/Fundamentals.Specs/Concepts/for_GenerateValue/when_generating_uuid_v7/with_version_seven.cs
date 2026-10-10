// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_uuid_v7;

public class with_version_seven : Specification
{
    Guid _result;

    void Because() => _result = GenerateValue.UuidV7();

    [Fact] void should_have_version_seven() => (_result.ToByteArray(bigEndian: true)[6] >> 4).ShouldEqual(7);
}
