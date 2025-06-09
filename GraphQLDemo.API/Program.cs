using Serilog;

using GraphQLDemo.Infrastructure.Data;
using GraphQLDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using GraphQLDemo.Core.Validators;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("HotChocolate", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    var assembly = typeof(Program).Assembly;

    // Register all Query classes

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    builder.Services.AddMemoryCache();
    builder.Services.AddAuthorization();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<PostService>();

    builder.Services
        .AddGraphQLServer()
        .AddErrorFilter<GraphQLErrorFilter>() // Register custom error filter
        .AddAuthorization()
        .AddQueryType(d => d.Name("Query"))
        .AddMutationType(d => d.Name("Mutation"))
        // .AddTypes(d => d.Name("Type"))
        // Auto-register all Query types with [ExtendObjectType(Name = "Query")]
        .AddTypes(assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
                .Cast<ExtendObjectTypeAttribute>()
                .Any(attr => attr.Name == "Query"))
            .ToArray())
        // Auto-register all Mutation types with [ExtendObjectType(Name = "Mutation")]
        .AddTypes(assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
                .Cast<ExtendObjectTypeAttribute>()
                .Any(attr => attr.Name == "Mutation"))
            .ToArray())
        // Auto-register all type types with [ExtendObjectType(Name = "Type")]
        .AddTypes(assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(ExtendObjectTypeAttribute), false)
                .Cast<ExtendObjectTypeAttribute>()
                .Any(attr => attr.Name == "Type"))
            .ToArray())
             .AddProjections()
             .ModifyPagingOptions(options => options.IncludeTotalCount = true)
        .AddFiltering()
        .AddSorting();

    builder.Services.AddValidatorsFromAssemblyContaining<RegisterInputValidator>();

    var app = builder.Build();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<GraphQLDemo.API.Middleware.GraphQLRequestLoggingMiddleware>();

    app.UseRouting();

    app.MapGraphQL();
    Log.Information("Starting the application...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.CloseAndFlush();
}

