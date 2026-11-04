---
description: Return success, typed errors or caught exceptions from .NET code with Catch, keeping producer try-catch boundaries explicit.
---
# Catch

`Catch` represents the outcome of an operation that can throw an exception. The producer catches the exception and returns it as a value; the consumer then handles that value alongside success. Constructing or returning a `Catch` does not automatically trap exceptions thrown by the operation or by a consumer callback.

## Purpose

Use `Catch` at a boundary with exception-throwing code, such as file I/O, network calls or a third-party API. Choose the variant before writing the producer:

- `Catch` represents success without a return value, or an exception.
- `Catch<TResult>` represents a result or an exception.
- `Catch<TResult, TError>` represents a result, a typed error or a raw exception. The typed error does not replace the exception case.

The file examples below keep the `try`/`catch` in the producer. That boundary lets the caller handle a returned outcome without repeating the file operation's exception-handling code.

## Usage

### Producer Perspective

`DeleteFile` wraps an operation with no success value; `LoadConfiguration` and `ReadConfigValue` return a value on success. `LoadSecureConfig` additionally translates selected exceptions into `ConfigError`. These excerpts omit `AppConfig`, the configuration provider and the application-specific `LoadAndValidate` implementation.

```csharp
// Simple Catch
public class FileOperations
{
    public Catch DeleteFile(string path)
    {
        try
        {
            File.Delete(path);
            return Catch.Success();
        }
        catch (Exception ex)
        {
            return Catch.Failed(ex);
        }
    }
}

// Catch<TResult>
public class ConfigurationService
{
    public Catch<AppConfig> LoadConfiguration(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<AppConfig>(json);
            
            // Implicit conversion from TResult to Catch<TResult>
            return config!;
        }
        catch (Exception ex)
        {
            // Implicit conversion from Exception to Catch<TResult>
            return ex;
        }
    }
    
    public Catch<string> ReadConfigValue(string key)
    {
        try
        {
            var value = _configProvider.GetValue(key);
            return Catch<string>.Success(value);
        }
        catch (Exception ex)
        {
            return Catch<string>.Failed(ex);
        }
    }
}

// Catch<TResult, TError>
public record ConfigError(string Code, string Message, string? Details);

public class SecureConfigService
{
    public Catch<AppConfig, ConfigError> LoadSecureConfig(string filePath)
    {
        try
        {
            var config = LoadAndValidate(filePath);
            return Catch<AppConfig, ConfigError>.Success(config);
        }
        catch (FileNotFoundException ex)
        {
            return Catch<AppConfig, ConfigError>.Failed(
                new ConfigError("FILE_NOT_FOUND", "Configuration file not found", ex.Message));
        }
        catch (JsonException ex)
        {
            return Catch<AppConfig, ConfigError>.Failed(
                new ConfigError("INVALID_JSON", "Invalid configuration format", ex.Message));
        }
        catch (Exception ex)
        {
            // Can also pass the raw exception
            return Catch<AppConfig, ConfigError>.Failed(ex);
        }
    }
}
```

`LoadConfiguration` explicitly catches exceptions from reading or deserializing the file and returns them through implicit conversion to `Catch<AppConfig>`. `ReadConfigValue` uses the `Success` and `Failed` factory methods to construct the same two-case contract.

`LoadSecureConfig` returns three possible outcomes. A successful load supplies `AppConfig`; a missing file or invalid JSON becomes a `ConfigError`; any other exception reaches the final catch block and remains a raw exception. A caller that handles only `AppConfig` and `ConfigError` would leave that last case unhandled.

### Consumer Perspective

The methods below show alternative ways to handle returned outcomes. `LoadAndApplyConfig` consumes the two-case contract; `LoadSecureConfiguration` handles all three cases. Service initialization and application helpers such as `ApplyConfiguration`, `LogError` and `LogException` are omitted.

```csharp
public class ConfigurationManager
{
    readonly ConfigurationService _configService;
    readonly SecureConfigService _secureConfigService;
    
    public void LoadAndApplyConfig(string path)
    {
        // Catch<TResult>
        var configResult = _configService.LoadConfiguration(path);
        
        if (configResult.IsSuccess && configResult.TryGetResult(out var config))
        {
            Console.WriteLine($"Configuration loaded: {config.AppName}");
            ApplyConfiguration(config);
        }
        else if (configResult.TryGetException(out var exception))
        {
            Console.WriteLine($"Failed to load configuration: {exception.Message}");
            LogException(exception);
        }
    }
    
    public void LoadSecureConfiguration(string path)
    {
        // Catch<TResult, TError>
        var result = _secureConfigService.LoadSecureConfig(path);
        
        if (result.TryGetResult(out var config))
        {
            Console.WriteLine("Secure configuration loaded successfully");
            ApplyConfiguration(config);
        }
        else if (result.TryGetError(out var error))
        {
            Console.WriteLine($"Configuration error: [{error.Code}] {error.Message}");
            if (error.Details is not null)
            {
                Console.WriteLine($"Details: {error.Details}");
            }
        }
        else if (result.TryGetException(out var exception))
        {
            LogException(exception);
        }
    }
    
    public void SafelyReadConfig(string key)
    {
        var result = _configService.ReadConfigValue(key);
        
        // Using TryGetResult/TryGetException
        var message = result.TryGetResult(out var value) 
            ? $"Config value: {value}" 
            : result.TryGetException(out var ex) 
                ? $"Error: {ex.Message}" 
                : "Unknown state";
        
        Console.WriteLine(message);
    }
    
    public string LoadConfigWithMatch(string path)
    {
        var result = _configService.LoadConfiguration(path);
        
        // Using Match() for functional composition
        return result.Match(
            config => $"Loaded: {config.AppName} v{config.Version}",
            exception => $"Failed to load: {exception.Message}");
    }
    
    public void ProcessConfigWithMatch(string path)
    {
        var result = _secureConfigService.LoadSecureConfig(path);
        
        // Switch() for side effects. Catch<T, TError> is a three-case union
        // (success, error, exception), so Switch takes three actions.
        result.Switch(
            config => 
            {
                Console.WriteLine("Configuration loaded successfully");
                ApplyConfiguration(config);
            },
            error => 
            {
                Console.WriteLine($"Configuration error: {error.Message}");
                LogError(error);
            },
            exception => 
            {
                Console.WriteLine($"Unexpected failure: {exception.Message}");
            });
    }
    
    public void HandleConfigWithPatternMatching(string path)
    {
        var result = _configService.LoadConfiguration(path);
        
        // Pattern matching with switch expression
        var status = result switch
        {
            { IsSuccess: true } => $"Config loaded: {result.AsT0.AppName}",
            { IsSuccess: false } when result.AsT1 is FileNotFoundException => "Config file not found",
            { IsSuccess: false } when result.AsT1 is JsonException => "Invalid config format",
            { IsSuccess: false } => $"Error: {result.AsT1.Message}",
            _ => "Unknown state"
        };
        
        Console.WriteLine(status);
    }
}
```

`LoadSecureConfiguration` first extracts a configuration, then a `ConfigError`, then a raw exception. Its `TryGetException` branch covers the producer's final catch block rather than assuming every failure has been translated into an application error. `SafelyReadConfig` shows the shorter two-case extraction pattern for a configuration value.

`LoadConfigWithMatch` needs a returned string, so `Match()` transforms either the loaded configuration or the exception into text. `ProcessConfigWithMatch` instead uses `Switch` for actions. Because it consumes `Catch<AppConfig, ConfigError>`, its callbacks are ordered as success, typed error and exception. These methods are alternatives, not additional steps after `LoadSecureConfiguration`.

`HandleConfigWithPatternMatching` consumes the two-case `Catch<AppConfig>` and refines the exception case with type guards. Its `AsT1` is an exception; in the three-case contract, the typed error occupies that position instead.

## Key Points

- `IsSuccess` indicates whether the operation succeeded.
- `TryGetResult` checks and extracts a success value in the value-bearing variants. `TryGetError` extracts a typed error in the three-case variant; `TryGetException` extracts a raw exception.
- `TResult` and `Exception` can be converted implicitly to a value-bearing `Catch`.
- Use `Match()` for a returned value and `Switch` for actions. A `switch` expression can refine an exception case with guards such as `when result.AsT1 is FileNotFoundException`.
