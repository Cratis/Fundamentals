# Dependency Injection

The `Cratis.DependencyInjection` namespace provides convention-based dependency injection features that simplify service registration in .NET applications. These utilities help reduce boilerplate code when working with Microsoft's dependency injection container.

## Overview

The dependency injection utilities offer:

- **Convention-based registration**: Automatically register services following naming conventions
- **Self-binding**: Register concrete types for direct injection
- **Lifecycle management**: Control service lifetimes using attributes
- **Selective registration**: Exclude types from convention-based registration

## Extension Methods

### AddBindingsByConvention

Automatically registers services based on interface naming conventions.

```csharp
services.AddBindingsByConvention();
```

**Convention Rules:**

- Interface and implementation must be in the same namespace
- Interface name must be `I{ImplementationName}` (e.g., `IUserService` → `UserService`)
- Only one implementation per interface is allowed
- Types with `[IgnoreConvention]` attribute are excluded

**Examples:**

```csharp
// These will be automatically registered:
public interface IUserService { }
public class UserService : IUserService { } // Registered as Transient

public interface IOrderProcessor { }
[Singleton]
public class OrderProcessor : IOrderProcessor { } // Registered as Singleton

// This will be ignored:
[IgnoreConvention]
public class TestUserService : IUserService { }
```

### AddSelfBindings

Registers concrete types for direct injection without requiring an interface.

```csharp
services.AddSelfBindings();
```

**Registration Rules:**

- Non-static, non-abstract, non-interface types
- Types not derived from `Exception`
- Types without `[IgnoreConvention]` attribute
- Types not in `System` or `Microsoft` namespaces
- Types with resolvable constructor parameters

**Examples:**

```csharp
// These will be automatically registered:
public class EmailSender { } // Registered as Transient

[Singleton]
public class CacheManager { } // Registered as Singleton

// These will be ignored:
public class SystemType { } // In System namespace
[IgnoreConvention]
public class InternalService { } // Has ignore attribute
public class BadService(string connectionString) { } // Primitive parameter
```

## Attributes

### SingletonAttribute

Marks a class to be registered as a singleton service.

```csharp
[Singleton]
public class DatabaseConnection : IDatabaseConnection
{
    // This service will be registered as a singleton
}
```

**Usage:**

- Apply to class declarations
- Works with both convention-based and self-binding registration
- Overrides the default transient lifetime

### IgnoreConventionAttribute

Excludes a class from convention-based registration.

```csharp
[IgnoreConvention]
public class TestImplementation : IService
{
    // This class will not be automatically registered
}
```

**Usage:**

- Apply to class declarations
- Useful for test implementations, decorators, or manually registered services
- Affects both `AddBindingsByConvention` and `AddSelfBindings`

## Best Practices

### 1. Use Convention-Based Registration First

Start with convention-based registration for most services:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddBindingsByConvention();
    services.AddSelfBindings();
    
    // Manual registrations for special cases
    services.AddScoped<ISpecialService, CustomImplementation>();
}
```

### 2. Follow Naming Conventions

Ensure your interfaces and implementations follow the expected patterns:

```csharp
// ✅ Good - follows convention
public interface IUserRepository { }
public class UserRepository : IUserRepository { }

// ❌ Bad - doesn't follow convention
public interface UserRepo { }
public class UserRepositoryImpl : UserRepo { }
```

### 3. Use Attributes Appropriately

Apply lifecycle and exclusion attributes where needed:

```csharp
// Singleton for expensive-to-create services
[Singleton]
public class DatabaseConnectionPool : IDatabaseConnectionPool { }

// Ignore for test doubles or special implementations
[IgnoreConvention]
public class MockUserService : IUserService { }
```

### 4. Avoid Primitive Dependencies

Constructor parameters should use concepts or dependency injection:

```csharp
// ❌ Bad - primitive parameters prevent auto-registration
public class EmailService(string smtpServer, int port) { }

// ✅ Good - uses concepts and injection
public class EmailService(SmtpConfiguration config, ILogger<EmailService> logger) { }
```

## Integration Example

Complete setup in a typical ASP.NET Core application:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add convention-based services
        services.AddBindingsByConvention();
        services.AddSelfBindings();
        
        // Add framework services
        services.AddControllers();
        services.AddDbContext<MyDbContext>(options => 
            options.UseSqlServer(connectionString));
        
        // Manual registrations for complex scenarios
        services.AddScoped<IEmailService>(provider => 
            new EmailService(provider.GetRequiredService<IConfiguration>()));
    }
}
```

This approach significantly reduces the amount of manual service registration code while maintaining flexibility for special cases.
