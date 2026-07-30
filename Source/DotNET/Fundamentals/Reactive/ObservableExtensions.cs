// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Cratis.Reactive;

/// <summary>
/// Holds extension methods for <see cref="ISubject{T}"/>.
/// </summary>
public static class ObservableExtensions
{
    /// <summary>
    /// Completes the subject when the <see cref="CancellationToken"/> is cancelled.
    /// </summary>
    /// <typeparam name="TResult">Type of result.</typeparam>
    /// <param name="subject">The subject to complete.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>The subject.</returns>
    public static ISubject<TResult> CompletedBy<TResult>(this ISubject<TResult> subject, CancellationToken cancellationToken)
    {
        cancellationToken.Register(subject.OnCompleted);
        return subject;
    }

    /// <summary>
    /// Invokes an action and wraps the observable result in a subject providing, with a cancellation token passed to the action that cancels when the subject is completed.
    /// </summary>
    /// <param name="instance">The instance to extend.</param>
    /// <param name="action">The action to invoke that returns an observable.</param>
    /// <typeparam name="TResult">Type of result.</typeparam>
    /// <returns>The subject.</returns>
    /// <remarks>
    /// The returned subject replays its most recent value to whoever subscribes to it. Callers wrap an observable
    /// here and hand the subject on to a consumer that subscribes later, so a source emitting synchronously while
    /// it is being subscribed - a <see cref="BehaviorSubject{T}"/> seed, a <see cref="ReplaySubject{T}"/> buffer,
    /// anything backed by in-memory state - would otherwise emit into a subject nobody is listening to yet and the
    /// value would be lost for good.
    /// </remarks>
#pragma warning disable RCS1175 // Unused 'this' parameter
#pragma warning disable IDE0060 // Remove unused parameter
    public static ISubject<TResult> InvokeAndWrapWithSubject<TResult>(this object instance, Func<CancellationToken, IObservable<TResult>> action)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1175 // Unused 'this' parameter
        => WrapWithReplayingSubject(action, observable => observable);

    /// <summary>
    /// Invokes an action and wraps the observable result in a subject providing, with a cancellation token passed to the action that cancels when the subject is completed.
    /// </summary>
    /// <param name="instance">The instance to extend.</param>
    /// <param name="action">The action to invoke that returns an observable.</param>
    /// <param name="transform">The transform to apply to the observable result.</param>
    /// <typeparam name="TResult">Type of result.</typeparam>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <returns>The subject.</returns>
    /// <remarks>
    /// The returned subject replays its most recent value - see <see cref="InvokeAndWrapWithSubject{TResult}"/> for why.
    /// </remarks>
#pragma warning disable RCS1175 // Unused 'this' parameter
#pragma warning disable IDE0060 // Remove unused parameter
    public static ISubject<TResult> InvokeAndWrapWithTransformSubject<TResult, TSource>(this object instance, Func<CancellationToken, IObservable<TSource>> action, Func<TSource, TResult> transform)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1175 // Unused 'this' parameter
        => WrapWithReplayingSubject(action, observable => observable.Select(transform));

    static ReplaySubject<TResult> WrapWithReplayingSubject<TResult, TSource>(
        Func<CancellationToken, IObservable<TSource>> action,
        Func<IObservable<TSource>, IObservable<TResult>> compose)
    {
        // Both the token source and the subscription live for as long as the returned subject does and are
        // disposed when it completes, which is beyond the scope the analyzer can see.
#pragma warning disable CA2000 // Dispose objects before losing scope
        var cts = new CancellationTokenSource();
        var subject = new ReplaySubject<TResult>(1);

        // Assigned after the completion handler is wired up, so a source that completes synchronously still gets
        // its subscription disposed - SingleAssignmentDisposable disposes whatever is assigned to it afterwards.
        var subscription = new SingleAssignmentDisposable();
#pragma warning restore CA2000 // Dispose objects before losing scope

        subject.Subscribe(_ => { }, _ => { }, () =>
        {
            subscription.Dispose();
            cts.Cancel();
            cts.Dispose();
        });

        subscription.Disposable = compose(action(cts.Token)).Subscribe(subject);
        return subject;
    }
}
