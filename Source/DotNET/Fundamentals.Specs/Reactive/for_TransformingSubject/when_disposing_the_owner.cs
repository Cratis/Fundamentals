// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_disposing_the_owner : Specification
{
    given.tracked_source source;
    int independentValues;

    void Because()
    {
        source = new();
        using var independent = source.Subscribe(_ => independentValues++);
        var subject = new TransformingSubject<int, int>(source, value => value);
        subject.Dispose();
        subject.Dispose();
        source.OnNext(1);
    }

    [Fact] void should_release_both_owned_subscriptions_and_the_independent_handle() => source.releases.ShouldEqual(3);
    [Fact] void should_leave_the_independent_observer_alive_until_its_own_disposal() => independentValues.ShouldEqual(1);
    [Fact] void should_never_dispose_the_source() => source.sourceDisposals.ShouldEqual(0);
}
