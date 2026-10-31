// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Cratis.Specifications.Resources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Fundamentals.TypeDiscovery.Generator.Specs;

sealed class CompilationInputs() : ISpecificationResource
{
    static readonly Assembly[] _defaultAssemblies =
    [
        typeof(object).Assembly,
        Assembly.Load("System.Runtime"),
        Assembly.Load("netstandard"),
    ];

    static int _starts;
    readonly ConcurrentDictionary<Assembly, MetadataReference> _additionalReferences = new();

    public ImmutableArray<MetadataReference> DefaultReferences { get; private set; }
    public CSharpCompilationOptions Options { get; private set; } = null!;
    public CSharpParseOptions ParseOptions { get; private set; } = null!;
    internal static int Starts => Volatile.Read(ref _starts);

    public ValueTask Start(CancellationToken cancellationToken)
    {
        DefaultReferences = [.. _defaultAssemblies.Distinct().Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))];
        Options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);
        ParseOptions = CSharpParseOptions.Default;
        Interlocked.Increment(ref _starts);
        return ValueTask.CompletedTask;
    }

    public IEnumerable<MetadataReference> References(Assembly[] additional) => DefaultReferences.Concat(additional
        .Except(_defaultAssemblies)
        .Select(assembly => _additionalReferences.GetOrAdd(assembly, static value => MetadataReference.CreateFromFile(value.Location))));

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
