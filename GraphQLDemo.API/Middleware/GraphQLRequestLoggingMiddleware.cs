using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Serilog;

namespace GraphQLDemo.API.Middleware;

public class GraphQLRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public GraphQLRequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/graphql") || context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
        context.Request.Body.Position = 0;

        string queryText = "N/A";
        string userEmail = "Anonymous";

        // Extract query from body
        try
        {
            var doc = JsonDocument.Parse(requestBody);
            if (doc.RootElement.TryGetProperty("query", out var queryElement) &&
                queryElement.ValueKind == JsonValueKind.String)
            {
                queryText = queryElement.GetString() ?? "EmptyQuery";
            }
        }
        catch (Exception ex)
        {
            queryText = $"Invalid JSON: {ex.Message}";
        }

        // Extract user email/id from token
        var user = context.User;
        userEmail = user?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

        var originalBody = context.Response.Body;
        await using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        string resultStatus = "Success";

        try
        {
            await _next(context);

            // If the GraphQL response contains "errors", it's a failed query.
            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            if (responseBody.Contains("\"errors\""))
                resultStatus = "Fail";

            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
        }
        catch (Exception)
        {
            resultStatus = "Fail";
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
            stopwatch.Stop();

            Log.Information("GraphQL Operation: {Query}\nTime: {Elapsed} ms\nUser: {User}\nStatus: {Status}",
                queryText.Trim(), stopwatch.ElapsedMilliseconds, userEmail, resultStatus);
        }
    }
}
