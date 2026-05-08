// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap;

public class when_feeding_precomputed_contracts_and_implementors_with_duplicates : given.an_empty_map
{
    void Because() => map.Feed(new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IInterface)] = [typeof(ImplementationOfInterface), typeof(ImplementationOfInterface)]
    });

    [Fact] void should_keep_unique_implementors_only() => map.GetImplementorsFor(typeof(IInterface)).ShouldContainOnly(typeof(ImplementationOfInterface));
}