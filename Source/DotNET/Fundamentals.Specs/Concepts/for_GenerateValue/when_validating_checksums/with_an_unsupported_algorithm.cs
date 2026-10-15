// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_validating_checksums;

public class with_an_unsupported_algorithm : Specification
{
    Exception[] _errors;

    void Because()
    {
        Checksum[] algorithms = [Checksum.None, (Checksum)99];
        _errors = [.. algorithms.Select(algorithm => Catch.Exception(() => GenerateValue.HasValidChecksum("0", algorithm)))];
    }

    [Fact] void should_reject_none_and_undefined_algorithms() => _errors.All(error => error is UnknownChecksum).ShouldBeTrue();
}
