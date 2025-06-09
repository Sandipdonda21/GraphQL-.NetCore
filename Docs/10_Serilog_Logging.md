# 🔧 Doc 10: Implementing Serilog Logging in GraphQL .NET Core API

This guide demonstrates how to configure Serilog in your .NET Core project to:
- Output logs to the Console
- Save logs to a file (e.g., `logs/log.txt`)
- Log GraphQL operations with detailed information
- Implement structured logging

## 📦 1. Install Required NuGet Packages

Run the following commands:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

## 🛠️ 2. Configure Serilog in Program.cs

📄 File: `GraphQLDemo.API/Program.cs`

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("HotChocolate", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting up the application");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Existing services configuration...
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // other services...
    
    var app = builder.Build();

    // Add GraphQL request logging middleware
    app.UseMiddleware<GraphQLDemo.API.Middleware.GraphQLRequestLoggingMiddleware>();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRouting();

    app.MapGraphQL();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}
```

## 🔍 3. GraphQL Request Logging Middleware

📄 File: `GraphQLDemo.API/Middleware/GraphQLRequestLoggingMiddleware.cs`

```csharp
public class GraphQLRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public GraphQLRequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/graphql") || context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        context.Request.EnableBuffering();
        
        // Read request body
        var requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
        context.Request.Body.Position = 0;

        string queryText = "N/A";
        string userEmail = "Anonymous";

        // Extract query and user information
        try
        {
            var doc = JsonDocument.Parse(requestBody);
            if (doc.RootElement.TryGetProperty("query", out var queryElement))
            {
                queryText = queryElement.GetString() ?? "EmptyQuery";
            }
        }
        catch (Exception ex)
        {
            queryText = $"Invalid JSON: {ex.Message}";
        }

        userEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

        // Capture response
        var originalBody = context.Response.Body;
        await using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        string resultStatus = "Success";

        try
        {
            await _next(context);
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            
            if (responseBody.Contains("\"errors\""))
                resultStatus = "Fail";

            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
        }
        catch (Exception)
        {
            resultStatus = "Fail";
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
            stopwatch.Stop();

            Log.Information(
                "GraphQL Operation: {Query}\nTime: {Elapsed} ms\nUser: {User}\nStatus: {Status}",
                queryText.Trim(),
                stopwatch.ElapsedMilliseconds,
                userEmail,
                resultStatus
            );
        }
    }
}
```

## 📝 4. Service-Level Logging

Example: Add Logging in AuthService
📄 File: `GraphQLDemo.Infrastructure/Services/AuthService.cs`

```csharp
public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IValidator<RegisterInput> _registerValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext context, 
        IConfiguration config, 
        IValidator<RegisterInput> registerValidator, 
        ILogger<AuthService> logger)
    {
        _context = context;
        _config = config;
        _registerValidator = registerValidator;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(RegisterInput input)
    {
        var result = await _registerValidator.ValidateAsync(input);
        if (!result.IsValid)
        {
            var errorDict = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            throw new Core.Exceptions.ValidationException(errorDict);
        }

        _logger.LogInformation("Registering user with email: {Email}", input.Email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = input.Username,
            Email = input.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
            Role = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<string> LoginAsync(LoginInput input)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == input.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        // ... JWT token generation code ...
        return tokenHandler.WriteToken(token);
    }
}
```

## 📁 5. Log File Structure

Logs are stored in:
```
GraphQLDemo.API/logs/log.txt
```

Example log entries:
```
[2024-03-14 10:15:23 INF] Starting up the application
[2024-03-14 10:15:24 INF] GraphQL Operation: query { posts { id content } }
Time: 45 ms
User: user@example.com
Status: Success
[2024-03-14 10:15:25 INF] Registering user with email: newuser@example.com
```

## ✅ Best Practices

| Practice               | Description                                                    |
| ---------------------- | -------------------------------------------------------------- |
| Use `ILogger<T>`       | For class-specific logging context                             |
| Use log levels         | `.LogInformation`, `.LogWarning`, `.LogError`, etc.            |
| Avoid logging secrets  | Never log passwords or sensitive JWTs                          |
| Use structured logging | e.g., `_logger.LogInformation("User {Id}", user.Id);`          |
| Roll log files         | Use `rollingInterval: RollingInterval.Day` for manageable logs |
