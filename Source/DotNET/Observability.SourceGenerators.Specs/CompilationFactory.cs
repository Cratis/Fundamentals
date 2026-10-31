// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using Cratis.Specifications.Resources;
using Microsoft.CodeAnalysis.CSharp;

namespace Cratis.Metrics.Roslyn.Specs;

static class CompilationFactory
{
    public static CSharpCompilation CreateCompilation(string source, params Assembly[] additionalAssemblies)
    {
        var inputs = Inputs();
        var syntaxTree = CSharpSyntaxTree.ParseText(source, inputs.ParseOptions);

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            inputs.References(additionalAssemblies),
            inputs.Options);
    }

    static CompilationInputs Inputs()
    {
        var inputs = SpecificationResources.Get<CompilationInputs>();
        return inputs.IsCompletedSuccessfully ? inputs.Result : inputs.AsTask().GetAwaiter().GetResult();
    }
}
