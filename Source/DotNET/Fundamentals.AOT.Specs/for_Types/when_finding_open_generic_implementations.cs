// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_Types;

public class when_finding_open_generic_implementations : given.a_service_provider
{
    Type[] result;

    void Because() => result =
    [
        ..service_provider.GetRequiredService<ITypes>().FindMultiple(typeof(IGenericDemo<>))
    ];

    [Fact] void should_find_generic_demo() => result.ShouldContain(typeof(GenericDemo));
}
