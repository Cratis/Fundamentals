// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces.for_ActivityScope;

public class when_disposing : Specification
{
    System.Diagnostics.Activity _activity;
    ActivityScope<when_disposing> _scope;

    void Establish()
    {
        _activity = new System.Diagnostics.Activity("test").Start();
        _scope = new ActivityScope<when_disposing>(_activity);
    }

    void Because() => _scope.Dispose();

    [Fact] void should_stop_the_activity() => System.Diagnostics.Activity.Current.ShouldBeNull();
}
