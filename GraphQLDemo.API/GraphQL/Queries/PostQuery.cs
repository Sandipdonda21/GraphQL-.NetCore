using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using HotChocolate;
using HotChocolate.Data;

namespace GraphQLDemo.API.GraphQL.Query;

[ExtendObjectType("Query")]
public class PostQuery
{
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