// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_bytes;

public class with_invalid_counts : Specification
{
    Exception[] _errors;

    void Because() => _errors = [.. new[] { 0, 513 }.Select(count => Catch.Exception(() => GenerateValue.Bytes(count)))];

    [Fact] void should_reject_counts_outside_the_range() => _errors.All(error => error is InvalidByteCount).ShouldBeTrue();
}
