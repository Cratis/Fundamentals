// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace Cratis.Types.for_ContractToImplementorsMap.when_feeding_framework_types;

public class with_concurrent_reads_and_feeds : given.framework_types
{
    readonly ConcurrentQueue<Type[]> _snapshots = new();

    void Because() => Parallel.ForEach(types.Chunk(32), chunk =>
    {
        map.Feed(chunk);
        _snapshots.Enqueue([.. map.GetImplementorsFor(typeof(IComparable))]);
    });

    [Fact]
    void should_preserve_the_previous_contracts() =>
        map.ContractsAndImplementors.Keys.ShouldContainOnly([.. previousMappings.Keys]);

    [Fact] void should_preserve_the_previous_implementor_sets() => AssertImplementorsMatch();

    [Fact]
    void should_return_unique_implementors_during_mapping()
    {
        foreach (var snapshot in _snapshots)
        {
            snapshot.Distinct().Count().ShouldEqual(snapshot.Length);
        }
    }
}
