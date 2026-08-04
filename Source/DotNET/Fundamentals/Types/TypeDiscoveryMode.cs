// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Types;

/// <summary>
/// Represents how the assemblies behind a <see cref="ITypes"/> were arrived at.
/// </summary>
/// <remarks>
/// The choice is made once, when the type universe is built, and holds for the lifetime of that
/// <see cref="ITypes"/>. It is reported because the two automatic modes have different failure
/// characteristics: <see cref="Generated"/> sees exactly the assemblies whose module initializers had
/// run by then, while <see cref="Reflected"/> walks the dependency closure and does not care what is
/// loaded.
/// </remarks>
public enum TypeDiscoveryMode
{
    /// <summary>
    /// The assembly providers were supplied by the caller, so neither automatic strategy was chosen.
    /// </summary>
    Explicit = 0,

    /// <summary>
    /// Compile-time generated providers were registered and used exclusively; the reflection fallback
    /// was skipped.
    /// </summary>
    /// <remarks>
    /// A generated provider registers itself from a module initializer, which runs when the CLR loads
    /// its assembly. The universe therefore holds the assemblies that had been loaded at that moment,
    /// and an assembly loaded later contributes nothing. <see cref="ITypes.Assemblies"/> is the list of
    /// the ones that did contribute.
    /// </remarks>
    Generated = 1,

    /// <summary>
    /// No generated providers were registered, so the project- and package-reference closures were
    /// walked by reflection.
    /// </summary>
    /// <remarks>
    /// This does not currently occur. Cratis.Fundamentals is itself built with the type discovery
    /// generator, so its own module initializer registers a provider as the CLR loads the assembly -
    /// which necessarily happens before anything can reach <see cref="Types"/> at all. The registry is
    /// therefore never empty and the fallback is never taken. The value stays because the fallback is
    /// still there and still correct; if the generator ever stops running over this assembly, this is
    /// what a consumer would see.
    /// </remarks>
    Reflected = 2
}
