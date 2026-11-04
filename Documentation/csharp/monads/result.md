---
description: Return success or a typed error from .NET operations with the Result monad instead of using exceptions for control flow.
---
# Result

`Result` represents success or an expected failure. Use it when the caller needs to act on an operation's outcome without treating a predictable rejection, such as an invalid order, as an exception.

## Purpose

Choose the variant according to the information the caller needs:

- `Result` reports only success or failure, as in `ValidateOrder`.
- `Result<TError>` adds a typed failure reason, as in `Validate`, whose `ValidationError` names the invalid field.
- `Result<TResult, TError>` returns either a success value or a typed error, as in `ConfirmOrder`, which returns an `OrderConfirmation` or an `OrderError`.

These are alternative contracts, not stages of one operation. Returning a `Result` also does not catch exceptions thrown while performing the operation.

## Usage

### Producer Perspective

The order examples progress from a pass/fail check to a validation explanation and then a confirmation value. They omit the application-specific order types and repository setup; the classes illustrate the return contracts rather than a complete application.

```csharp
// Simple Result
public class OrderProcessor
{
    public Result ValidateOrder(Order order)
    {
        if (order.Items.Count == 0)
            return Result.Failed();
        
        if (order.TotalAmount <= 0)
            return Result.Failed();
        
        return Result.Success();
    }
}

// Result with typed error
public record ValidationError(string Field, string Message);

public class OrderValidator
{
    public Result<ValidationError> Validate(Order order)
    {
        if (order.Items.Count == 0)
            return Result.Failed(new ValidationError("Items", "Order must have at least one item"));
        
        if (order.TotalAmount <= 0)
            return Result.Failed(new ValidationError("TotalAmount", "Total must be greater than zero"));
        
        return Result.Success<ValidationError>();
    }
}

// Result with both result and error types
public record OrderConfirmation(OrderId Id, DateTime ConfirmedAt);
public record OrderError(string Code, string Description);

public class OrderService
{
    public Result<OrderConfirmation, OrderError> ConfirmOrder(OrderId orderId)
    {
        var order = _repository.FindById(orderId);
        if (order is null)
            return Result.Failed<OrderConfirmation, OrderError>(
                new OrderError("NOT_FOUND", "Order not found"));
        
        if (order.Status != OrderStatus.Pending)
            return new OrderError("INVALID_STATE", $"Cannot confirm order in {order.Status} state");
        
        order.Status = OrderStatus.Confirmed;
        _repository.Save(order);
        
        // Implicit conversion from TResult to Result<TResult, TError>
        return new OrderConfirmation(orderId, DateTime.UtcNow);
    }
}
```

`OrderProcessor.ValidateOrder` returns only whether the order passes its checks. `OrderValidator.Validate` checks the same conditions but supplies a field and message on failure. `OrderService.ConfirmOrder` needs a different contract: it can explain a missing order or an invalid state, and on success it saves the confirmed order and returns its ID and confirmation time.

### Consumer Perspective

The controller methods illustrate separate handling choices for these outcomes. They are not a sequence of steps: several methods call `ConfirmOrder` themselves. Controller initialization and application helpers such as email delivery and error logging are omitted.

```csharp
public class OrderController
{
    readonly OrderService _orderService;
    readonly OrderValidator _validator;
    
    public void ProcessOrder(Order order)
    {
        // Result<TError>: check success without inspecting the error
        var validationResult = _validator.Validate(order);
        if (!validationResult.IsSuccess)
        {
            Console.WriteLine("Order validation failed");
            return;
        }
        
        Console.WriteLine("Order is valid");
    }
    
    public void ValidateAndDisplay(Order order)
    {
        // Result<TError>
        var result = _validator.Validate(order);
        
        if (result.IsSuccess)
        {
            Console.WriteLine("Validation passed");
        }
        else if (result.TryGetError(out var error))
        {
            Console.WriteLine($"Validation failed: {error.Field} - {error.Message}");
        }
    }
    
    public void ConfirmOrder(OrderId orderId)
    {
        // Result<TResult, TError>
        var result = _orderService.ConfirmOrder(orderId);
        
        if (result.TryGetResult(out var confirmation))
        {
            Console.WriteLine($"Order confirmed: {confirmation.Id} at {confirmation.ConfirmedAt}");
        }
        else if (result.TryGetError(out var error))
        {
            Console.WriteLine($"Failed to confirm: [{error.Code}] {error.Description}");
        }
    }
    
    public string GetOrderStatus(OrderId orderId)
    {
        var result = _orderService.ConfirmOrder(orderId);
        
        // Using Match() for functional composition
        return result.Match(
            confirmation => $"Order {confirmation.Id} confirmed at {confirmation.ConfirmedAt}",
            error => $"Error: [{error.Code}] {error.Description}");
    }
    
    public void ProcessOrderWithMatch(OrderId orderId)
    {
        var result = _orderService.ConfirmOrder(orderId);
        
        // Switch() for side effects (Match returns a value; Switch is for actions)
        result.Switch(
            confirmation => 
            {
                Console.WriteLine($"Success: Order {confirmation.Id} confirmed");
                SendConfirmationEmail(confirmation);
            },
            error => 
            {
                Console.WriteLine($"Failed: {error.Description}");
                LogError(error);
            });
    }
    
    public void HandleOrderWithPatternMatching(OrderId orderId)
    {
        var result = _orderService.ConfirmOrder(orderId);
        
        // Pattern matching with switch expression
        var outcome = result switch
        {
            { IsSuccess: true } => $"Confirmed: {result.AsT0.Id}",
            { IsSuccess: false } when result.AsT1.Code == "NOT_FOUND" => "Order not found",
            { IsSuccess: false } => $"Error: {result.AsT1.Description}",
            _ => "Unknown state"
        };
        
        Console.WriteLine(outcome);
    }
}
```

`ProcessOrder` shows the pass/fail check with `IsSuccess`. `ValidateAndDisplay` can go further because its error contains `Field` and `Message`. `ConfirmOrder` extracts either the confirmation or the typed error, so it can display the confirmation time on success and the error code on failure.

For the confirmation outcome, choose a handler according to what the caller needs next. `GetOrderStatus` uses `Match()` to return a string. `ProcessOrderWithMatch` uses `Switch` to perform actions instead: confirmation triggers email delivery, while failure triggers error logging. `HandleOrderWithPatternMatching` uses a `switch` expression and a guard to give `NOT_FOUND` its own message. Each method invokes confirmation again; none merely inspects the outcome of a previous controller call.

## Key Points

- `IsSuccess` checks whether the operation succeeded.
- `TryGetResult` and `TryGetError` check and extract the corresponding values.
- `Result<TResult, TError>` supports implicit conversion from a success value or an error.
- Use `Match()` for a returned value and `Switch` for actions; pattern guards can refine an error case when using a `switch` expression.
