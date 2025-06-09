# 🧠 Caching Strategy

This document outlines the caching implementation for the GraphQL API to improve performance and reduce database load.

## 🔧 Implementation Details

### 1. Required Packages

```bash
dotnet add package Microsoft.Extensions.Caching.Memory
```

### 2. Service Registration

In `Program.cs`:

```csharp
// Add memory cache
builder.Services.AddMemoryCache();
```

### 3. Modify Post Service to Use Cache

📄 File: `GraphQLDemo.Infrastructure/Services/PostService.cs`

```csharp
using Microsoft.Extensions.Caching.Memory;
using GraphQLDemo.Infrastructure.Data;
using GraphQLDemo.Core.Models;
using Microsoft.EntityFrameworkCore;

public class PostService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION_MINUTES = 5;

    public PostService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Post>> GetPostsAsync(Guid userId)
    {
        string cacheKey = $"posts_user_{userId}";
        
        if (!_cache.TryGetValue(cacheKey, out List<Post>? posts))
        {
            posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, posts, cacheEntryOptions);
        }

        return posts!;
    }

    public void InvalidateUserPostsCache(Guid userId)
    {
        string cacheKey = $"posts_user_{userId}";
        _cache.Remove(cacheKey);
    }
}
```

### 4. GraphQL Query Implementation

In `Program.cs:`

```csharp
builder.Services.AddScoped<PostService>();
```

### 5. Use PostService in Query Resolver

📄 File: `API/GraphQL/Query/PostQuery.cs`

```csharp
using HotChocolate;
using HotChocolate.Data;
using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraphQLDemo.API.GraphQL.Query
{
    [ExtendObjectType("Query")]
    public class PostQuery
    {
        /// <summary>
        /// Gets all posts with pagination, filtering, and sorting
        /// </summary>
        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Post> GetPosts([Service] AppDbContext context) => context.Posts;

        [UsePaging(IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IEnumerable<Post>> GetUserPostsAsync(Guid userId, [Service] PostService service)
        {
            return await service.GetPostsAsync(userId);
        }
    }
}
```

6. Clear Cache on Post Create/Delete/Update

📄 File: `GraphQLDemo.API/GraphQL/Mutations/PostMutation.cs`

```csharp
[ExtendObjectType(Name = "Mutation")]
public class PostMutation
{
    public async Task<Post> CreatePostAsync(
        string content, 
        Guid userId, 
        [Service] AppDbContext context,
        [Service] PostService postService)
    {
        var post = new Post
        {
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

    context.Posts.Add(post);
    await context.SaveChangesAsync();

        // Invalidate cache
        postService.InvalidateUserPostsCache(userId);

        return post;
    }

    public async Task<Post> UpdatePostAsync(
        Guid id,
        string content,
        [Service] AppDbContext context,
        [Service] PostService postService)
    {
        var post = await context.Posts.FindAsync(id);
        if (post == null)
            throw new NotFoundException("Post", id);

        post.Content = content;
        post.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // Invalidate cache
        postService.InvalidateUserPostsCache(post.UserId);

        return post;
    }

    public async Task<bool> DeletePostAsync(
        Guid id,
        [Service] AppDbContext context,
        [Service] PostService postService)
    {
        var post = await context.Posts.FindAsync(id);
        if (post == null)
            throw new NotFoundException("Post", id);

        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        // Invalidate cache
        postService.InvalidateUserPostsCache(post.UserId);

        return true;
    }
}
```

Do the same for update and delete.

## ✅ Best Practices

Use a consistent cache key naming convention.

Use `SetSlidingExpiration` or `SetAbsoluteExpiration` wisely.

Always invalidate the cache when writing data (create/update/delete).

Consider using distributed caching (e.g., Redis) for scale.
