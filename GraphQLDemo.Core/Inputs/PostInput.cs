namespace GraphQLDemo.Core.Inputs;

public record CreatePostInput(
    string Content,
    Guid UserId
);

public record UpdatePostInput(
    Guid PostId,
    string NewContent
);


