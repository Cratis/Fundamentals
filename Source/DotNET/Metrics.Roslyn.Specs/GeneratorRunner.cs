// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs;

static class GeneratorRunner
{
    public static GeneratorDriverRunResult Run(string source)
    {
        var compilation = CompilationFactory.CreateCompilation(source);
        var generator = new MetricsSourceGenerator();
        return CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation)
            .GetRunResult();
    }
}
