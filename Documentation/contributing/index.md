# Contributing

Before you start contributing, you should read our guidelines [here](https://github.com/cratis/.github/blob/main/contributing.md).

## Prerequisites

This repository requires the following:

- [.NET Core 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node JS](https://nodejs.org/)

## Build and Test

All C# based projects are added to the solution file at the root level, you can therefor
build it quite easily from root:

```shell
dotnet build
```

Similarly with the specifications you can do:

```shell
dotnet test
```

If you're using an IDE such as Visual Studio, Rider or similar - open the [cratis.sln](../../cratis.sln)
file and do the build / test run from within the IDE.

> Note: if it takes a long time on every build, perform a `dotnet clean` from the project you're working on
> and then `dotnet build` - subsequent builds should then be fast. This is due to the Orleans CodeGenerator being confused
> and hindering incremental builds.

## Static Code Analysis

All projects are built using the same static code analysis rules found [here](https://github.com/cratis/Defaults).
You can find the rule-sets [here](https://github.com/cratis/Defaults/tree/main/Source/Defaults).
