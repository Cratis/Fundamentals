// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_ContractToImplementorsMap.when_feeding_framework_types;

public class with_reflected_types : given.framework_types
{
    void Because() => map.Feed(types);

    [Fact]
    void should_preserve_the_previous_contracts() =>
        map.ContractsAndImplementors.Keys.ShouldContainOnly([.. previousMappings.Keys]);

    [Fact] void should_preserve_the_previous_implementor_sets() => AssertImplementorsMatch();

    [Fact] void should_index_all_types() => map.All.ShouldContainOnly(types);
}
