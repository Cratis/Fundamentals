// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Reactive.for_TransformingSubject;

public class when_upstream_disposal_throws : Specification
{
    List<int> releases;
    List<Exception> failures;
    List<Exception> repeatFailures;
    List<Exception> lateFailures;

    void Because()
    {
        releases = [];
        failures = [];
        repeatFailures = [];
        lateFailures = [];
        foreach (var scoped in new[] { false, true })
        {
            var source = new given.tracked_source { onRelease = () => throw new InvalidOperationException("release failed") };
            var subject = scoped
                ? TransformingSubject<int, int>.CreateSubscriberScoped(() => source, value => value)
                : new TransformingSubject<int, int>(source, value => value);
            using var consumer = subject.Subscribe(_ => { });
            failures.Add(Catch.Exception(subject.Dispose));
            repeatFailures.Add(Catch.Exception(subject.Dispose));
            lateFailures.Add(Catch.Exception(() => subject.Subscribe(_ => { })));
            releases.Add(source.releases);
        }
    }

    [Fact] void should_attempt_all_owned_releases() => releases.ShouldEqual([2, 1]);
    [Fact] void should_surface_the_disposal_failure() => failures.Select(error => error.GetType()).ShouldEqual([typeof(InvalidOperationException), typeof(InvalidOperationException)]);
    [Fact] void should_not_retry_a_throwing_disposable() => repeatFailures.ShouldEqual([null, null]);
    [Fact] void should_still_release_replay() => lateFailures.Select(error => error.GetType()).ShouldEqual([typeof(ObjectDisposedException), typeof(ObjectDisposedException)]);
}
