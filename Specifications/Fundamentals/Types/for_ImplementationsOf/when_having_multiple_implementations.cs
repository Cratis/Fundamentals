// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ImplementationsOf;

public class when_having_multiple_implementations : Specification
{
    Mock<ITypes> type_finder_mock;
    Type[] instances;

    void Establish()
    {
        type_finder_mock = new();
        type_finder_mock.Setup(t => t.FindMultiple<IAmAnInterface>()).Returns(
        [
                typeof(OneImplementation),
                typeof(SecondImplementation)
        ]);
    }

    void Because() => instances = new ImplementationsOf<IAmAnInterface>(type_finder_mock.Object).ToArray();

    [Fact] void should_get_the_implementations() => instances.ShouldContainOnly([typeof(OneImplementation), typeof(SecondImplementation)]);
}
