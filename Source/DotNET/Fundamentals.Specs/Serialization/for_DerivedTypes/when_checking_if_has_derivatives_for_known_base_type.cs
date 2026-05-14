// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization.for_DerivedTypes;

public class when_checking_if_has_derivatives_for_known_base_type : given.derived_types
{
    interface ITargetType;

    abstract record BaseType : ITargetType;

    [DerivedType("known-type")]
    record DerivedType : BaseType;

    bool result;
    DerivedTypes derived_types;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(DerivedType)];

    void Establish() => derived_types = new(types.Object);

    void Because() => result = derived_types.HasDerivatives(typeof(BaseType));

    [Fact] void should_have_derivatives() => result.ShouldBeTrue();
}
