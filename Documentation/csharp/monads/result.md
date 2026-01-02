# Result

The `Result` monad represents the outcome of an operation that can either succeed or fail. It provides type-safe handling of both success and error states without relying on exceptions for control flow.

## Purpose

`Result` enables explicit error handling by encoding success and failure as types. This makes error cases visible in the type system and forces consumers to handle both paths explicitly.

## Usage

### Producer Perspective

Return `Result` to indicate operation outcomes:

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

### Consumer Perspective

Handle success and failure paths explicitly:

```csharp
public class OrderController
{
    readonly OrderService _orderService;
    readonly OrderValidator _validator;
    
    public void ProcessOrder(Order order)
    {
        // Simple Result
        var validationResult = _validator.ValidateOrder(order);
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
        
        // Match() with side effects
        result.Match(
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

## Key Points

- Use `Result` for operations that can fail in predictable ways
- Choose the appropriate variant based on your needs:
  - `Result` - Simple success/failure indication
  - `Result<TError>` - When you need to communicate why it failed
  - `Result<TResult, TError>` - When success produces a value
- Use `Match()` for functional composition and transforming results
- Pattern matching with `switch` expressions enables conditional logic based on success/error states
- Access underlying exception types with pattern guards for specific error handling
- Implicit conversions simplify creating results from values or errors
- `IsSuccess` provides a quick check before extracting values
- `TryGetResult` and `TryGetError` provide safe access to the underlying values
- Avoid exceptions for expected failure cases - use `Result` instead
