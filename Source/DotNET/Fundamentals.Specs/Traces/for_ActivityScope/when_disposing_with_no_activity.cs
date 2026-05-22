// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Traces.for_ActivityScope;

public class when_disposing_with_no_activity : Specification
{
    Exception? _exception;

    void Because()
    {
        try
        {
            new ActivityScope<when_disposing_with_no_activity>(null).Dispose();
        }
        catch (Exception exception)
        {
            _exception = exception;
        }
    }

    [Fact] void should_not_throw() => _exception.ShouldBeNull();
}
