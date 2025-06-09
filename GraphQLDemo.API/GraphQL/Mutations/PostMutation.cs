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