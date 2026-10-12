// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_ulid_in_context;

public class with_the_unix_epoch : Specification
{
    string _result;

    void Because() => _result = GenerateValue.Ulid(new GenerationContext(DateTimeOffset.UnixEpoch, GenerationContext.DefaultTenant));

    [Fact] void should_encode_a_zero_timestamp() => _result[..10].ShouldEqual("0000000000");
}
