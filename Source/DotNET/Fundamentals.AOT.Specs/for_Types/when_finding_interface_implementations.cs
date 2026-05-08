// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_Types;

public class when_finding_interface_implementations : given.a_service_provider
{
    Type[] result;

    void Because() => result =
    [
        ..service_provider.GetRequiredService<ITypes>().FindMultiple<ISomeInterface>()
    ];

    [Fact] void should_find_first_implementation() => result.ShouldContain(typeof(FirstImplementation));
    [Fact] void should_find_second_implementation() => result.ShouldContain(typeof(SecondImplementation));
}
