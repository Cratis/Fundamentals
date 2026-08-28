# Fundamentals

The shared building blocks beneath the Cratis stack — concepts, serialization, dependency injection, and type discovery for .NET and TypeScript.

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

Read more about how to use it in the [Fundamentals documentation](https://www.cratis.io/fundamentals/).

## Support

Cratis is an open community, and we are glad to help users, teams evaluating the stack, and contributors.

| Channel | Details |
|---|---|
| Discord | Join the community on [Discord](https://discord.gg/kt4AMpV8WV) for questions and discussions |
| GitHub Issues | [Report bugs or request features](https://github.com/Cratis/Fundamentals/issues) |
| Documentation | Read the docs at [cratis.io](https://cratis.io) |

## Contributing

If you want to jump into building this repository and possibly contributing, please refer to [contributing](./Documentation/contributing/index.md).

### Prerequisites

The following are prerequisites to work with this repository.

* [.NET 6+](https://dotnet.microsoft.com/en-us/).
* [Node 16+](https://nodejs.org/en)
* [Yarn](https://yarnpkg.com)

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

This project is part of [Cratis](https://www.cratis.io) — free, MIT-licensed tools for building event-sourced and CQRS applications.

- **[Chronicle](https://github.com/Cratis/Chronicle)** — event-sourcing database and runtime. Orleans-based kernel, pluggable storage (MongoDB default; PostgreSQL, SQL Server, SQLite, in-memory), language-agnostic gRPC contracts. [Docs](https://www.cratis.io/chronicle/)
- **Chronicle clients** — first-class [.NET SDK](https://github.com/Cratis/Chronicle), plus [TypeScript](https://github.com/Cratis/Chronicle.TypeScript), [Kotlin/Java](https://github.com/Cratis/Chronicle.Kotlin), and [Elixir](https://github.com/Cratis/Chronicle.Elixir); [Python](https://github.com/Cratis/Chronicle.Python) coming soon (pre-alpha). AI agents connect through the [Chronicle MCP server](https://github.com/Cratis/Chronicle.Mcp).
- **[Arc](https://github.com/Cratis/Arc)** — opinionated CQRS framework for ASP.NET Core with commands, queries, validation, authorization, and TypeScript proxy generation. Works without event sourcing. [Docs](https://www.cratis.io/arc/)
- **[Components](https://github.com/Cratis/Components)** — React components aligned with Arc patterns. [Docs](https://www.cratis.io/components/)
- **[CLI](https://github.com/Cratis/cli) + Workbench** — inspect and diagnose Chronicle from the terminal or the browser. [Docs](https://www.cratis.io/cli/)
- **Model-first layer (experimental)** — Studio, [Screenplay](https://github.com/Cratis/Screenplay), [Stage](https://github.com/Cratis/Stage), [Scene](https://github.com/Cratis/Scene), [Prologue](https://github.com/Cratis/Prologue)
- **Supporting** — [Fundamentals](https://github.com/Cratis/Fundamentals), [Specifications](https://github.com/Cratis/Specifications), [Synopsis](https://github.com/Cratis/Synopsis), [Lens](https://github.com/Cratis/Lens), [Narrator](https://github.com/Cratis/Narrator), and free [AI tooling](https://github.com/Cratis/AI) (preview); Ensemble coming soon (pre-release)
- **[Samples](https://github.com/Cratis/Samples)** — runnable event sourcing and CQRS samples for the whole stack

Everything Cratis publishes today is MIT licensed and free to use.

Join the community on [Discord](https://discord.gg/kt4AMpV8WV).

Release notes and announcements: the [Cratis blog](https://blog.cratis.io).
