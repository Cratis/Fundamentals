// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_ContractToImplementorsMap;

public class when_feeding_a_type_that_cannot_be_inspected : given.an_empty_map
{
    Exception error;

    void Because() => error = Catch.Exception(() => map.Feed(
    [
        typeof(ImplementationOfInterface),
        new type_that_throws_on_inspection(),
        typeof(SecondImplementationOfInterface)
    ]));

    [Fact] void should_not_fail() => error.ShouldBeNull();
    [Fact] void should_still_map_the_loadable_implementations() =>
        map.GetImplementorsFor(typeof(IInterface)).ShouldContainOnly(typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface));

    /// <summary>
    /// A type whose base/interface inspection throws <see cref="TypeLoadException"/>, simulating a
    /// referenced assembly that resolves to a version no longer containing a referenced type.
    /// </summary>
    sealed class type_that_throws_on_inspection() : TypeDelegator(typeof(ImplementationOfInterface))
    {
        public override Type[] GetInterfaces() => throw new TypeLoadException("Simulated missing referenced type");
    }
}
