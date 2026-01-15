// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reflection.for_ParameterExtensions;

public class when_checking_if_non_nullable_reference_type_method_parameter_is_nullable : Specification
{
    static bool result;

    void Because()
    {
        var method = typeof(ClassWithMethod).GetMethod(nameof(ClassWithMethod.MethodWithParameters))!;
        var parameter = method.GetParameters()[0];
        result = parameter.IsNullable();
    }

    [Fact] void should_return_false() => result.ShouldBeFalse();
}
