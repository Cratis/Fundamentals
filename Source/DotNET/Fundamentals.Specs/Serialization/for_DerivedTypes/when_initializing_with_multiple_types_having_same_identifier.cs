// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Serialization.for_DerivedTypes;

public class when_initializing_with_multiple_types_having_same_identifier : given.derived_types
{
    interface ITargetType;

    [DerivedType("first-type")]
    record FirstDerivedType : ITargetType { }

    [DerivedType("first-type")]
    record SecondDerivedType : ITargetType { }

    Exception result;

    protected override IEnumerable<Type> ProvideDerivedTypes() => [typeof(FirstDerivedType), typeof(SecondDerivedType)];

    void Because() => result = Catch.Exception(() => _ = new DerivedTypes(types.Object));

    [Fact] void should_throw_ambiguous_type_identifiers() => result.ShouldBeOfExactType<AmbiguousDerivedTypeIdentifiers>();
}
