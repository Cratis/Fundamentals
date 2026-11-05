---
title: Register .NET services by convention with Cratis
description: Cratis.DependencyInjection registers .NET services by naming convention or concrete type, with transient defaults and attributes for exceptions.
---

<a id="dependency-injection"></a>

`Cratis.DependencyInjection` in Cratis Fundamentals registers services with Microsoft's dependency injection container by naming convention or concrete type. Use it when you would otherwise maintain matching registrations for each application service. Keep explicit registrations for services that need a factory or a choice between implementations.

## Overview

A registration tells the container which implementation to construct when a consumer asks for a service. `AddBindingsByConvention` pairs an interface with its matching implementation; `AddSelfBindings` registers a concrete type so consumers can request it directly.

Both methods default to a transient lifetime: the container creates an instance for each resolution. Apply `[Singleton]` when the registration should reuse one instance, or `[IgnoreConvention]` when you want to register a type yourself. Existing registrations take precedence: neither method replaces a service type already registered.

Each method prefers its generated binding list and falls back to reflection when that list is empty. If an expected service is missing, check the contributing assemblies in [type discovery](./types.md) before changing its registration.

## Extension Methods

### AddBindingsByConvention

Call `AddBindingsByConvention` to pair interfaces with implementations whose names match:

```csharp
services.AddBindingsByConvention();
```

A pair qualifies when:

- The interface and implementation are in the same namespace.
- The interface name is `I{ImplementationName}`, such as `IUserService` and `UserService`.
- Exactly one non-ignored implementation exists for the interface.
- The implementation is not marked `[IgnoreConvention]`.

The following pairs qualify; the test implementation is excluded so it does not compete with `UserService`:

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

Call `AddSelfBindings` when consumers depend on concrete classes rather than interfaces:

```csharp
services.AddSelfBindings();
```

Self-binding excludes static or abstract types, interfaces, exception types, and types marked `[IgnoreConvention]`. It also excludes types in `System` or `Microsoft` namespaces.

A type is excluded if any public constructor has a primitive-typed or record-typed parameter. This keeps data values, such as a connection string, from being treated as services the container should resolve. The rule checks all public constructors, not just the one you expect the container to use.

`EmailSender` and `CacheManager` below qualify. The remaining examples illustrate the namespace, attribute, and constructor exclusions:

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

Apply `[Singleton]` to change a convention or self-binding registration from transient to singleton:

```csharp
[Singleton]
public class DatabaseConnection : IDatabaseConnection
{
    // This service will be registered as a singleton
}
```

The attribute controls the lifetime of an eligible registration; it does not make an otherwise excluded type eligible. Choose it only when sharing the instance across consumers fits the service's state and dependencies.

### IgnoreConventionAttribute

Apply `[IgnoreConvention]` to keep a class out of both `AddBindingsByConvention` and `AddSelfBindings`:

```csharp
[IgnoreConvention]
public class TestImplementation : IService
{
    // This class will not be automatically registered
}
```

Use this for test implementations or services you register manually, including decorators. Excluding a test implementation also prevents it from making an otherwise unique interface-to-implementation pair ambiguous.

## Best Practices

### 1. Use Convention-Based Registration First

Combine the two methods when the application uses both interface and concrete-type dependencies. Register exceptions explicitly, as with the scoped service below:

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

Use an `I`-prefixed interface name and place the matching implementation in the same namespace when you want automatic pairing. A different naming scheme is valid C#, but requires an explicit registration:

```csharp
// ✅ Good - follows convention
public interface IUserRepository { }
public class UserRepository : IUserRepository { }

// ❌ Bad - doesn't follow convention
public interface UserRepo { }
public class UserRepositoryImpl : UserRepo { }
```

### 3. Use Attributes Appropriately

Choose a singleton lifetime for services intended to be shared, and exclude implementations that should not participate in automatic registration. Construction cost alone is not a reason to share mutable state across requests:

```csharp
// Singleton for expensive-to-create services
[Singleton]
public class DatabaseConnectionPool : IDatabaseConnectionPool { }

// Ignore for test doubles or special implementations
[IgnoreConvention]
public class MockUserService : IUserService { }
```

### 4. Avoid Primitive Dependencies

For a self-bound service, pass configuration through an eligible non-record configuration class rather than individual primitive values. The container still needs a registration for that configuration:

```csharp
// ❌ Bad - primitive parameters prevent auto-registration
public class EmailService(string smtpServer, int port) { }

// ✅ Good - injects an eligible non-record configuration dependency and a logger
public class EmailService(SmtpConfiguration config, ILogger<EmailService> logger) { }
```

Here, `SmtpConfiguration` must be a non-record class. If you also want it self-bound, it must satisfy the same exclusions. A `ConceptAs<T>` dependency does not avoid the record restriction because the concept base type is a record.

Registration and construction are separate steps. Even when `EmailService` qualifies for self-binding, the container must be able to resolve both `SmtpConfiguration` and `ILogger<EmailService>`.

## Integration Example

In an ASP.NET Core host, combine conventions with framework registrations and factories. Adapt the application-specific constructors and connection settings to your host:

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

Conventions handle the matching service pairs and eligible concrete types; the factory supplies the manually configured email service. If a type is not registered as expected, use [Types](./types.md) to inspect the discovery mode and contributing assemblies, then check the naming and constructor rules above.
