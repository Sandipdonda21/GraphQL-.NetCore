# ✍️ 05 - Post CRUD Mutation Setup

## 🗂️ Folder Structure

```
GraphQLDemo.Core/
├── Inputs/
│   └── PostInput.cs

GraphQLDemo.API/
├── GraphQL/
│   └── Mutations/
│       └── PostMutation.cs
```

## 📌 1. `PostInput.cs` (Input class for Create/Update)

```csharp
namespace GraphQLDemo.Core.Inputs;

public class CreatePostInput
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdatePostInput
{
    public Guid PostId { get; set; }
    public string NewContent { get; set; } = string.Empty;
}
```

## 📌 2. `PostMutation.cs`

```csharp
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using GraphQLDemo.Core.Inputs;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;

namespace GraphQLDemo.API.GraphQL.Mutation;

[ExtendObjectType("Mutation")]
public class PostMutation
{
    [Authorize(Roles = ["User"])]
    public async Task<Post> CreatePostAsync(CreatePostInput input, [Service] AppDbContext context, [Service] IMemoryCache cache)
    {
        Post post = new Post
        {
            UserId = input.UserId,
            Content = input.Content
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        string cacheKey = $"posts_user_{input.UserId}";
        cache.Remove(cacheKey); // Clear cache after post creation

        return post;
    }

    public async Task<Post?> UpdatePostAsync(UpdatePostInput input, [Service] AppDbContext context)
    {
        var post = await context.Posts.FindAsync(input.PostId);
        if (post == null)
            return null;

        post.Content = input.NewContent;
        await context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> DeletePostAsync(Guid postId, [Service] AppDbContext context)
    {
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
            return false;

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return true;
    }
}
```

## 🧪 GraphQL Playground Examples

### ✅ Create a Post (requires User role)

```graphql
mutation {
  createPost(input: {
    userId: "GUID_HERE",
    content: "This is a test post"
  }) {
    id
    content
    createdAt
    userId
    user {
      username
      email
    }
  }
}
```

### ✏️ Update a Post

```graphql
mutation {
  updatePost(input: {
    postId: "POST_ID_HERE",
    newContent: "Updated post content"
  }) {
    id
    content
    createdAt
    userId
    user {
      username
      email
    }
  }
}
```

### ❌ Delete a Post

```graphql
mutation {
  deletePost(postId: "POST_ID_HERE")
}
```

## 🔒 Authorization Notes

- The `createPost` mutation requires the "User" role
- Make sure to include the JWT token in the request headers:
  ```
  Authorization: Bearer YOUR_JWT_TOKEN
  ```
- Other mutations (update/delete) can be secured by adding the `[Authorize]` attribute as needed

## 💾 Caching Notes

- Post creation automatically invalidates the user's post cache
- Cache key format: `posts_user_{userId}`
- Cache is cleared after successful post creation





