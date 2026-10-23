// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization.for_DerivedTypes;

public class when_getting_for_same_universe_twice : given.derived_types
{
    interface ITargetType;

    [DerivedType("known-type")]
    record DerivedType : ITargetType;

    DerivedTypes first;
    DerivedTypes second;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(DerivedType)];

    void Because()
    {
        first = DerivedTypes.For(types.Object);
        second = DerivedTypes.For(types.Object);
    }

    [Fact] void should_return_same_instance() => ReferenceEquals(first, second).ShouldBeTrue();
    [Fact] void should_resolve_identifier() => first.GetDerivedTypeFor(typeof(ITargetType), "known-type").ShouldEqual(typeof(DerivedType));
}
