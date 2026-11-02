// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

[SpecificationResource(Key = "fundamentals-tcp-probe-exclusive", MaxConcurrency = 1)]
sealed class ExclusiveTcpProbeServer() : TcpProbeServerBase("fundamentals-tcp-probe-exclusive")
{
    public override ValueTask Start(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        StartListener();
        ProbeLedger.RecordStart(GetType(), stopwatch.Elapsed);
        return ValueTask.CompletedTask;
    }
}
