---
title: Microsoft library guidance
description: Look up bounded Fundamentals evidence, unassessed areas, and existing policy departures from Microsoft library guidance.
---

<!-- Copyright (c) Cratis. All rights reserved. -->
<!-- Licensed under the MIT license. See LICENSE file in the project root for full license information. -->

Use this checklist to distinguish configured behavior from compatibility guarantees when choosing Fundamentals packages. It covers selected .NET library, options, and logging recommendations, not TypeScript or every public API. The source links identify the assessed revision; this is not a repository-wide compliance certification.

## Status meanings

| Status | Meaning |
| --- | --- |
| conforming | The stated example supports the named recommendation within the row's scope. |
| partial | Some relevant evidence matches, but limits or gaps remain. |
| not-assessed | No conclusion has been established. An omission is not an exemption. |
| authorized waiver | An intentional departure is grounded in an existing applicable policy or explicit maintainer authorization. |

## Library and package reach

Microsoft's [library guidance][library-guidance] covers inclusiveness, stability, evolution, debugging, and trust. Its [cross-platform targeting guidance][targeting-guidance] recommends starting with `net8.0` or later, while distinguishing modern .NET targeting from breadth targeting.

| Recommendation or concern | Status | Fundamentals evidence and limits |
| --- | --- | --- |
| Include a modern .NET target | conforming | The shared [build properties][build-targets] configure Release for `net8.0;net9.0;net10.0` and Debug for `net10.0`. This is a configured-target observation for runtime projects that inherit these settings, not proof of published assets or platform support. |
| Distinguish runtime libraries from generators | partial | [Cratis.Fundamentals][fundamentals-project] provides the base library and bundles the type-discovery generator. [Cratis.OpenTelemetry][telemetry-project] separately references hosting, exporter, and instrumentation packages. The [type-discovery generator][discovery-project] and [metrics generator][metrics-project] override the targets to `netstandard2.0` and configure analyzer package paths. Their targets do not extend the runtime library's reach or establish independently published artifacts. |
| Cross-platform, trimming, and AOT compatibility | not-assessed | `IsAotCompatible` in [Fundamentals.csproj][fundamentals-project] is a declaration, not an AOT or trimming audit. Runtime execution across operating systems and published-package compatibility have not been established by this checklist. |
| Debuggability, package trust, and evolution | not-assessed | [Symbol/source properties][symbol-properties] configure embedded sources, symbols, and `snupkg`. Published symbols and package contents, metadata, signing, versioning, binary compatibility, and transitive dependency reach are not assessed. |

## Options

Microsoft's [options guidance for library authors][options-guidance] describes registration patterns for configuration and callbacks. Use the existing [OpenTelemetry setup guide](opentelemetry.md) for host integration.

| Recommendation or concern | Status | Fundamentals evidence and limits |
| --- | --- | --- |
| Expose consumer configuration and options registration | partial | [OpenTelemetryExtensions][telemetry-options] accepts an optional `Action<CratisOpenTelemetryOptions>` and offers an explicit `IConfiguration` overload. It directly creates the public [application options][application-options], while registering SDK options through `AddOptions` and consuming SDK `IOptions<T>`. These are distinct: the callback is not evidence that `CratisOpenTelemetryOptions` itself has a complete `IOptions<T>` registration contract. |
| Configuration binding, validation, and updates | not-assessed | The assessed overload documents standard OTEL keys, not a named application-options section. Named-section conventions, reload/change notifications, and comprehensive validation against Microsoft's options patterns remain unassessed. No additional overload or binding contract is implied. |

## Logging

Microsoft's [logging guidance for library authors][logging-guidance] discusses logger/factory selection when emitting logs and recommends source-generated logging for most situations. Configuring a host pipeline is different from emitting library logs.

| Recommendation or concern | Status | Fundamentals evidence and limits |
| --- | --- | --- |
| Structured, categorized, source-generated library logs | not-assessed | The applicable [C# logging policy][logging-policy] requires named structured parameters, `ILogger<T>`, and a separate partial logging class using `LoggerMessage`. This is policy, not an implementation survey. The [OpenTelemetry pipeline][telemetry-logging] enables scopes and formatted messages and conditionally configures export; it does not establish universal library-log adherence. |
| Bootstrap diagnostics without a host logger | partial | [TypeDiscoveryEventSource][discovery-diagnostics] emits type-universe diagnostics through `EventSource`. Its documented boundary is static initialization in a leaf package, before a container, configuration, or `ILoggerFactory` exists; taking a logging dependency there would introduce an initialization-order hazard. A host can bridge the events. This rationale is not a blanket waiver from Microsoft's logging guidance. |

## Existing exception-policy departure

The supplemental Microsoft [standard exception guidance][exception-guidance] recommends `ArgumentException` or a subtype for bad arguments and `InvalidOperationException` for inappropriate object state. This is separate framework design guidance, not an additional rule inferred from the three library-author entry points above.

| Recommendation or concern | Status | Scoped departure and authority |
| --- | --- | --- |
| Use standard exception types for argument and state errors | authorized waiver | The applicable [C# exception policy][exception-policy] instead requires custom types derived from `Exception`, prohibits built-in exception types, and requires meaningful messages. Its rationale is to name what went wrong in domain terms. [InvalidOpenTelemetryEndpoint][endpoint-exception] is a concrete custom type for an invalid endpoint. The waiver records this standing policy departure, not universal source adherence: the [keyed-service fallback][builtin-exception] also throws a built-in `InvalidOperationException`. |

This checklist does not authorize new exceptions to policy. Maintainer approval of the checklist and its intentional departures remains pending; documenting the standing exception policy does not complete that approval. Reconsider the exception row if maintainers change the policy or the public error contract.

## Keep the checklist current

Reassess affected rows when public configuration, logging, target frameworks, or package behavior changes. New departures require maintainer approval. Keep unexamined areas `not-assessed` rather than extending an example into a repository-wide guarantee.

[library-guidance]: https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/
[targeting-guidance]: https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting
[options-guidance]: https://learn.microsoft.com/en-us/dotnet/core/extensions/options-library-authors
[logging-guidance]: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-library-authors
[exception-guidance]: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types
[build-targets]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Directory.Build.props#L25-L35
[symbol-properties]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Directory.Build.props#L7-L12
[fundamentals-project]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/Fundamentals/Fundamentals.csproj#L1-L33
[telemetry-project]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/OpenTelemetry/OpenTelemetry.csproj#L1-L14
[discovery-project]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/Fundamentals.TypeDiscovery.Generator/Fundamentals.TypeDiscovery.Generator.csproj#L1-L26
[metrics-project]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/Metrics.Roslyn/Metrics.Roslyn.csproj#L1-L40
[telemetry-options]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/OpenTelemetry/OpenTelemetryExtensions.cs#L25-L123
[application-options]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/OpenTelemetry/CratisOpenTelemetryOptions.cs#L10-L41
[logging-policy]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/.cratis/ai/rules/csharp.md#L315-L320
[telemetry-logging]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/OpenTelemetry/OpenTelemetryExtensions.cs#L73-L167
[discovery-diagnostics]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/Fundamentals/Types/TypeDiscoveryEventSource.cs#L8-L56
[exception-policy]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/.cratis/ai/rules/csharp.md#L118-L177
[endpoint-exception]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/OpenTelemetry/InvalidOpenTelemetryEndpoint.cs#L6-L10
[builtin-exception]: https://github.com/Cratis/Fundamentals/blob/eb3728436ac6c3c7a3c78359b7a2b25e31195443/Source/DotNET/OpenTelemetry/OpenTelemetryExtensions.cs#L332-L334
