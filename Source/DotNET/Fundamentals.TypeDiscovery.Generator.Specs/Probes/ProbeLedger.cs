// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Text.Json;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

static class ProbeLedger
{
    static readonly ConcurrentDictionary<string, int> _starts = new();
    static readonly ConcurrentDictionary<string, double> _startMs = new();
    static readonly ConcurrentDictionary<string, byte> _leaseIds = new();
    static readonly ConcurrentDictionary<string, int> _held = new();
    static readonly ConcurrentDictionary<string, int> _peaks = new();
    static int _leases;

    static ProbeLedger()
    {
        var path = Environment.GetEnvironmentVariable("FUNDAMENTALS_SPECS_PROBE_LEDGER");
        if (!string.IsNullOrEmpty(path))
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) => File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                starts = _starts,
                startMs = _startMs,
                leases = Leases,
                peakLeases = _peaks
            }));
        }
    }

    public static int TotalStarts => _starts.Values.Sum();
    public static int Leases => Volatile.Read(ref _leases);

    public static void RecordStart(Type type, TimeSpan startDuration)
    {
        _starts.AddOrUpdate(type.Name, 1, static (_, count) => count + 1);
        _startMs.AddOrUpdate(type.Name, static (_, ms) => ms, static (_, duration, ms) => duration + ms, startDuration.TotalMilliseconds);
    }

    public static bool RecordLease(string group, string leaseId)
    {
        Interlocked.Increment(ref _leases);
        var held = _held.AddOrUpdate(group, 1, static (_, count) => count + 1);
        _peaks.AddOrUpdate(group, static (_, current) => current, static (_, peak, current) => Math.Max(peak, current), held);
        return _leaseIds.TryAdd(leaseId, 0);
    }

    public static void RecordRelease(string group) => _held.AddOrUpdate(group, 0, static (_, count) => count - 1);
    public static int StartsOf<T>() => _starts.GetValueOrDefault(typeof(T).Name);
    public static int HeldAtLease(string group) => _held.GetValueOrDefault(group);
    public static int PeakLeases(string group) => _peaks.GetValueOrDefault(group);
}
