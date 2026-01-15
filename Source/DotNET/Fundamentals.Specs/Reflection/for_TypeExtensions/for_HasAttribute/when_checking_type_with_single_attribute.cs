// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_TypeExtensions.for_HasAttribute;

public class when_checking_type_with_single_attribute : Specification
{
    static bool result;

    void Because() => result = typeof(TypeWithSingleAttribute).HasAttribute<SerializableAttribute>();

    [Fact] void should_return_true() => result.ShouldBeTrue();
}
