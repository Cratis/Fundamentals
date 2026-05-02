// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization.for_DerivedTypes;

public class when_initializing_with_type_with_target_type_but_no_target_types_implemented : given.derived_types
{
    interface ITarget;

    [DerivedType("known-type", typeof(ITarget))]
    record DerivedType { }

    Exception result;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(DerivedType)];

    void Because() => result = Catch.Exception(() => _ = new DerivedTypes(types.Object));

    [Fact] void should_throw_missing_target_type() => result.ShouldBeOfExactType<MissingTargetTypeForDerivedType>();
}
