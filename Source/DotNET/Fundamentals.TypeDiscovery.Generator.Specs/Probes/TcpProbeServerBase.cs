// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Cratis.Specifications.Resources;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

abstract class TcpProbeServerBase(string group) : ISpecificationResource<TcpProbeConnection>
{
    readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    readonly CancellationTokenSource _stopping = new();
    readonly ConcurrentBag<TcpClient> _clients = [];
    readonly ConcurrentBag<Task> _echoes = [];
    Task _accepting = Task.CompletedTask;

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    public abstract ValueTask Start(CancellationToken cancellationToken);

    public async ValueTask<TcpProbeConnection> Lease(SpecificationLeaseContext context, CancellationToken cancellationToken)
    {
#pragma warning disable CA2000 // The connection owns the client; the catch disposes it if connecting fails.
        var client = new TcpClient();
#pragma warning restore CA2000
        try
        {
            await client.ConnectAsync(IPAddress.Loopback, Port, cancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            return new(context.LeaseId, client, group);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _stopping.CancelAsync();
            _listener.Stop();
            try
            {
                await _accepting.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (OperationCanceledException)
            {
                // Cancellation ends the accept loop.
            }
            catch (ObjectDisposedException)
            {
                // Stopping the listener ends the accept loop.
            }

            await Task.WhenAll(_echoes).WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            foreach (var client in _clients) client.Dispose();
            _listener.Dispose();
            _stopping.Dispose();
        }
    }

    protected void StartListener()
    {
        _listener.Start();
        _accepting = AcceptConnections();
    }

    async Task AcceptConnections()
    {
        try
        {
            while (!_stopping.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_stopping.Token);
                _clients.Add(client);
                _echoes.Add(Echo(client));
            }
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException && _stopping.IsCancellationRequested)
        {
            // Stopping the listener aborts the pending accept.
        }
    }

    async Task Echo(TcpClient client)
    {
        try
        {
            using var reader = new StreamReader(client.GetStream(), leaveOpen: true);
            await using var writer = new StreamWriter(client.GetStream(), leaveOpen: true) { AutoFlush = true };
            while (await reader.ReadLineAsync(_stopping.Token) is { } line)
            {
                await writer.WriteLineAsync(line.AsMemory(), _stopping.Token);
            }
        }
        catch (OperationCanceledException) when (_stopping.IsCancellationRequested)
        {
            // Shutdown cancels pending reads.
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            // A reset or closed connection ends the echo; it must not fail the teardown.
        }
    }
}
