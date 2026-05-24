// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using DiagnosticsActivitySource = System.Diagnostics.ActivitySource;

namespace Cratis.Traces;

/// <summary>
/// Represents a typed named <see cref="DiagnosticsActivitySource"/>.
/// </summary>
/// <typeparam name="T">Type the activity source is for.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="NamedActivitySource{T}"/> class.
/// </remarks>
/// <param name="activitySource">The named activity source resolved by inheriting the parent keyed service key.</param>
public class NamedActivitySource<T>([FromKeyedServices] DiagnosticsActivitySource activitySource) : IActivitySource<T>
{
    /// <inheritdoc/>
    public DiagnosticsActivitySource ActualSource { get; } = activitySource;
}
