// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization.for_DerivedTypes;

public class when_getting_known_derived_type_that_matches_base_class : given.derived_types
{
    interface ITargetType;

    abstract record BaseType : ITargetType;

    [DerivedType("known-type")]
    record DerivedType : BaseType;

    DerivedTypes derived_types;
    Type result;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(DerivedType)];

    void Establish() => derived_types = new DerivedTypes(types.Object);

    void Because() => result = derived_types.GetDerivedTypeFor(typeof(BaseType), "known-type");

    [Fact] void should_return_derived_type() => result.ShouldEqual(typeof(DerivedType));
}
