// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

sealed class PartiallyStartingProbe : ISpecificationResource
{
    static int _disposeCalls;
    static int _listenerStopped;
    readonly TcpListener _listener = new(IPAddress.Loopback, 0);

    public static bool ListenerStopped => Volatile.Read(ref _listenerStopped) == 1;
    public static int DisposeCalls => Volatile.Read(ref _disposeCalls);

    public ValueTask Start(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _listener.Start();
        ProbeLedger.RecordStart(GetType(), stopwatch.Elapsed);
        throw new InvalidOperationException("Probe failed part way through starting");
    }

    public ValueTask DisposeAsync()
    {
        _listener.Stop();
        _listener.Dispose();
        Volatile.Write(ref _listenerStopped, 1);
        Interlocked.Increment(ref _disposeCalls);
        return ValueTask.CompletedTask;
    }
}
