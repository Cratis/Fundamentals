// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization.for_DerivedTypes;

public class when_getting_derived_type_without_any_matching_derived_type_identifier : given.derived_types
{
    interface ITargetType;

    [DerivedType("known-type")]
    record DerivedType : ITargetType { }

    DerivedTypes derived_types;
    Exception result;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(DerivedType)];

    void Establish() => derived_types = new DerivedTypes(types.Object);

    void Because() => result = Catch.Exception(() => _ = derived_types.GetDerivedTypeFor(typeof(ITargetType), "unknown-type"));

    [Fact] void should_throw_missing_derived_type_for_target_type() => result.ShouldBeOfExactType<MissingDerivedTypeForTargetType>();
}
