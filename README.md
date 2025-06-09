# GraphQL .NET Core API Demo

A comprehensive GraphQL API built with .NET Core, demonstrating best practices in API development, authentication, caching, and more.

## Features

- 🔐 **Authentication & Authorization**
  - JWT-based authentication
  - Role-based authorization
  - Secure password hashing

- 📝 **Blog-like Functionality**
  - Users can create and manage posts
  - Comment system
  - Like/Unlike functionality

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

1. [Getting Started](Docs/1_GettingStarted.md)
   - Project setup
   - Environment configuration
   - Running the application

2. [Authentication](Docs/2_Authentication.md)
   - JWT implementation
   - User registration
   - Login process

3. [GraphQL Schema](Docs/3_GraphQLSchema.md)
   - Type definitions
   - Queries
   - Mutations

4. [Data Access](Docs/4_DataAccess.md)
   - Entity Framework setup
   - Database context
   - Migrations

5. [Validation](Docs/5_Validation.md)
   - Input validation
   - Error handling
   - Custom validators

6. [Error Handling](Docs/6_ErrorHandling.md)
   - Global error handling
   - Custom error types
   - Error logging

7. [Logging](Docs/7_Logging.md)
   - Serilog configuration
   - Request logging
   - Error logging

8. [Testing](Docs/8_Testing.md)
   - Unit tests
   - Integration tests
   - Test data setup

9. [Caching](Docs/9_Caching.md)
   - Response caching
   - Query caching
   - Cache invalidation

## Getting Started

1. Clone the repository
2. Navigate to the project directory
3. Run `dotnet restore`
4. Update the connection string in `appsettings.json`
5. Run `dotnet ef database update`
6. Start the application with `dotnet run`

## Requirements

- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

## License

MIT License 