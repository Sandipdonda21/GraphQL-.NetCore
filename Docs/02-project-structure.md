# 02 - Project Structure for GraphQL + .NET Core API

This section explains the modular folder structure used to organize the GraphQL API, following **clean architecture principles**.

---

## 📁 Root Structure Overview

```
GraphQLDemo/
│
├── GraphQLDemo.API/ → Main API project
├── GraphQLDemo.Core/ → Business/domain models
├── GraphQLDemo.Infrastructure/→ EF Core, DB access, services
├── GraphQLDemo.Tests/ → Unit & integration tests
└── Docs/ → Documentation (this folder)
```


---

## 📂 1. `GraphQLDemo.API/`

This is the **entry point** for the GraphQL application.

### Key files:

| File/Folder | Purpose |
|-------------|---------|
| `Program.cs` | 	Configures all services and middleware (minimal hosting model) |
| `appsettings.json` | Configuration (e.g. DB, JWT) |
| `GraphQL/` | Where all GraphQL logic resides |

### Inside `/GraphQL` folder:

| Folder | Description |
|--------|-------------|
| `Queries/` | GraphQL query definitions (read operations) |
| `Mutations/` | GraphQL mutation definitions (write operations) |
| `Types/` | GraphQL type definitions and resolvers |
| `Filters/` | Custom filtering and sorting logic |

---

## 📂 2. `GraphQLDemo.Core/`

Contains **pure business logic and domain models**.

| Folder | Description |
|--------|-------------|
| `Models/` | POCOs (Plain Old C# Objects) like `Post.cs`, `User.cs` |
| `Inputs/` | Input DTOs for GraphQL operations |
| `Validators/` | Validation logic for inputs and models |
| `Exceptions/` | Custom domain exceptions |

✅ **No infrastructure or GraphQL code should live here.**

---

## 📂 3. `GraphQLDemo.Infrastructure/`

Handles **data access** and persistence.

| Folder | Description |
|--------|-------------|
| `Data/` | `AppDbContext.cs`, migration setup |
| `Repositories/` | Repositories or EF queries |
| `Services/` | Implementation of business services |
| `Migrations/` | EF Core database migrations |
| `DependencyInjection.cs` | Registers services into the container |

---

## 📂 4. `GraphQLDemo.Tests/`

Unit tests and integration tests go here.

| Folder/File | Purpose |
|-------------|---------|
| `QueryTests.cs` | Tests for query resolvers |
| `MutationTests.cs` | Tests for mutation logic |
| `TestStartup.cs` | Optional startup override for test environment |

---

## 🧠 Benefits of This Structure

- 🔌 Decouples data logic (Infrastructure) from domain logic (Core)
- 📊 Keeps GraphQL-specific code modular and testable
- 🧪 Encourages clean testing practices
- 🔄 Swappable layers — you can swap SQL with Mongo or gRPC with GraphQL
- 🛡️ Clear separation of concerns with dedicated folders for inputs, validators, and exceptions

---

## 🗂 Bonus Tip: Suggested File Naming Convention

| Type | Naming Pattern |
|------|----------------|
| Query | `PostQuery.cs`, `UserQuery.cs` |
| Mutation | `PostMutation.cs`, `AuthMutation.cs` |
| Type | `PostType.cs`, `UserType.cs` |
| Input | `CreatePostInput.cs`, `UpdateUserInput.cs` |
| Validator | `PostValidator.cs`, `UserValidator.cs` |
| Context | `AppDbContext.cs` |

---

## Navigation
- [Next: 03-create-models-and-dbcontext →](03-create-models-and-dbcontext.md)
