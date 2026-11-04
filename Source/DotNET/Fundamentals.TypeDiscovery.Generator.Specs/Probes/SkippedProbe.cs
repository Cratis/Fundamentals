// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

sealed class SkippedProbe : ISpecificationResource
{
    public ValueTask Start(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        ProbeLedger.RecordStart(GetType(), stopwatch.Elapsed);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
