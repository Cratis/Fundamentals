// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs.Probes;

public sealed class ProbeFactAttribute : FactAttribute
{
    public ProbeFactAttribute(ProbeRun run = ProbeRun.On, [CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        var enabled = run switch
        {
            ProbeRun.Off => ProbeGate.Run is ProbeRun.Off,
            ProbeRun.On => ProbeGate.IsOn,
            ProbeRun.TeardownFailure => ProbeGate.IsTeardownFailure,
            _ => false
        };
        if (!enabled) Skip = ProbeGate.Reason;
    }
}
