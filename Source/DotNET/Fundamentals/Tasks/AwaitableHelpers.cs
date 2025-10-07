// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Cratis.Tasks;

/// <summary>
/// Helpers for dealing with awaitable objects (Task, ValueTask, etc).
/// </summary>
public static class AwaitableHelpers
{
    const string VoidTaskResultTypeFullName = "System.Threading.Tasks.VoidTaskResult";
    const string TaskResultPropertyName = nameof(Task<int>.Result);
    const string ValueTaskResultPropertyName = nameof(ValueTask<int>.Result);
    const string ValueTaskAsTaskMethodName = nameof(ValueTask<int>.AsTask);

    /// <summary>
    /// Awaits the given object if it is awaitable.
    /// </summary>
    /// <param name="maybeAwaitable">The <see cref="object"/> that is maybe awaitable.</param>
    /// <returns>A <see cref="ValueTask{T}"/> with a tuple of a boolean indicating whether the object was awaitable and the nullable result object.</returns>
    public static async ValueTask<(bool IsAwaitable, object? Result)> AwaitIfNeeded(object? maybeAwaitable)
    {
        if (maybeAwaitable is null)
        {
            return (false, null);
        }

        if (maybeAwaitable is Task task)
        {
            await task.ConfigureAwait(false);
            return (true, GetResultFromAwaitableIfPresent(task, TaskResultPropertyName));
        }

        var type = maybeAwaitable.GetType();

        if (maybeAwaitable is ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
            return (true, null);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var asTaskMethod = type.GetMethod(ValueTaskAsTaskMethodName, BindingFlags.Instance | BindingFlags.Public);
            var taskObj = (Task)asTaskMethod!.Invoke(maybeAwaitable, null)!;
            await taskObj.ConfigureAwait(false);
            return (true, GetResultFromAwaitableIfPresent(taskObj, ValueTaskResultPropertyName));
        }

        return (false, null);
    }

    static object? GetResultFromAwaitableIfPresent(object task, string resultPropertyName)
    {
        var resultProperty = task.GetType().GetProperty(resultPropertyName);
        if (resultProperty is null)
        {
            return null;
        }
        var result = resultProperty.GetValue(task);
        return IsNotVoidType(result?.GetType()) ? result : null;
    }

    static bool IsNotVoidType(Type? type) => type is not null &&
        type != typeof(void) &&
        type.FullName != VoidTaskResultTypeFullName;
}