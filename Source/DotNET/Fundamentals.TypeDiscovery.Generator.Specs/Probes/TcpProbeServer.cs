// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

sealed class TcpProbeServer() : TcpProbeServerBase("tcp-probe")
{
    public override ValueTask Start(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        StartListener();
        ProbeLedger.RecordStart(GetType(), stopwatch.Elapsed);
        return ValueTask.CompletedTask;
    }
}
