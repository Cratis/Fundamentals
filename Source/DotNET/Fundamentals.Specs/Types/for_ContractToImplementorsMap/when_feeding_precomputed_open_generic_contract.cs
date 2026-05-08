// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap;

public class when_feeding_precomputed_open_generic_contract : given.an_empty_map
{
    void Because() => map.Feed(new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IGenericInterface<>)] = [typeof(GenericImplementationOfInterface)]
    });

    [Fact] void should_resolve_implementor_for_open_generic_contract() => map.GetImplementorsFor(typeof(IGenericInterface<>)).ShouldContainOnly(typeof(GenericImplementationOfInterface));
}