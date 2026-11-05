// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

sealed class TcpProbeConnection(string leaseId, TcpClient client, string group) : IAsyncDisposable
{
    readonly StreamReader _reader = new(client.GetStream(), leaveOpen: true);
    readonly StreamWriter _writer = new(client.GetStream(), leaveOpen: true) { AutoFlush = true };

    public string LeaseId { get; } = leaseId;
    public bool HasNewLeaseId { get; } = ProbeLedger.RecordLease(group, leaseId);
    public int HeldAtLease { get; } = ProbeLedger.HeldAtLease(group);

    public async Task<string> Echo(string text, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync(text.AsMemory(), cancellationToken).WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
        return await _reader.ReadLineAsync(cancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(10), cancellationToken) ?? throw new IOException("Probe connection closed before replying");
    }

    public async ValueTask DisposeAsync()
    {
        ProbeLedger.RecordRelease(group);
        await _writer.DisposeAsync();
        _reader.Dispose();
        client.Dispose();
    }
}
