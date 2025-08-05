// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Serialization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IServiceCollection"/> for adding model name conventions.
/// </summary>
public static class NamingPolicyCollectionExtensions
{
    /// <summary>
    /// Add the default model name convention.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddDefaultNamingPolicy(this IServiceCollection services) =>
        services
            .AddSingleton<INamingPolicy, DefaultNamingPolicy>();

    /// <summary>
    /// Add the default model name convention.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <param name="pluralizeReadModelNames">Whether to pluralize the read model names. Defaults to true.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddCamelCaseNamingPolicy(this IServiceCollection services, bool pluralizeReadModelNames = true) =>
        services
            .AddSingleton<INamingPolicy>(new CamelCaseNamingPolicy(pluralizeReadModelNames));

    /// <summary>
    /// Add the namespaced model name convention.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <param name="segmentsToSkip">Optionally number of segments in the namespace to skip. Defaults to 0.</param>
    /// <param name="separator">Optional separator character to use between namespace segments. Defaults to '-'.</param>
    /// <param name="prefix">Optional prefix to prepend all model names with.</param>
    /// <param name="camelCase">Whether to apply camel case to the names.</param>
    /// <param name="pluralizeReadModelNames">Whether to pluralize the read model names. Defaults to true.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddNamespaceNamingPolicy(
        this IServiceCollection services,
        int segmentsToSkip = 0,
        char separator = '-',
        string prefix = "",
        bool camelCase = false,
        bool pluralizeReadModelNames = true) =>
            services.AddSingleton<INamingPolicy>(new NamespacedNamingPolicy(segmentsToSkip, separator, prefix, camelCase, pluralizeReadModelNames));

    /// <summary>
    /// Add a specific model name convention.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="INamingPolicy"/> to use.</typeparam>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuing build.</returns>
    public static IServiceCollection AddNamingPolicy<T>(this IServiceCollection services)
        where T : class, INamingPolicy =>
        services
            .AddSingleton<INamingPolicy, T>();
}
