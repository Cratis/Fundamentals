// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics.for_MeterExtensions;

public class when_beginning_scope_with_dictionary : given.a_meter
{
    IMeterScope<given.a_meter> _scope;
    IDictionary<string, object> _tags;

    void Establish()
    {
        _tags = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42
        };
    }

    void Because() => _scope = _meter.BeginScope(_tags);

    [Fact] void should_return_a_scope() => _scope.ShouldNotBeNull();
    [Fact] void should_contain_key1_tag() => _scope.Tags["Key1"].ShouldEqual("Value1");
    [Fact] void should_contain_key2_tag() => _scope.Tags["Key2"].ShouldEqual(42);
}
