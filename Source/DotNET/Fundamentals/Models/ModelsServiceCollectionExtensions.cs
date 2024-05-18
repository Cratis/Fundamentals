// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Models;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/> for adding model name conventions.
/// </summary>
public static class ModelsServiceCollectionExtensions
{
    /// <summary>
    /// Add the default model name convention.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddDefaultModelNameConvention(this IServiceCollection services) =>
        services
            .AddSingleton<IModelNameConvention, DefaultModelNameConvention>()
            .AddSingleton<IModelNameResolver, ModelNameResolver>();

    /// <summary>
    /// Add the namespaced model name convention.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <param name="segmentsToSkip">Optionally number of segments in the namespace to skip. Defaults to 0.</param>
    /// <param name="separator">Optional separator character to use between namespace segments. Defaults to '-'.</param>
    /// <param name="prefix">Optional prefix to prepend all model names with.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddNamespaceModelNameConvention(this IServiceCollection services, int segmentsToSkip = 0, char separator = '-', string prefix = "") =>
        services
            .AddSingleton<IModelNameConvention>(new NamespacedModelNameConvention(segmentsToSkip, separator, prefix))
            .AddSingleton<IModelNameResolver, ModelNameResolver>();

    /// <summary>
    /// Add a specific model name convention.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IModelNameConvention"/> to use.</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddModelNameConvention<T>(this IServiceCollection services)
        where T : class, IModelNameConvention =>
        services
            .AddSingleton<IModelNameConvention, T>()
            .AddSingleton<IModelNameResolver, ModelNameResolver>();
}
