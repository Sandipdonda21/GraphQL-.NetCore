using GraphQLDemo.Core.Exceptions;
using HotChocolate;
using HotChocolate.Execution;
using System.Collections.Generic;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ValidationException vex)
        {
            // Convert errors into GraphQL-serializable object
            var serializableErrors = new Dictionary<string, object>();
            foreach (var kv in vex.Errors)
            {
                serializableErrors[kv.Key] = kv.Value;
            }

            return error
                .WithMessage("Validation failed.")
                .SetExtension("validationErrors", serializableErrors);
        }

        if (error.Exception != null)
        {
            return error
                .WithMessage("Unexpected error occurred.")
                .SetExtension("errorType", error.Exception.GetType().Name)
                .SetExtension("details", error.Exception.Message);
        }

        return error;
    }
}
