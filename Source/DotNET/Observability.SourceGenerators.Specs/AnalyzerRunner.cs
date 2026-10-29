// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cratis.Metrics.Roslyn.Specs;

static class AnalyzerRunner
{
    public static ImmutableArray<Diagnostic> Run(string source, params DiagnosticAnalyzer[] analyzers)
    {
        var compilation = CompilationFactory.CreateCompilation(source);
        return compilation.WithAnalyzers([.. analyzers]).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
    }
}
