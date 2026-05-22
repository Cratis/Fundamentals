// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces.for_ActivitySource;

public class when_created_without_an_explicit_activity_source : Specification
{
    ActivitySource<when_created_without_an_explicit_activity_source> _source;

    void Because() => _source = new();

    [Fact] void should_create_an_actual_activity_source() => _source.ActualSource.ShouldNotBeNull();
    [Fact] void should_name_the_activity_source_after_the_generic_type() => _source.ActualSource.Name.ShouldEqual(typeof(when_created_without_an_explicit_activity_source).FullName);
}
