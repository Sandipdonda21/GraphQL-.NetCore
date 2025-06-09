# GraphQL .NET Core API Demo

A comprehensive GraphQL API built with .NET Core, demonstrating best practices in API development, authentication, caching, and more.

## Features

- 🔐 **Authentication & Authorization**
  - JWT-based authentication
  - Role-based authorization
  - Secure password hashing

- 📝 **Blog-like Functionality**
  - Users can create and manage posts
  - Comment system(not implemented)
  - Like/Unlike functionality(not implemented)

- 🚀 **Performance Optimizations**
  - Response caching
  - Query optimization
  - Efficient data loading

- 🛠️ **Developer Experience**
  - Hot Chocolate GraphQL server
  - Entity Framework Core
  - Fluent Validation
  - Serilog logging

## Documentation

1. [Environment Setup](Docs/01-environment-setup.md)
   - Development environment setup
   - Required tools and packages
   - Initial configuration

2. [Project Structure](Docs/02-project-structure.md)
   - Solution organization
   - Project layers
   - Key components

3. [Create Models and DbContext](Docs/03-create-models-and-dbcontext.md)
   - Entity models
   - Database context
   - Entity relationships

4. [Setup GraphQL Schema](Docs/04-setup-graphql-schema.md)
   - Type definitions
   - Schema configuration
   - Type relationships

5. [CRUD Mutation in GraphQL](Docs/05-crud-mutation-in-graphQL.md)
   - Create operations
   - Update operations
   - Delete operations

6. [Advanced Get Query Playground](Docs/06-advanced-get-query-playground.md)
   - Query examples
   - Filtering
   - Sorting and pagination

7. [Authentication & Authorization](Docs/07-authentication-authorization.md)
   - JWT implementation
   - User registration
   - Login process

8. [Error Handling Best Practices](Docs/08-ErrorHandling-BestPractices.md)
   - Global error handling
   - Custom error types
   - Error logging

9. [Caching Strategy](Docs/9_Caching.md)
   - Memory cache implementation
   - Cache invalidation
   - Best practices

10. [Serilog Logging](Docs/10_Serilog_Logging.md)
    - Logging configuration
    - Request logging
    - Error logging

## Getting Started

1. Clone the repository
2. Navigate to the project directory
3. Run `dotnet restore`
4. Update the connection string in `appsettings.json`
5. Run `dotnet ef database update`
6. Start the application with `dotnet run`

## Requirements

- .NET 9.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

## License

MIT License 