// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Execution;

namespace Cratis.Concepts.for_IGeneratable;

public class when_creating_a_correlation_id : Specification
{
    CorrelationId _result;

    void Because() => _result = Create<CorrelationId>();

    [Fact] void should_create_a_non_empty_value() => (_result != CorrelationId.NotSet).ShouldBeTrue();

    static T Create<T>()
        where T : IGeneratable<T> => T.New();
}
