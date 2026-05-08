# TypeDiscovery Sample

Demonstrates the AOT-friendly type discovery system built into Cratis Fundamentals. A Roslyn source generator runs at compile time and emits a `GeneratedTypeDiscoveryProvider` that pre-computes all type mappings. At runtime, `ITypes`, `IInstancesOf<T>`, and `IImplementationsOf<T>` use these mappings instead of scanning assemblies via reflection, making the application fully compatible with Native AOT, trimming, and single-file publishing.

## What It Demonstrates

- `ITypes.FindMultiple<T>()` — resolving all implementations of an interface
- `ITypes.FindMultiple(typeof(IGenericDemo<>))` — open-generic contract discovery
- `IImplementationsOf<T>` — type-level enumeration via the DI container
- `IInstancesOf<T>` — instance-level enumeration via the DI container
- `AddBindingsByConvention()` — automatic interface-to-implementation DI bindings
- `AddSelfBindings()` — automatic self-registration of attributed types

## Running in Debug

All commands below assume your current working directory is `Samples/TypeDiscovery`.

```bash
cd Samples/TypeDiscovery
```

```bash
dotnet run
```

Expected output:

```
=== AOT-Friendly Type Discovery Demo ===

Discovered implementations of ISomeInterface:
- TypeDiscovery.FirstImplementation
- TypeDiscovery.SecondImplementation

Discovered implementations of open generic IGenericDemo<>:
- TypeDiscovery.GenericDemo

IImplementationsOf<ISomeInterface> (type-level):
- FirstImplementation
- SecondImplementation

IInstancesOf<ISomeInterface> (instance-level):
- FirstImplementation
- SecondImplementation

Convention binding resolved: ConventionalService

Self binding resolved: ScopedSelfBinding

Total discovered types: <n>
Demo complete.
```

## Building and Publishing (Release — Full AOT)

All commands below assume your current working directory is `Samples/TypeDiscovery`.

The Release configuration enables:

| Setting | Value |
|---|---|
| `PublishAot` | `true` |
| `PublishSingleFile` | `true` |
| `PublishTrimmed` | `true` |
| `TrimMode` | `full` |
| `InvariantGlobalization` | `true` |
| `StripSymbols` | `true` |
| `IlcOptimizationPreference` | `Speed` |

### One-command build + run (macOS/Linux)

Use the helper script to detect the current OS/architecture, publish with Native AOT, and execute the produced binary:

```bash
chmod +x run-aot.sh
./run-aot.sh
```

The script publishes with `-f net10.0` and runs the native executable directly from `bin/Release/net10.0/<rid>/publish/TypeDiscovery`.

The script supports:

- macOS arm64 (`osx-arm64`)
- macOS x64 (`osx-x64`)
- Linux x64 (`linux-x64`)
- Linux arm64 (`linux-arm64`)

Publish for your platform by passing a Runtime Identifier (`-r`):

```bash
# macOS (Apple Silicon)
dotnet publish TypeDiscovery.csproj -c Release -f net10.0 -r osx-arm64 --self-contained

# macOS (Intel)
dotnet publish TypeDiscovery.csproj -c Release -f net10.0 -r osx-x64 --self-contained

# Linux x64
dotnet publish TypeDiscovery.csproj -c Release -f net10.0 -r linux-x64 --self-contained

# Linux arm64
dotnet publish TypeDiscovery.csproj -c Release -f net10.0 -r linux-arm64 --self-contained

# Windows x64
dotnet publish TypeDiscovery.csproj -c Release -f net10.0 -r win-x64 --self-contained
```

The output is a single self-contained native executable under `bin/Release/net10.0/<rid>/publish/`.

> **Note:** Native AOT compilation requires the native toolchain for the target platform — on macOS that means Xcode command-line tools, on Linux a C compiler (`gcc`/`clang`), and on Windows Visual Studio Build Tools with the C++ workload.

## How the Generator Works

The `Fundamentals.TypeDiscovery.Generator` Roslyn incremental generator inspects the compilation at build time and emits two artifacts into the consuming assembly:

1. **`GeneratedTypeDiscoveryProvider`** — a class that implements both `ICanProvideAssembliesForDiscovery` and `ICanProvideContractToImplementorsForDiscovery`. It contains a pre-computed list of every concrete type in the assembly and a dictionary mapping each contract (interface or base class, including open generics) to its implementors.

2. **`GeneratedTypeDiscoveryProviderRegistration`** — a `[ModuleInitializer]` that registers the provider into `GeneratedTypeDiscoveryRegistry` the moment the assembly is loaded.

When `Types` is constructed it checks `GeneratedTypeDiscoveryRegistry`. If providers are present, it uses the pre-computed mappings directly — no assembly scanning, no reflection over attribute lists. This is what makes the zero-overhead, AOT-safe path possible.
