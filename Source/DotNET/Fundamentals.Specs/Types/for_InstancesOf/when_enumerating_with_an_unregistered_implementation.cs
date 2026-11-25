// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_InstancesOf;

public class when_enumerating_with_an_unregistered_implementation : Specification
{
    Mock<ITypes> type_finder;
    Mock<IServiceProvider> container;
    OneImplementation one_implementation_instance;
    List<IAmAnInterface> yielded;
    Exception exception;

    void Establish()
    {
        type_finder = new();
        type_finder.Setup(t => t.FindMultiple<IAmAnInterface>()).Returns([typeof(OneImplementation), typeof(SecondImplementation)]);
        one_implementation_instance = new OneImplementation();
        container = new();
        container.Setup(c => c.GetService(typeof(OneImplementation))).Returns(one_implementation_instance);
        container.Setup(c => c.GetService(typeof(SecondImplementation))).Returns(null);
        yielded = [];
    }

    void Because() => exception = Catch.Exception(() => yielded.AddRange(new InstancesOf<IAmAnInterface>(type_finder.Object, container.Object)));

    [Fact] void should_fail_with_invalid_operation() => exception.ShouldBeOfExactType<InvalidOperationException>();
    [Fact] void should_name_the_unresolved_implementation() => exception.Message.ShouldContain(nameof(SecondImplementation));
    [Fact] void should_yield_the_instances_before_the_unresolved_one() => yielded.ShouldContainOnly(one_implementation_instance);
}
