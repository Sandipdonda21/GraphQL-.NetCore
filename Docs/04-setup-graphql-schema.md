# 04 Setup GraphQL and Schema

Now that our Entity Framework and DB setup is working fine, let's integrate GraphQL into the project. This step will include installing GraphQL libraries, creating schemas, queries, and mutations.

---

## 📦 Required Packages

Install the following NuGet packages in the `GraphQLDemo.API` project:

```bash
# Core Hot Chocolate GraphQL server
dotnet add package HotChocolate.AspNetCore

# Entity Framework support
dotnet add package HotChocolate.Data.EntityFramework

# Authentication and Authorization
dotnet add package HotChocolate.AspNetCore.Authorization

# Optional: Banana Cake Pop UI (optional but useful)
dotnet add package HotChocolate.AspNetCore.Playground
```

## 📁 Folder Structure (within GraphQLDemo.API)

```bash
GraphQLDemo.API
│
├── GraphQL
│   ├── Queries
│   │   ├── UserQuery.cs
│   │   └── PostQuery.cs
│   ├── Mutations
│   │   ├── UserMutation.cs
│   │   ├── PostMutation.cs
│   │   └── AuthMutation.cs
│   ├── Types
│   └── Filters
```

## ✅ Step-by-Step Setup

1. 📦 Install NuGet Packages

Install required packages:

```bash
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data.EntityFramework
dotnet add package HotChocolate.AspNetCore.Authorization
```

2. 🧠 Create Query & Mutation Classes

`UserQuery.cs`

```csharp
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using HotChocolate;
using HotChocolate.Data;

namespace GraphQLDemo.API.GraphQL.Query;

[ExtendObjectType(Name = "Query")]
public class UserQuery
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context) => context.Users;
}
```

`UserMutation.cs`

```csharp
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using GraphQLDemo.API.GraphQL.Inputs;

namespace GraphQLDemo.API.GraphQL.Mutation;

[ExtendObjectType(Name = "Mutation")]
public class UserMutation
{
    public async Task<User> CreateUserAsync(string username, string email, [Service] AppDbContext context)
    {
        User user = new User
        {
            Username = username,
            Email = email,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
```

`PostQuery.cs`

```csharp
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using HotChocolate;
using HotChocolate.Data;

namespace GraphQLDemo.API.GraphQL.Query;

[ExtendObjectType(Name = "Query")]
public class PostQuery
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPosts([Service] AppDbContext context) => context.Posts;
}
```

`PostMutation.cs`

```csharp
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;

namespace GraphQLDemo.API.GraphQL.Mutation;

[ExtendObjectType(Name = "Mutation")]
public class PostMutation
{
    public async Task<Post> CreatePostAsync(string content, Guid userId, [Service] AppDbContext context)
    {
        Post post = new Post
        {
            UserId = userId,
            Content = content
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }
}
```

## 4. 🧷 Register GraphQL in `Program.cs`

```csharp
using GraphQLDemo.Infrastructure.Data;
using GraphQLDemo.API.GraphQL.Query;
using GraphQLDemo.API.GraphQL.Mutation;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var assembly = typeof(Program).Assembly;

// Register all Query classes

builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    // Auto-register all Query types
    .AddTypes(assembly.GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
            .Cast<ExtendObjectTypeAttribute>()
            .Any(attr => attr.Name == "Query"))
        .ToArray())
    // Auto-register all Mutation types
    .AddTypes(assembly.GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
            .Cast<ExtendObjectTypeAttribute>()
            .Any(attr => attr.Name == "Mutation"))
        .ToArray())
    // Auto-register all Type types
    .AddTypes(assembly.GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
            .Cast<ExtendObjectTypeAttribute>()
            .Any(attr => attr.Name == "Type"))
        .ToArray())
    .AddProjections()
    .SetPagingOptions(new PagingOptions
    {
        IncludeTotalCount = true
    })
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.MapGraphQL();

app.Run();
```

## 🧪 GraphQL Playground Examples

### ✅ Get all Posts

```graphql
query {
  posts {
    nodes {
      id
      content
      createdAt
      userId
    }
  }
}
```

### ✅ Get one Post

```graphql
query {
  posts(where: {
    id: {
      eq: "POST_ID_HERE"
    }
  }) {
    nodes {
      id
      content
      createdAt
      userId
    }
  }
}
```

### ✅ Get user info with post data

```graphql
query {
  posts {
    nodes {
      id
      content
      createdAt
      user {
        id
        username
        email
      }
    }
  }
}
```






