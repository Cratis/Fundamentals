// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Json.for_GeneratedEnumConverters;

public class when_no_provider_supports_the_enum : given.enum_converter_providers
{
    void Because() => result = GeneratedEnumConverters.TryCreate(typeof(DayOfWeek), [provider.Object, later_provider.Object]);

    [Fact] void should_return_null() => result.ShouldBeNull();
}
