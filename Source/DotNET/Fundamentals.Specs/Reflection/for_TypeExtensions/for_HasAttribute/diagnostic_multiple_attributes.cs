// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_TypeExtensions.for_HasAttribute;

public class diagnostic_multiple_attributes : Specification
{
    static bool isDefinedResult;
    static int getCustomAttributesCount;

    void Because()
    {
        var type = typeof(TypeWithMultipleAttributesOfSameType);
        isDefinedResult = Attribute.IsDefined(type, typeof(CustomAttribute));
        getCustomAttributesCount = type.GetCustomAttributes(typeof(CustomAttribute), false).Length;
    }

    [Fact] void should_have_is_defined_true() => isDefinedResult.ShouldBeTrue();
    [Fact] void should_have_two_attributes() => getCustomAttributesCount.ShouldEqual(2);
}
