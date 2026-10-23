// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Types;

namespace Cratis.Serialization.for_DerivedTypes;

public class when_getting_for_different_universes : given.derived_types
{
    interface ITargetType;

    [DerivedType("known-type")]
    record DerivedType : ITargetType;

    Mock<ITypes> other_types;
    DerivedTypes first;
    DerivedTypes second;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(DerivedType)];

    void Establish()
    {
        other_types = new();
        other_types.SetupGet(_ => _.All).Returns([]);
    }

    void Because()
    {
        first = DerivedTypes.For(types.Object);
        second = DerivedTypes.For(other_types.Object);
    }

    [Fact] void should_return_different_instances() => ReferenceEquals(first, second).ShouldBeFalse();
    [Fact] void should_keep_first_universe_content() => first.HasDerivatives(typeof(ITargetType)).ShouldBeTrue();
    [Fact] void should_keep_second_universe_content() => second.HasDerivatives(typeof(ITargetType)).ShouldBeFalse();
}
