// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

sealed class SlowStartingProbe : ISpecificationResource
{
    public static readonly TaskCompletionSource Starting = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public static readonly TaskCompletionSource Released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsStarted { get; private set; }

    public static void Release() => Released.TrySetResult();

    public async ValueTask Start(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        Starting.TrySetResult();
        await Released.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
        IsStarted = true;
        ProbeLedger.RecordStart(GetType(), stopwatch.Elapsed);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
