// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts.for_GenerateValue;

namespace Cratis.Concepts.for_IGeneratableInContext;

public class when_creating_through_the_static_contract : Specification
{
    GenerationContext _context;
    TenantOrderId _result;

    void Establish() => _context = new(DateTimeOffset.Parse("2024-06-15T12:34:56.789Z"), "tenant-a");
    void Because() => _result = Create<TenantOrderId>(_context);

    [Fact] void should_use_the_portable_tenant() => _result.Value[..9].ShouldEqual("tenant-a_");
    [Fact] void should_use_the_captured_time() => CrockfordDecoding.Timestamp(CrockfordDecoding.Decode(_result.Value[9..])).ShouldEqual(_context.Now.ToUnixTimeMilliseconds());

    static T Create<T>(GenerationContext context)
        where T : IGeneratableInContext<T> => T.New(context);

    record TenantOrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<TenantOrderId>
    {
        public static TenantOrderId New(GenerationContext context) => new($"{context.Tenant}_{GenerateValue.Ulid(context)}");
    }
}
