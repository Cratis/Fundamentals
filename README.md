# Fundamentals

The shared building blocks beneath the Cratis stack — concepts, serialization, dependency injection, and type discovery for .NET and TypeScript, with typed concepts and codecs for Go.

## Packages

[![Nuget](https://img.shields.io/nuget/v/Cratis.Fundamentals?logo=nuget)](http://nuget.org/packages/cratis.fundamentals)
[![NPM](https://img.shields.io/npm/v/@cratis/fundamentals?label=@cratis/fundamentals&logo=npm)](https://www.npmjs.com/package/@cratis/fundamentals)
[![MIT License](https://img.shields.io/badge/license-MIT-blue)](./LICENSE)

## Builds

[![.NET Build](https://github.com/cratis/Fundamentals/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/cratis/Fundamentals/actions/workflows/dotnet-build.yml)
[![JavaScript Build](https://github.com/cratis/Fundamentals/actions/workflows/javascript-build.yml/badge.svg)](https://github.com/cratis/Fundamentals/actions/workflows/javascript-build.yml)
[![Documentation site](https://github.com/Cratis/Documentation/actions/workflows/docs-site.yml/badge.svg)](https://github.com/Cratis/Documentation/actions/workflows/docs-site.yml)

## Description

The Cratis Fundamentals holds generic reusable helpers, utilities and tools that aims at solving common problems and help developers be more productive.
Fundamentals offers functionality for .NET and JavaScript environments. It is not a goal to have parity, as the different environments offer different
building blocks.

You should look at it as a convenience layer on top of the existing base environment you're running in.

Fundamentals underpins the rest of the Cratis stack: [Chronicle](https://github.com/Cratis/Chronicle) (the event-sourcing database and runtime), [Arc](https://github.com/Cratis/Arc) (the CQRS application framework for ASP.NET Core), and [Components](https://github.com/Cratis/Components) (the React component library) all build on it — and you can use it on its own.

## Start with your platform

* **[C# / .NET](https://www.cratis.io/fundamentals/csharp/)**: give domain values distinct types with `ConceptAs<T>`, discover services by convention, or represent absence and failure explicitly.
* **[TypeScript](https://www.cratis.io/fundamentals/typescript/)**: use GUIDs, serialization, property-path helpers, and decorators.
* **[Go](https://www.cratis.io/fundamentals/go/)**: use typed concepts and scalar codecs, correlation, naming, and optional dependency injection.

The APIs differ by platform; a feature in the .NET package is not a promise of a TypeScript or Go equivalent. You can use Fundamentals without Arc or Chronicle.

### Try the .NET package on its own

With a .NET 10 SDK, create a console project and install the package:

```bash
dotnet new console --framework net10.0 --name FundamentalsDemo
cd FundamentalsDemo
dotnet add package Cratis.Fundamentals --version 7.32.0
```

Replace `Program.cs` with this complete example:

```csharp
using System;
using Cratis.Monads;

public static class QuickStart
{
    public static void Main()
    {
        Option<string> timeout = "30";
        Console.WriteLine(timeout.Match(
            value => $"Configured: {value}",
            _ => "Use the default timeout"));
    }
}
```

`Option<string>` represents either text or an explicit absence. `Match` handles both cases and returns the selected message. Run it:

```bash
dotnet run
```

Expected output:

```text
Configured: 30
```

Continue with the [Option, Result, and Catch chapter](https://www.cratis.io/fundamentals/csharp/monads/) to look up an absent setting, reject invalid input with a typed error, and adapt a throwing parser. These APIs live in `Cratis.Monads` in the core package; they need no host or running service.

Browse the [Fundamentals documentation](https://www.cratis.io/fundamentals/) for the other building blocks.

## Support

Cratis is an open community, and we are glad to help users, teams evaluating the stack, and contributors.

| Channel | Details |
|---|---|
| Discord | Join the community on [Discord](https://discord.gg/kt4AMpV8WV) for questions and discussions |
| GitHub Issues | [Report bugs or request features](https://github.com/Cratis/Fundamentals/issues) |
| Documentation | Read the docs at [cratis.io](https://www.cratis.io) |

## Contributing

If you want to jump into building this repository and possibly contributing, please refer to [contributing](https://www.cratis.io/contributing/contributing/).

### Prerequisites

The following are prerequisites to work with this repository.

* [.NET SDK 10.0.100](https://dotnet.microsoft.com/en-us/) or a later feature band, as selected by `global.json` (roll-forward policy `latestFeature`).
* [Node 23+](https://nodejs.org/en) (`engines.node` is `>=23.0.0` in `package.json`).
* [Yarn](https://yarnpkg.com)

Consumers of the published libraries are not affected by these contributor requirements; the libraries target net8.0, net9.0, and net10.0.

### Running .NET specifications

The specifications use Cratis.Specifications 5.0 on the native xUnit v3 runner and require .NET 10.
Specification projects target net10.0 only; libraries still build and pack for net8.0, net9.0, and net10.0.

From the repository root, build once and run the five specification executables in parallel:

```bash
dotnet build -c Release
bash .github/scripts/run-specifications.sh Release
```

Logs and TRX results go under `TestResults/`. The script fails if a project is missing, fails, or runs zero tests.
Use `bash .github/scripts/run-specifications.sh --self-test` to check the runner's failure guards.

For a single project, run its executable directly (append `.exe` on Windows):

```bash
Source/DotNET/Fundamentals.Specs/bin/Release/net10.0/Cratis.Fundamentals.Specs
```

Use `-method`, `-class`, or `-namespace` with a fully qualified name to filter the executable's tests;
run it with `-?` to see all options. For example:

```bash
Source/DotNET/Fundamentals.Specs/bin/Release/net10.0/Cratis.Fundamentals.Specs -namespace 'Cratis.Concepts*'
```

`dotnet test -c Release --no-build` and IDE test explorers continue to use the VSTest adapter.
They do not require Microsoft.Testing.Platform configuration. For Debug, build with `dotnet build` and run `dotnet test --no-build`.
AOT.Specs is skipped by `dotnet test`; run it as an executable through `bash .github/scripts/run-specifications.sh Release` (also covered by the native-aot CI job).

### Central Package Management

This repository leverages [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management), which
means that all package versions are managed from a file at the root level called [Directory.Packages.props](./Directory.Packages.props).

In addition there are also [Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022#directorybuildprops-and-directorybuildtargets) files for
setting up common settings that are applied cross cuttingly.

### Root package.json

The `package.json` found at the root level defines all the workspaces. It is assumed

All developer dependencies are defined in the top level `package.json`. The reason for this is to be able to provide global scripts
for every package to use for easier maintenance.

The `package.json` found at the top level contains scripts that can then be used in a child project for this to work properly.

In a package, all you need to do is to define the scripts to use the global scripts in the `package.json´ of that project:

```json
{
    "scripts": {
        "prepublish": "yarn g:build",
        "clean": "yarn g:clean",
        "build": "yarn g:build",
        "lint": "yarn g:lint",
        "lint:ci": "yarn g:lint:ci",
        "test": "yarn g:test",
        "ci": "yarn g:ci",
        "up": "yarn g:up"
    }
}
```

## The Cratis ecosystem

This project is part of the [Cratis Platform](https://www.cratis.io) — tools for building event-sourced and CQRS applications.

* **[Chronicle](https://github.com/Cratis/Chronicle)** — event-sourcing database and runtime. Orleans-based kernel, pluggable storage (MongoDB default; PostgreSQL, SQL Server, SQLite, in-memory), language-agnostic gRPC contracts. [Docs](https://www.cratis.io/chronicle/)
* **Chronicle clients** — first-class [.NET SDK](https://github.com/Cratis/Chronicle), plus [TypeScript](https://github.com/Cratis/Chronicle.TypeScript), [Kotlin/Java](https://github.com/Cratis/Chronicle.Kotlin), and [Elixir](https://github.com/Cratis/Chronicle.Elixir); [Python](https://github.com/Cratis/Chronicle.Python) coming soon. AI agents connect through the [Chronicle MCP server](https://github.com/Cratis/Chronicle.Mcp).
* **[Arc](https://github.com/Cratis/Arc)** — opinionated CQRS framework for ASP.NET Core with commands, queries, validation, authorization, and TypeScript proxy generation. Works without event sourcing. [Docs](https://www.cratis.io/arc/)
* **[Components](https://github.com/Cratis/Components)** — React components aligned with Arc patterns. [Docs](https://www.cratis.io/components/)
* **[CLI](https://github.com/Cratis/cli) + Workbench** — inspect and diagnose Chronicle from the terminal or the browser. [Docs](https://www.cratis.io/cli/)
* **Model-first layer** — Studio, [Screenplay](https://github.com/Cratis/Screenplay), [Stage](https://github.com/Cratis/Stage), [Scene](https://github.com/Cratis/Scene), [Prologue](https://github.com/Cratis/Prologue)
* **Supporting** — [Fundamentals](https://github.com/Cratis/Fundamentals), [Specifications](https://github.com/Cratis/Specifications), [Synopsis](https://github.com/Cratis/Synopsis), [Lens](https://github.com/Cratis/Lens), [Narrator](https://github.com/Cratis/Narrator), and free [AI tooling](https://github.com/Cratis/AI); Ensemble coming soon
* **[Samples](https://github.com/Cratis/Samples)** — runnable event sourcing and CQRS samples for the whole stack

Fundamentals is MIT licensed and free to use.

Join the community on [Discord](https://discord.gg/kt4AMpV8WV).

Release notes and announcements: the [Cratis blog](https://blog.cratis.io).
