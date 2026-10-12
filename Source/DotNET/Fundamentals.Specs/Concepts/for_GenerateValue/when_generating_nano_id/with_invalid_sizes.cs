// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_nano_id;

public class with_invalid_sizes : Specification
{
    Exception[] _errors;

    void Because() => _errors = [.. new[] { 0, 257 }.Select(size => Catch.Exception(() => GenerateValue.NanoId(size)))];

    [Fact] void should_reject_sizes_outside_the_range() => _errors.All(error => error is InvalidNanoIdSize).ShouldBeTrue();
}
