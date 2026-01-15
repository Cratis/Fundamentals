// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_TypeExtensions.for_HasAttribute;

public class when_checking_type_with_multiple_attributes_of_same_type : Specification
{
    static bool result;

    void Because() => result = typeof(TypeWithMultipleAttributesOfSameType).HasAttribute<CustomAttribute>();

    [Fact] void should_return_true() => result.ShouldBeTrue();
}
