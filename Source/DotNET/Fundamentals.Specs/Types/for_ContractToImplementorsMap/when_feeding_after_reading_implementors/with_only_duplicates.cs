// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap.when_feeding_after_reading_implementors;

public class with_only_duplicates : given.an_empty_map
{
    IEnumerable<Type> _snapshot;

    void Establish()
    {
        map.Feed([typeof(ImplementationOfInterface)]);
        _snapshot = map.GetImplementorsFor<IInterface>();
    }

    void Because() => map.Feed([typeof(ImplementationOfInterface)]);

    [Fact] void should_reuse_the_unchanged_snapshot() => map.GetImplementorsFor<IInterface>().ShouldBeSame(_snapshot);
    [Fact] void should_reuse_the_snapshot_for_dictionary_reads() => map.ContractsAndImplementors[typeof(IInterface)].ShouldBeSame(_snapshot);
}
