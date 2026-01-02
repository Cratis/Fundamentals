# Option

The `Option<TValue>` monad represents an optional value that may or may not be present. It provides a type-safe alternative to null references, making the optionality explicit in the type system.

## Purpose

`Option<T>` eliminates null reference exceptions by forcing explicit handling of the absence of a value. Instead of returning `null` and hoping consumers remember to check, `Option<T>` makes the optionality part of the method signature.

## Usage

### Producer Perspective

Return `Option<T>` from methods that may not always have a value:

```csharp
public class UserRepository
{
    public Option<User> FindById(UserId id)
    {
        var user = _database.Users.FirstOrDefault(u => u.Id == id);
        
        // Implicit conversion from TValue to Option<TValue>
        if (user is not null)
            return user;
        
        // Return None when value is absent
        return Option<User>.None();
    }
    
    public Option<string> GetEmailAddress(UserId userId)
    {
        var user = FindById(userId);
        if (!user.HasValue)
            return Option<string>.None();
        
        if (user.TryGetValue(out var userValue) && !string.IsNullOrEmpty(userValue.Email))
            return userValue.Email;
        
        return Option<string>.None();
    }
}
```

### Consumer Perspective

Explicitly handle the presence or absence of values:

```csharp
public class UserService
{
    readonly UserRepository _repository;
    
    public void ProcessUser(UserId id)
    {
        var userOption = _repository.FindById(id);
        
        // Check if value is present
        if (userOption.HasValue)
        {
            // TryGetValue provides safe access
            if (userOption.TryGetValue(out var user))
            {
                Console.WriteLine($"Processing user: {user.Name}");
            }
        }
        else
        {
            Console.WriteLine("User not found");
        }
    }
    
    public void DisplayUserEmail(UserId id)
    {
        var emailOption = _repository.GetEmailAddress(id);
        
        // Pattern matching approach
        if (emailOption.TryGetValue(out var email))
        {
            Console.WriteLine($"Email: {email}");
        }
        else
        {
            Console.WriteLine("No email available");
        }
    }
}
```

## Key Points

- Use `Option<T>` instead of returning `null` for optional values
- The `HasValue` property indicates if a value is present
- `TryGetValue` provides safe value extraction similar to Dictionary's TryGetValue pattern
- Implicit conversion from `TValue` to `Option<TValue>` simplifies creation
- Makes API contracts explicit - callers know they must handle absence
