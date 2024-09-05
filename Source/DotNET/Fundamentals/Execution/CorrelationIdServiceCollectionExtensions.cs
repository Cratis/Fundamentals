// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Execution;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class CorrelationIdServiceCollectionExtensions
{
    /// <summary>
    /// Add the <see cref="CorrelationIdAccessor"/> to the services.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> to add to.</param>
    /// <returns><see cref="IServiceCollection"/> for continuation.</returns>
    public static IServiceCollection AddCorrelationIdSupport(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }
}