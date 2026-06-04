// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DiagnosticsActivitySource = System.Diagnostics.ActivitySource;

namespace Cratis.Traces;

/// <summary>
/// Represents an unkeyed typed <see cref="DiagnosticsActivitySource"/>.
/// </summary>
/// <typeparam name="T">Type the activity source is for.</typeparam>
public class UnkeyedActivitySource<T> : IActivitySource<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnkeyedActivitySource{T}"/> class.
    /// </summary>
    public UnkeyedActivitySource()
    {
        ActualSource = new(typeof(T).FullName ?? typeof(T).Name);
    }

    /// <inheritdoc/>
    public DiagnosticsActivitySource ActualSource { get; }
}