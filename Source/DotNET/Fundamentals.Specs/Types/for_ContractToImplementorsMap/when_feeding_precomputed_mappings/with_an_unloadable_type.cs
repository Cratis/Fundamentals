// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_ContractToImplementorsMap.when_feeding_precomputed_mappings;

public class with_an_unloadable_type : given.an_empty_map
{
    Exception _error;

    void Because() => _error = Catch.Exception(() => map.Feed(new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IInterface)] = [typeof(ImplementationOfInterface), new an_unloadable_type(), typeof(SecondImplementationOfInterface)]
    }));

    [Fact] void should_not_fail() => _error.ShouldBeNull();

    [Fact]
    void should_map_only_the_loadable_implementors() =>
        map.GetImplementorsFor<IInterface>().ShouldContainOnly(typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface));

    sealed class an_unloadable_type() : TypeDelegator(typeof(ImplementationOfInterface))
    {
        protected override TypeAttributes GetAttributeFlagsImpl() => throw new TypeLoadException("Simulated missing referenced type");
    }
}
