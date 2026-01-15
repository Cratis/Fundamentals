// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_PropertyExtensions;

public class when_checking_if_nullable_value_type_class_property_is_nullable : Specification
{
    static bool result;

    void Because() => result = typeof(ClassWithProperties).GetProperty(nameof(ClassWithProperties.NullableInt))!.IsNullable();

    [Fact] void should_return_true() => result.ShouldBeTrue();
}
