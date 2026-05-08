// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Fundamentals.AOT.Specs.for_InstancesOf;

public class when_resolving_instances : given.a_service_provider
{
    Type[] result;

    void Because() => result =
    [
        ..service_provider.GetRequiredService<IInstancesOf<ISomeInterface>>().Select(_ => _.GetType())
    ];

    [Fact] void should_include_first_implementation() => result.ShouldContain(typeof(FirstImplementation));
    [Fact] void should_include_second_implementation() => result.ShouldContain(typeof(SecondImplementation));
}
