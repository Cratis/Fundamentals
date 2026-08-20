// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;
using System.Reflection;

namespace Cratis.Types.for_Types.given;

public class a_listener_for_type_discovery_events : Specification
{
    protected capturing_listener _listener;

    void Establish() => _listener = new capturing_listener();

    void Destroy() => _listener.Dispose();

    /// <summary>
    /// The captured events whose assembly list names the given assembly.
    /// </summary>
    /// <param name="assemblyName">The assembly to attribute events to.</param>
    /// <returns>The events naming it.</returns>
    /// <remarks>
    /// The event source is process wide and xUnit runs test classes in parallel, so a listener sees
    /// every universe built while it is attached, not only the one its own spec built. Attributing by
    /// the assembly list is what keeps an assertion about this spec's universe from being answered by
    /// somebody else's.
    /// </remarks>
    protected IEnumerable<(int Id, string Name, string[] Payload)> EventsNaming(string assemblyName) =>
        _listener.Events.Where(_ => _.Payload.Length > 1 && _.Payload[1].Contains(assemblyName, StringComparison.Ordinal));

    /// <summary>
    /// Captures what the type discovery event source writes, the way a host bridging it into its own
    /// logging would. The source is enabled by name so nothing else in the process is collected.
    /// </summary>
    protected sealed class capturing_listener : EventListener
    {
        readonly List<(int Id, string Name, string[] Payload)> _events = [];
#pragma warning disable MA0158, IDE0330 // Cannot use Lock in .net 8
        readonly object _lock = new();
#pragma warning restore

        public IEnumerable<(int Id, string Name, string[] Payload)> Events
        {
            get
            {
                lock (_lock)
                {
                    return [.. _events];
                }
            }
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name.Equals(TypeDiscoveryEventSourceName, StringComparison.Ordinal))
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            lock (_lock)
            {
                _events.Add((eventData.EventId, eventData.EventName ?? string.Empty, [.. eventData.Payload!.Select(_ => _?.ToString() ?? string.Empty)]));
            }
        }
    }

    /// <summary>
    /// The source name, read off the internal event source rather than copied, so a rename is caught here.
    /// </summary>
    static string TypeDiscoveryEventSourceName =>
        (string)typeof(Types).Assembly
            .GetType("Cratis.Types.TypeDiscoveryEventSource")!
            .GetField("SourceName", BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null)!;
}
