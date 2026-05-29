// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using DiagnosticsActivitySource = System.Diagnostics.ActivitySource;

namespace Cratis.Traces;

/// <summary>
/// Represents a typed <see cref="DiagnosticsActivitySource"/>.
/// </summary>
/// <typeparam name="T">Type the activity source is for.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ActivitySource{T}"/> class.
/// </remarks>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/> used for resolving keyed services.</param>
/// <param name="key">The key used when resolving the service.</param>
public class ActivitySource<T>(IServiceProvider serviceProvider, [ServiceKey] string? key = null) : IActivitySource<T>
{
    /// <inheritdoc/>
    public DiagnosticsActivitySource ActualSource { get; } = key is null
        ? new(typeof(T).FullName ?? typeof(T).Name)
        : serviceProvider.GetRequiredKeyedService<DiagnosticsActivitySource>(key);
}
