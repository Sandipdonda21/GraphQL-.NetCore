[← Back to Main README](../README.md)

# 01 - Environment Setup for GraphQL + .NET Core API

This guide helps you set up the required tools, create the base folder structure, and install packages for building a GraphQL API using ASP.NET Core and Entity Framework.

---

## 🧰 Step 1: Install Tools with Chocolatey

Install the following using [Chocolatey](https://chocolatey.org/install):

```bash
choco install dotnetcore-sdk
choco install vscode
choco install git
```

Optional: install a database engine like PostgreSQL or SQL Server

```bash
choco install postgresql
# OR
choco install sql-server-express
```

## ✅ Step 2: Verify .NET SDK

Ensure that .NET SDK is correctly installed:

```bash
dotnet --version
```

## 📁 Step 3: Create Folder & Project Structure

Open terminal and run:

```bash
mkdir GraphQLDemo
cd GraphQLDemo

dotnet new sln -n GraphQLDemo

dotnet new webapi -n GraphQLDemo.API
dotnet new classlib -n GraphQLDemo.Core
dotnet new classlib -n GraphQLDemo.Infrastructure
dotnet new xunit -n GraphQLDemo.Tests
```

## 🔗 Step 4: Add Projects to Solution

```bash
dotnet sln add GraphQLDemo.API/GraphQLDemo.API.csproj
dotnet sln add GraphQLDemo.Core/GraphQLDemo.Core.csproj
dotnet sln add GraphQLDemo.Infrastructure/GraphQLDemo.Infrastructure.csproj
dotnet sln add GraphQLDemo.Tests/GraphQLDemo.Tests.csproj
```

## 🔄 Step 5: Setup Project References

```bash
dotnet add GraphQLDemo.API/GraphQLDemo.API.csproj reference GraphQLDemo.Core/GraphQLDemo.Core.csproj
dotnet add GraphQLDemo.API/GraphQLDemo.API.csproj reference GraphQLDemo.Infrastructure/GraphQLDemo.Infrastructure.csproj
dotnet add GraphQLDemo.Tests/GraphQLDemo.Tests.csproj reference GraphQLDemo.API/GraphQLDemo.API.csproj
dotnet add GraphQLDemo.Infrastructure/GraphQLDemo.Infrastructure.csproj reference GraphQLDemo.Core/GraphQLDemo.Core.csproj
```

## 📦 Step 6: Install Required Packages

In the `GraphQLDemo.API` project:

```bash
cd GraphQLDemo.API
```

Install GraphQL and EF Core packages:

```bash
# GraphQL packages
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.AspNetCore.Authorization
dotnet add package HotChocolate.AspNetCore.Playground
dotnet add package HotChocolate.Data.EntityFramework
dotnet add package GraphQL.SystemTextJson

# EF Core packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

## 🧱 Step 7: Target Folder Structure

We will follow a clean architecture for scalable structure:

```sql
GraphQLDemo/
│
├── GraphQLDemo.API/          → ASP.NET Core entry project
│   ├── Program.cs
│   ├── appsettings.json
│   └── GraphQL/              → Schema, Types, Resolvers
│       ├── Queries/
│       ├── Mutations/
│       ├── Types/
│       └── Filters/
│
├── GraphQLDemo.Core/         → Domain models and interfaces
│   └── Models/
│
├── GraphQLDemo.Infrastructure/ → EF Core, DB context, services
│   ├── Data/
│   ├── Repositories/
│   └── DependencyInjection.cs
│
└── GraphQLDemo.Tests/        → Unit & integration tests
```

## Navigation
- [Next: 02-project-structure →](02-project-structure.md)