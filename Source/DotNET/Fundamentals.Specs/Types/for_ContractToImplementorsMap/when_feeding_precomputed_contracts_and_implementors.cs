// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap;

public class when_feeding_precomputed_contracts_and_implementors : given.an_empty_map
{
    void Because() => map.Feed(new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IInterface)] = [typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface)]
    });

    [Fact] void should_map_all_implementors_for_the_contract() => map.GetImplementorsFor(typeof(IInterface)).ShouldContainOnly(typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface));
    [Fact] void should_include_all_types_in_the_type_index() => map.All.ShouldContainOnly(typeof(IInterface), typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface));
}