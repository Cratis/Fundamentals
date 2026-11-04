// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

static class ProbeGate
{
    public const string Reason = "Opt-in infrastructure probe: FUNDAMENTALS_SPECS_PROBE (Cratis/Fundamentals#1210)";

    public static ProbeRun Run { get; } = Environment.GetEnvironmentVariable("FUNDAMENTALS_SPECS_PROBE") switch
    {
        "on" => ProbeRun.On,
        "teardown-failure" => ProbeRun.TeardownFailure,
        _ => ProbeRun.Off
    };

    public static bool IsOn => Run is ProbeRun.On or ProbeRun.TeardownFailure;
    public static bool IsTeardownFailure => Run is ProbeRun.TeardownFailure;
}
