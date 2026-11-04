---
description: Represent a value that may be absent with the .NET Option monad instead of null, so callers must handle both cases.
---
# Option

`Option<TValue>` represents either a value or its absence. Returning it from a lookup makes a missing value part of the method's contract, rather than leaving the caller to infer whether `null` is possible.

## Purpose

Use `Option<T>` when absence is enough information for the caller. In the repository below, a missing user and a missing email address both lead to `None`. If the caller needs a reason for failure, that is a different contract; `Option<T>` carries no error explanation.

## Usage

### Producer Perspective

`UserRepository` first looks up an optional user, then uses that lookup to return an optional email address. This excerpt omits the database field and initialization, along with the application-specific `User` and `UserId` types.

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

`FindById` returns the found user through implicit conversion from `TValue` to `Option<TValue>`; otherwise it returns `Option<User>.None()`. `GetEmailAddress` has a narrower success condition: the user must exist and have a nonempty email address. Either kind of absence returns `Option<string>.None()`, so the caller cannot distinguish them from this result alone.

### Consumer Perspective

The methods in `UserService` show alternative ways to consume those lookups, not a sequence to run for each user. Repository initialization is omitted.

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
        
        // TryGetValue approach
        if (emailOption.TryGetValue(out var email))
        {
            Console.WriteLine($"Email: {email}");
        }
        else
        {
            Console.WriteLine("No email available");
        }
    }
    
    public string GetUserDisplayName(UserId id)
    {
        var userOption = _repository.FindById(id);
        
        // Using Match() for functional composition
        return userOption.Match(
            value => value.Name,
            none => "Unknown User");
    }
    
    public void ProcessUserWithMatch(UserId id)
    {
        var userOption = _repository.FindById(id);
        
        // Switch() for side effects (Match returns a value; Switch is for actions)
        userOption.Switch(
            value => Console.WriteLine($"Processing user: {value.Name}"),
            none => Console.WriteLine("User not found"));
    }
    
    public void DisplayUserWithPatternMatching(UserId id)
    {
        var userOption = _repository.FindById(id);
        
        // Pattern matching with switch expression
        var message = userOption switch
        {
            { IsT0: true } => $"Found user: {userOption.AsT0.Name}",
            _ => "User not found"
        };
        
        Console.WriteLine(message);
    }
}
```

`ProcessUser` checks `HasValue` before extracting the user, while `DisplayUserEmail` uses `TryGetValue` directly to choose between the email and an absence message. The latter pattern combines the presence check and extraction in one call.

`GetUserDisplayName` needs a string, so its `Match()` returns either the user's name or `"Unknown User"`. `ProcessUserWithMatch`, despite its name, uses `Switch` for console output rather than a returned value. `DisplayUserWithPatternMatching` expresses the same presence decision with a `switch` expression; it accesses `AsT0` only in the value case.

## Key Points

- `HasValue` indicates whether the option contains a value.
- `TryGetValue` checks and extracts the value, following the same pattern as Dictionary's TryGetValue.
- `Option<T>.None()` represents absence; a `TValue` can be converted implicitly to `Option<TValue>`.
- Use `Match()` to transform either case into a returned value, or `Switch` for actions.
