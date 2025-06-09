# 🧩 Error Handling & Best Practices

This document outlines robust error handling strategies, custom exceptions, validation mechanisms, and best practices to ensure your GraphQL API is reliable, secure, and developer-friendly.

## 1️⃣ Custom Exceptions

🔹 File: `AppException.cs`

Location: `GraphQLDemo.Core/Exceptions/AppException.cs`

```csharp
namespace GraphQLDemo.Core.Exceptions;

public class AppException : Exception
{
    public AppException(string message) : base(message) { }

    public AppException(string message, Exception innerException) : base(message, innerException) { }
}
```

## 2️⃣ NotFound Exception

🔹 File: `NotFoundException.cs`

Location: `GraphQLDemo.Core/Exceptions/NotFoundException.cs`

```csharp
namespace GraphQLDemo.Core.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found.") { }
}
```

## 3️⃣ Validation Exception

🔹 File: `ValidationException.cs`

Location: `GraphQLDemo.Core/Exceptions/ValidationException.cs`

```csharp
namespace GraphQLDemo.Core.Exceptions;

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}
```

## 4️⃣ Global Error Filter (GraphQL)

🔹 File: `GraphQLErrorFilter.cs`

Location: `GraphQLDemo.API/GraphQL/Filters/GraphQLErrorFilter.cs`

```csharp
using GraphQLDemo.Core.Exceptions;
using HotChocolate;
using HotChocolate.Execution;
using System.Collections.Generic;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ValidationException vex)
        {
            // Convert errors into GraphQL-serializable object
            var serializableErrors = new Dictionary<string, object>();
            foreach (var kv in vex.Errors)
            {
                serializableErrors[kv.Key] = kv.Value;
            }

            return error
                .WithMessage("Validation failed.")
                .SetExtension("validationErrors", serializableErrors);
        }

        if (error.Exception != null)
        {
            return error
                .WithMessage("Unexpected error occurred.")
                .SetExtension("errorType", error.Exception.GetType().Name)
                .SetExtension("details", error.Exception.Message);
        }

        return error;
    }
}
```

## 5️⃣ Fluent Validation Integration

🔹 Install Package

```bash
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

🔹 Sample Validator: `RegisterInputValidator.cs`

Location: `GraphQLDemo.Core/Validators/RegisterInputValidator.cs`

```csharp
using FluentValidation;
using GraphQLDemo.Core.Inputs;

namespace GraphQLDemo.Core.Validators;

public class RegisterInputValidator : AbstractValidator<RegisterInput>
{
    public RegisterInputValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}
```

## 6️⃣ Registering in Program.cs

🔹 Program.cs

Location: `GraphQLDemo.API/Program.cs`

```csharp
// Add validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterInputValidator>();

// Configure GraphQL
builder.Services
    .AddGraphQLServer()
    .AddErrorFilter<GraphQLErrorFilter>()
    .AddAuthorization()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddTypes(assembly.GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
            .Cast<ExtendObjectTypeAttribute>()
            .Any(attr => attr.Name == "Query"))
        .ToArray())
    .AddTypes(assembly.GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
            .Cast<ExtendObjectTypeAttribute>()
            .Any(attr => attr.Name == "Mutation"))
        .ToArray());
```

## 7️⃣ Example Error Responses

### Validation Error

```json
{
  "errors": [
    {
      "message": "Validation failed.",
      "extensions": {
        "validationErrors": {
          "username": ["Username must be at least 3 characters long."],
          "email": ["Email is not a valid email address."],
          "password": ["Password must be at least 6 characters long."]
        }
      }
    }
  ]
}
```

### Not Found Error

```json
{
  "errors": [
    {
      "message": "Post with key '123' was not found.",
      "extensions": {
        "errorType": "NotFoundException"
      }
    }
  ]
}
```

### Unexpected Error

```json
{
  "errors": [
    {
      "message": "Unexpected error occurred.",
      "extensions": {
        "errorType": "InvalidOperationException",
        "details": "Database connection failed."
      }
    }
  ]
}
```

## 8️⃣ Best Practices

1. **Use Custom Exceptions**
   - Create domain-specific exceptions
   - Provide meaningful error messages
   - Include relevant context in error details

2. **Validation**
   - Validate input at the edge (GraphQL resolvers)
   - Use FluentValidation for complex validation rules
   - Return structured validation errors

3. **Error Filtering**
   - Use GraphQL error filter to standardize error responses
   - Include error type and details in extensions
   - Sanitize error messages for production

4. **Logging**
   - Log all errors with appropriate severity
   - Include correlation IDs for tracking
   - Don't expose sensitive information in error messages

5. **Security**
   - Don't expose stack traces in production
   - Sanitize error messages
   - Use appropriate HTTP status codes

## Navigation
- [Next: 9_Caching →](9_Caching.md)