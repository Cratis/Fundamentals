// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.for_MeterExtensions;

public class when_beginning_scope_with_anonymous_type_containing_null_values : given.a_meter
{
    IMeterScope<given.a_meter> _scope;

    void Because() => _scope = _meter.BeginScope(new
    {
        NonNullValue = "test",
        NullValue = (string?)null
    });

    [Fact] void should_return_a_scope() => _scope.ShouldNotBeNull();
    [Fact] void should_contain_non_null_value() => _scope.Tags["NonNullValue"].ShouldEqual("test");
    [Fact] void should_not_contain_null_value() => _scope.Tags.ContainsKey("NullValue").ShouldBeFalse();
}
