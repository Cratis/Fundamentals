// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Types.for_Types;

/// <summary>
/// A universe holding only this package is never legitimate for an application: every convention-based
/// lookup comes back empty, and an empty result is indistinguishable from a feature nobody wrote. One
/// comparison catches it, where checking against the reference closure would mean running the scan the
/// generated providers exist to avoid.
/// </summary>
public class when_a_universe_reaches_nothing_beyond_this_package : given.a_listener_for_type_discovery_events
{
    only_this_package _provider;

    void Establish() => _provider = new only_this_package();

    void Because() => _ = new Types([_provider]);

    [Fact] void should_warn() => EventsNaming(PackageName).Any(_ => _.Id == 2).ShouldBeTrue();
    [Fact] void should_say_which_mode_produced_it() => EventsNaming(PackageName).First(_ => _.Id == 2).Payload[0].ShouldEqual(nameof(TypeDiscoveryMode.Explicit));
    [Fact] void should_still_report_that_it_was_built() => EventsNaming(PackageName).Any(_ => _.Id == 1).ShouldBeTrue();

    static string PackageName => typeof(Types).Assembly.GetName().Name!;

    class only_this_package : ICanProvideAssembliesForDiscovery
    {
        public IEnumerable<Assembly> Assemblies => [typeof(Types).Assembly];

        public IEnumerable<Type> DefinedTypes => [];

        public void Initialize()
        {
        }
    }
}
