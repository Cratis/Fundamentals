// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_GenerateValue.when_generating_type_id_in_context;

public class with_invalid_prefixes : Specification
{
    GenerationContext _context;
    Exception[] _errors;

    void Establish() => _context = new(DateTimeOffset.UnixEpoch, GenerationContext.DefaultTenant);
    void Because() => _errors = [.. new[] { "User", "_user", "user_", "us-er", new string('a', 64), null! }.Select(prefix => Catch.Exception(() => GenerateValue.TypeId(prefix, _context)))];

    [Fact] void should_reject_every_invalid_prefix() => _errors.All(error => error is InvalidTypeIdPrefix).ShouldBeTrue();
}
