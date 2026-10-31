// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap.when_feeding_precomputed_mappings_concurrently;

public class with_duplicate_implementors : given.an_empty_map
{
    void Because() => Parallel.For(0, 64, _ => map.Feed(new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IInterface)] = [typeof(ImplementationOfInterface), typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface)]
    }));

    [Fact]
    void should_keep_each_implementor_once() =>
        map.GetImplementorsFor<IInterface>().ShouldContainOnly(typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface));

    [Fact] void should_keep_the_type_index_unique() => map.All.Count().ShouldEqual(3);
}
