// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts.for_GenerateValue;

namespace Cratis.Concepts.for_IGeneratableInContext;

public class when_creating_an_order_id : Specification
{
    GenerationContext _context;
    OrderId _result;

    void Establish() => _context = new(DateTimeOffset.Parse("2024-06-15T12:34:56.789Z"), GenerationContext.DefaultTenant);
    void Because() => _result = OrderId.New(_context);

    [Fact] void should_use_the_order_prefix() => _result.Value[..6].ShouldEqual("order_");
    [Fact] void should_use_the_captured_time() => CrockfordDecoding.Timestamp(CrockfordDecoding.Decode(_result.Value[6..])).ShouldEqual(_context.Now.ToUnixTimeMilliseconds());

    record OrderId(string Value) : ConceptAs<string>(Value), IGeneratableInContext<OrderId>
    {
        public static OrderId New(GenerationContext context) => new(GenerateValue.TypeId("order", context));
    }
}
