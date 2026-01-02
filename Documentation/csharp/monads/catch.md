# Catch

The `Catch` monad represents operations that may throw exceptions, converting exception-based error handling into value-based handling. It captures exceptions as values, enabling functional composition without try-catch blocks.

## Purpose

`Catch` provides a way to handle exceptions as values rather than control flow. This is particularly useful when working with third-party APIs or legacy code that uses exceptions, while keeping your application code functional and composable.

## Usage

### Producer Perspective

Wrap exception-throwing code to return `Catch`:

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

### Consumer Perspective

Handle exceptions as values without try-catch blocks:

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
        
        // Match() with side effects
        result.Match(
            config => 
            {
                Console.WriteLine("Configuration loaded successfully");
                ApplyConfiguration(config);
            },
            error => 
            {
                Console.WriteLine($"Configuration error: {error.Message}");
                LogError(error);
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

## Key Points

- Use `Catch` to convert exception-based APIs into value-based ones
- Choose the appropriate variant:
  - `Catch` - Simple exception capture without return value
  - `Catch<TResult>` - Capture results or exceptions
  - `Catch<TResult, TError>` - Custom error representation for specific exception types
- Use `Match()` for functional composition of potentially failing operations
- Pattern matching with `switch` expressions enables exception-type-specific handling
- Access specific exception types with pattern guards (e.g., `when result.AsT1 is FileNotFoundException`)
- Implicit conversions from `TResult` and `Exception` simplify creation
- `IsSuccess` indicates whether the operation succeeded
- `TryGetResult` and `TryGetException` provide safe access to values
- Particularly useful for wrapping file I/O, network calls, and third-party libraries
- Enables functional composition of potentially failing operations
