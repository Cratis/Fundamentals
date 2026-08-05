// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types.for_Types;

public class when_a_universe_is_built : given.a_listener_for_type_discovery_events
{
    plain_assembly_provider _provider;

    void Establish() => _provider = new plain_assembly_provider();

    void Because() => _ = new Types([_provider]);

    [Fact] void should_report_that_it_was_built() => _reported.ShouldNotBeEmpty();
    [Fact] void should_name_the_discovery_mode() => _reported[0].Payload[0].ShouldEqual(nameof(TypeDiscoveryMode.Explicit));
    [Fact] void should_not_warn_about_it() => _warned.ShouldBeEmpty();

    /// <summary>
    /// The build events for the universe this spec built. Attributed, because the event source is
    /// process wide and xUnit runs classes in parallel - an unattributed "nothing warned" assertion
    /// was answered by a concurrent spec's universe about one run in ten.
    /// </summary>
    (int Id, string Name, string[] Payload)[] _reported => [.. EventsNaming(AssemblyName).Where(_ => _.Id == 1)];

    (int Id, string Name, string[] Payload)[] _warned => [.. EventsNaming(AssemblyName).Where(_ => _.Id == 2)];

    static string AssemblyName => typeof(plain_assembly_provider).Assembly.GetName().Name!;
}
