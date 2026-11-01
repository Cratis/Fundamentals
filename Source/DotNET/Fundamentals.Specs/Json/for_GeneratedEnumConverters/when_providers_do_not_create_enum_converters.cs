// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_GeneratedEnumConverters;

public class when_providers_do_not_create_enum_converters : given.enum_converter_providers
{
    void Because() => result = GeneratedEnumConverters.TryCreate(typeof(DayOfWeek), [non_factory.Object]);

    [Fact] void should_return_null() => result.ShouldBeNull();
}
