// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_validating_checksums;

public class without_a_value : Specification
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => GenerateValue.HasValidChecksum(null!, Checksum.Luhn));

    [Fact] void should_reject_null() => _error.ShouldBeOfExactType<ArgumentNullException>();
}
