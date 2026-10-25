// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Collections;
using Cratis.DependencyInjection;
using Cratis.Types;
using Microsoft.Extensions.DependencyInjection;

namespace TypeDiscovery;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== AOT-Friendly Type Discovery Demo ===");

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

        var dependent = serviceProvider.GetRequiredService<IDependentService>();
        Console.WriteLine($"Discovery-only convention binding resolved: {dependent.Describe()}");

        var selfBoundType = types.All.Single(type => type.Name == "OnlySelfBound");
        var selfBound = serviceProvider.GetRequiredService(selfBoundType);
        Console.WriteLine($"Discovery-only self binding resolved: {selfBound.GetType().Name}");

        Console.WriteLine();
        Console.WriteLine($"Total discovered types: {types.All.Count()}");
        Console.WriteLine("Demo complete.");
    }
}
