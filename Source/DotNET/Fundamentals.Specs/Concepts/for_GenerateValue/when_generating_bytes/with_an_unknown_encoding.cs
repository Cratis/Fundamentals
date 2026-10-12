// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_bytes;

public class with_an_unknown_encoding : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.Bytes(16, (ValueEncoding)3));

    [Fact] void should_reject_the_encoding() => _error.ShouldBeOfExactType<UnknownValueEncoding>();
}
