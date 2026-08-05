// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Tracing;

namespace Cratis.Types;

/// <summary>
/// Reports what type discovery captured, at the moment it captured it.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Types.Instance"/> is a static field initializer in a leaf package: there is no container,
/// no configuration and no <c>ILoggerFactory</c> at that point, and taking a logging dependency there
/// would buy an initialization-order hazard for a line of text. An event source needs none of that, is
/// free when nothing is listening, and a host that wants these in its log can bridge them.
/// </para>
/// <para>
/// Listen with <c>dotnet-trace collect --providers Cratis.Fundamentals.TypeDiscovery</c>, or from code
/// with an <see cref="EventListener"/>.
/// </para>
/// </remarks>
[EventSource(Name = SourceName)]
internal sealed class TypeDiscoveryEventSource : EventSource
{
    /// <summary>
    /// The name to enable this source under.
    /// </summary>
    public const string SourceName = "Cratis.Fundamentals.TypeDiscovery";

    /// <summary>
    /// Gets the singleton instance to write through.
    /// </summary>
    public static readonly TypeDiscoveryEventSource Log = new();

    /// <summary>
    /// Reports the assemblies a type universe was built from.
    /// </summary>
    /// <param name="discoveryMode">The strategy that chose them.</param>
    /// <param name="assemblies">The assemblies, comma separated.</param>
    [Event(1, Level = EventLevel.Informational, Message = "Type universe built by {0} discovery from: {1}")]
    public void UniverseBuilt(string discoveryMode, string assemblies) => WriteEvent(1, discoveryMode, assemblies);

    /// <summary>
    /// Reports a type universe that reached nothing beyond this package.
    /// </summary>
    /// <param name="discoveryMode">The strategy that produced it.</param>
    /// <param name="assemblies">The assemblies it did reach, comma separated.</param>
    /// <remarks>
    /// Never a legitimate state for an application: it means nothing the application owns was
    /// discovered, so every convention-based lookup will come back empty and none of them will say so.
    /// The usual cause is a project that reaches this package through a project reference rather than
    /// the package, which does not flow the generator that would have registered its own provider.
    /// </remarks>
    [Event(2, Level = EventLevel.Warning, Message = "Type universe built by {0} discovery contains only {1} - nothing this application owns was discovered")]
    public void UniverseContainsOnlyThisPackage(string discoveryMode, string assemblies) => WriteEvent(2, discoveryMode, assemblies);
}
