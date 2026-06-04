// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs;

static class CompilationFactory
{
    static readonly Assembly[] _defaultAssemblies =
    [
        typeof(object).Assembly,
        Assembly.Load("System.Runtime"),
        Assembly.Load("netstandard"),
        typeof(System.Diagnostics.ActivitySource).Assembly,
        typeof(Traces.SpanAttribute).Assembly,
    ];

    public static CSharpCompilation CreateCompilation(string source, params Assembly[] additionalAssemblies)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        MetadataReference[] references =
        [
            .. _defaultAssemblies
                .Concat(additionalAssemblies)
                .Distinct()
                .Select(static asm => (MetadataReference)MetadataReference.CreateFromFile(asm.Location))
        ];

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }
}
