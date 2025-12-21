// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Metrics;

/// <summary>
/// Extension methods for working with meters.
/// </summary>
public static class MeterExtensions
{
    /// <summary>
    /// Begin a scope for a meter.
    /// </summary>
    /// <param name="meter">The meter to begin scope for.</param>
    /// <param name="tags">The tags associated with the scope.</param>
    /// <typeparam name="T">Type the scope is for.</typeparam>
    /// <returns>A new <see cref="IMeterScope{T}"/>.</returns>
    public static IMeterScope<T> BeginScope<T>(this IMeter<T> meter, IDictionary<string, object> tags)
    {
        return new MeterScope<T>(meter, tags);
    }

    /// <summary>
    /// Begin a scope for a meter using an anonymous type for tags.
    /// </summary>
    /// <param name="meter">The meter to begin scope for.</param>
    /// <param name="tags">An anonymous object containing the tags. Property names become tag keys and property values become tag values.</param>
    /// <typeparam name="T">Type the scope is for.</typeparam>
    /// <returns>A new <see cref="IMeterScope{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">The exception that is thrown when tags is null.</exception>
    public static IMeterScope<T> BeginScope<T>(this IMeter<T> meter, object tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var tagsDictionary = new Dictionary<string, object>();

        foreach (var property in tags.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            var value = property.GetValue(tags);
            if (value is not null)
            {
                tagsDictionary[property.Name] = value;
            }
        }

        return new MeterScope<T>(meter, tagsDictionary);
    }
}
