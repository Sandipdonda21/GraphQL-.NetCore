using GraphQLDemo.Core.Models;
using HotChocolate.Types;

namespace GraphQLDemo.API.GraphQL.Types;

[ExtendObjectType("Type")]
public class PostType : ObjectType<Post>
{
    protected override void Configure(IObjectTypeDescriptor<Post> descriptor)
    {
        descriptor.Field(p => p.Comments)
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();

        descriptor.Field(p => p.Likes)
            .UsePaging()
            .UseProjection()
            .UseFiltering()
            .UseSorting();
    }
}
