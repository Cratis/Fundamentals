// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

#pragma warning disable RCS1047

namespace Cratis.Reflection;

/// <summary>
/// Provides a set of methods for working with methods, such as <see cref="MethodInfo"/>.
/// </summary>
public static class MethodExtensions
{
    /// <summary>
    /// Check if a method has an attribute associated with it.
    /// </summary>
    /// <param name="methodInfo">Method to check.</param>
    /// <typeparam name="T">Attribute type to check for.</typeparam>
    /// <returns>True if there is an attribute, false if not.</returns>
    public static bool HasAttribute<T>(this MethodInfo methodInfo)
    where T : Attribute
    {
        var attributes = methodInfo.GetCustomAttributes(typeof(T), false).ToArray();
        return attributes.Length == 1;
    }

    /// <summary>
    /// Check whether or not a <see cref="MethodInfo"/> is async or not.
    /// </summary>
    /// <param name="methodInfo"><see cref="MethodInfo"/> to check.</param>
    /// <returns>True if is async, false if not.</returns>
    public static bool IsAsync(this MethodInfo methodInfo)
    {
        return methodInfo.ReturnType.IsAssignableTo(typeof(Task));
    }

    /// <summary>
    /// Get the actual return type of a method. If the method is async, it will unwrap the type from the task.
    /// </summary>
    /// <param name="methodInfo">Method to get for.</param>
    /// <returns>The actual type.</returns>
    public static Type GetActualReturnType(this MethodInfo methodInfo)
    {
        if (methodInfo.IsAsync())
        {
            return methodInfo.ReturnType.GetGenericArguments()[0];
        }

        return methodInfo.ReturnType;
    }
}
#pragma warning restore
