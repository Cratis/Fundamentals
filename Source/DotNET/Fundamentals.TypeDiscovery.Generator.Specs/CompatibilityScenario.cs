// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs;

enum CompatibilityScenario
{
    /// <summary>Source types and accessibility.</summary>
    Types = 0,

    /// <summary>Runtime references and collisions.</summary>
    References = 1,

    /// <summary>Aliases, paths, attributes, and constructor filters.</summary>
    Filters = 2,

    /// <summary>Assemblies depending on Roslyn.</summary>
    BuildTime = 3,

    /// <summary>Convention ambiguity and inheritance.</summary>
    Conventions = 4,

    /// <summary>The synthesized application entry point.</summary>
    TopLevel = 5,

    /// <summary>An assembly with no source types.</summary>
    Empty = 6
}
