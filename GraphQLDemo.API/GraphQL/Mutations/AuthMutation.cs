using GraphQLDemo.Infrastructure.Services;
using GraphQLDemo.Core.Inputs;
using GraphQLDemo.Core.Models;

namespace GraphQLDemo.API.GraphQL.Mutation;

[ExtendObjectType("Mutation")]
public class AuthMutation
{
    public async Task<User> RegisterAsync(RegisterInput input, [Service] AuthService authService)
        => await authService.RegisterAsync(input);

    public async Task<string> LoginAsync(LoginInput input, [Service] AuthService authService)
        => await authService.LoginAsync(input);
}
