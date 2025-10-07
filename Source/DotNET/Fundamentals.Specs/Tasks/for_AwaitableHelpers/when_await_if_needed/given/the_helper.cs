// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Tasks;

namespace Cratis.Tasks.for_AwaitableHelpers.when_await_if_needed.given;


public class the_helper : Specification
{
    protected object? Input;
    protected (bool IsAwaitable, object? Result) Result;

    protected async Task AwaitIfNeeded() => Result = await AwaitableHelpers.AwaitIfNeeded(Input);
}