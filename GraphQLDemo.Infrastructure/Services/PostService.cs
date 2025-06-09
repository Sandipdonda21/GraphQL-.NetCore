using Microsoft.Extensions.Caching.Memory;
using GraphQLDemo.Infrastructure.Data;
using GraphQLDemo.Core.Models;
using Microsoft.EntityFrameworkCore;

public class PostService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

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
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, posts, cacheEntryOptions);
        }

        return posts!;
    }
}
