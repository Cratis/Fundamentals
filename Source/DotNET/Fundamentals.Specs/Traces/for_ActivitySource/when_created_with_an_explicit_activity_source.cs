// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces.for_ActivitySource;

public class when_created_with_an_explicit_activity_source : Specification
{
    System.Diagnostics.ActivitySource _actualSource;
    ActivitySource<when_created_with_an_explicit_activity_source> _source;

    void Establish() => _actualSource = new("CustomSource");

    void Because() => _source = new(_actualSource);

    [Fact] void should_expose_the_provided_activity_source() => _source.ActualSource.ShouldEqual(_actualSource);
}
