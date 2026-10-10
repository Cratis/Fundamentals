// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts.for_IGeneratable;

public class when_creating_through_the_static_contract : Specification
{
    SomeId _result;

    void Because() => _result = Create<SomeId>();

    [Fact] void should_create_a_non_empty_value() => (_result.Value != Guid.Empty).ShouldBeTrue();

    static T Create<T>()
        where T : IGeneratable<T> => T.New();

    record SomeId(Guid Value) : ConceptAs<Guid>(Value), IGeneratable<SomeId>
    {
        public static SomeId New() => new(GenerateValue.Uuid());
    }
}
