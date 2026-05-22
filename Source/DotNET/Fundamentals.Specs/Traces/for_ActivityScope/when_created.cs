// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces.for_ActivityScope;

public class when_created : Specification
{
    System.Diagnostics.Activity _activity;
    ActivityScope<when_created> _scope;

    void Establish() => _activity = new System.Diagnostics.Activity("test");

    void Because() => _scope = new(_activity);

    [Fact] void should_expose_the_activity() => _scope.Activity.ShouldEqual(_activity);
}
