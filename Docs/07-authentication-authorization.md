# 🔐 Authentication & Authorization Setup

We will implement JWT-based authentication and role-based authorization using ASP.NET Core Identity. This will include updates to:

- Database (add fields to User)
- Input models
- GraphQL resolvers
- Program.cs configuration
- Middleware for policy-based access
- Validation

## 🧩 Step 1: Update User Model

Add the following properties to `User.cs` in `Core/Models`:

```csharp
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; }
    public string Role { get; set; } = "User";
}
```

## 🧾 Step 2: Create Input DTOs

`Core/Inputs/UserInput.cs`

```csharp
namespace GraphQLDemo.Core.Inputs;

public class RegisterInput
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginInput
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

## ✅ Step 3: Add Input Validation

`Core/Validators/RegisterInputValidator.cs`

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

## 🔐 Step 4: Create AuthService

`Infrastructure/Services/AuthService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using GraphQLDemo.Core.Inputs;
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using GraphQLDemo.Core.Exceptions;

namespace GraphQLDemo.Infrastructure.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IValidator<RegisterInput> _registerValidator;

    public AuthService(
        AppDbContext context, 
        IConfiguration config, 
        IValidator<RegisterInput> registerValidator)
    {
        _context = context;
        _config = config;
        _registerValidator = registerValidator;
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

            throw new ValidationException(errorDict);
        }

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

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

## 🧪 Step 5: Add Auth Mutation

`GraphQL/Mutations/AuthMutation.cs`:

```csharp
using GraphQLDemo.Core.Inputs;
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Services;
using HotChocolate;

namespace GraphQLDemo.API.GraphQL.Mutation;

[ExtendObjectType(Name = "Mutation")]
public class AuthMutation
{
    public async Task<User> RegisterAsync(
        RegisterInput input, 
        [Service] AuthService authService)
        => await authService.RegisterAsync(input);

    public async Task<string> LoginAsync(
        LoginInput input, 
        [Service] AuthService authService)
        => await authService.LoginAsync(input);
}
```

## 🧰 Step 6: Configure JWT in Program.cs

```csharp
// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<AuthService>();

// Add Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterInputValidator>();

// Configure GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddErrorFilter<GraphQLErrorFilter>();

// Add middleware
app.UseAuthentication();
app.UseAuthorization();
```

Add this to your `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKey"
  }
}
```

## 🛡️ Step 7: Protect GraphQL Resolvers with Roles

Example: Protect creating a post for only authenticated users:

```csharp
[Authorize(Roles = ["User"])]
public async Task<Post> CreatePostAsync(
    CreatePostInput input, 
    [Service] AppDbContext context,
    ClaimsPrincipal user)
{
    var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    var post = new Post 
    { 
        UserId = userId, 
        Content = input.Content 
    };
    context.Posts.Add(post);
    await context.SaveChangesAsync();
    return post;
}
```

## 🧪 GraphQL Playground Examples

### Register a new user

```graphql
mutation {
  register(input: {
    username: "testuser",
    email: "test@example.com",
    password: "password123"
  }) {
    id
    username
    email
    role
  }
}
```

### Login

```graphql
mutation {
  login(input: {
    email: "test@example.com",
    password: "password123"
  })
}
```

### Create a post (requires authentication)

```graphql
mutation {
  createPost(input: {
    content: "Hello GraphQL!"
  }) {
    id
    content
    createdAt
  }
}
```

Remember to include the JWT token in the request headers:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

## Navigation
- [Next: 08-ErrorHandling-BestPractices →](08-ErrorHandling-BestPractices.md)