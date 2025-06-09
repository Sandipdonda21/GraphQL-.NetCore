using GraphQLDemo.Core.Models;
using GraphQLDemo.Infrastructure.Data;
using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;

namespace GraphQLDemo.API.GraphQL.Query;

[ExtendObjectType("Query")]
public class UserQuery
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context) => context.Users;
}