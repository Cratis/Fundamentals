// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using Cratis.Collections;
using Cratis.DependencyInjection;
using Cratis.Types;
using Microsoft.Extensions.DependencyInjection;

namespace TypeDiscovery;

internal static class Program
{
    public static void Main()
    {
        PreserveConstructorsForConventionBasedActivation();

        Console.WriteLine("=== AOT-Friendly Type Discovery Demo ===");

        // Keep explicit references so analyzers understand these types are intentionally used via discovery/conventions.
        var referencedTypes = new Type[]
        {
            typeof(FirstImplementation),
            typeof(SecondImplementation),
            typeof(ConventionalService),
            typeof(ScopedSelfBinding),
            typeof(GenericDemo)
        };

        var services = new ServiceCollection();
        services.AddTypeDiscovery();
        services.AddBindingsByConvention();
        services.AddSelfBindings();
        var serviceProvider = services.BuildServiceProvider();

        var types = serviceProvider.GetRequiredService<ITypes>();

        Console.WriteLine();
        Console.WriteLine("Discovered implementations of ISomeInterface:");
        types.FindMultiple<ISomeInterface>().ForEach(type => Console.WriteLine($"- {type.FullName}"));

        Console.WriteLine();
        Console.WriteLine("Discovered implementations of open generic IGenericDemo<>:");
        types.FindMultiple(typeof(IGenericDemo<>)).ForEach(type => Console.WriteLine($"- {type.FullName}"));

        Console.WriteLine();
        Console.WriteLine("IImplementationsOf<ISomeInterface> (type-level):");
        var implementationTypes = serviceProvider.GetRequiredService<IImplementationsOf<ISomeInterface>>();
        implementationTypes.ForEach(type => Console.WriteLine($"- {type.Name}"));

        Console.WriteLine();
        Console.WriteLine("IInstancesOf<ISomeInterface> (instance-level):");
        var instances = serviceProvider.GetRequiredService<IInstancesOf<ISomeInterface>>();
        instances.ForEach(instance => Console.WriteLine($"- {instance.GetType().Name}"));

        Console.WriteLine();
        var conventional = serviceProvider.GetRequiredService<IConventionalService>();
        Console.WriteLine($"Convention binding resolved: {conventional.Describe()}");

        using var scope = serviceProvider.CreateScope();
        var scopedSelfBinding = scope.ServiceProvider.GetRequiredService<ScopedSelfBinding>();
        Console.WriteLine($"Self binding resolved: {scopedSelfBinding.Describe()}");

        Console.WriteLine();
        Console.WriteLine($"Total discovered types: {types.All.Count()}");
        Console.WriteLine("Demo complete.");
        GC.KeepAlive(referencedTypes);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(FirstImplementation))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(SecondImplementation))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ConventionalService))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ScopedSelfBinding))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(GenericDemo))]
    static void PreserveConstructorsForConventionBasedActivation()
    {
    }
}
