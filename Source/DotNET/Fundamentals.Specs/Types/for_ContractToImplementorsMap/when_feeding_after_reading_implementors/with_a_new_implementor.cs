// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap.when_feeding_after_reading_implementors;

public class with_a_new_implementor : given.an_empty_map
{
    IEnumerable<Type> _snapshot;
    IEnumerable<Type> _dictionarySnapshot;

    void Establish()
    {
        map.Feed([typeof(ImplementationOfInterface)]);
        _snapshot = map.GetImplementorsFor<IInterface>();
        _dictionarySnapshot = map.ContractsAndImplementors[typeof(IInterface)];
    }

    void Because() => map.Feed(new Dictionary<Type, IEnumerable<Type>>
    {
        [typeof(IInterface)] = [typeof(SecondImplementationOfInterface)]
    });

    [Fact] void should_keep_the_original_snapshot_stable() => _snapshot.ShouldContainOnly(typeof(ImplementationOfInterface));
    [Fact] void should_keep_the_dictionary_snapshot_stable() => _dictionarySnapshot.ShouldContainOnly(typeof(ImplementationOfInterface));
    [Fact]
    void should_include_the_new_implementor_in_subsequent_reads() =>
        map.GetImplementorsFor<IInterface>().ShouldContainOnly(typeof(ImplementationOfInterface), typeof(SecondImplementationOfInterface));
    [Fact] void should_not_expose_a_mutable_collection() => ((ICollection<Type>)_snapshot).IsReadOnly.ShouldBeTrue();
}
