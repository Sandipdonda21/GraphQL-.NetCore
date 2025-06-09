# 03 - Create Models and DbContext

We will now define the main entities of the social media app and configure `AppDbContext` using Entity Framework Core.

---

## 🧩 1. User Model

📁 `GraphQLDemo.Core/Models/User.cs`

```csharp
namespace GraphQLDemo.Core.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public required string PasswordHash { get; set; }
    public string Role { get; set; } = "User";
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
```

## 🧩 2. Post Model

📁 `GraphQLDemo.Core/Models/Post.cs`

```csharp
namespace GraphQLDemo.Core.Models;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
```

## 🧩 3. Comment Model

📁 `GraphQLDemo.Core/Models/Comment.cs`

```csharp
namespace GraphQLDemo.Core.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid PostId { get; set; }
    public Post Post { get; set; }
}
```

## 🧩 4. Like Model

📁 `GraphQLDemo.Core/Models/Like.cs`

```csharp
namespace GraphQLDemo.Core.Models;

public class Like
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid PostId { get; set; }
    public Post Post { get; set; }
}
```

## 🗃️ 5. AppDbContext

📁 `GraphQLDemo.Infrastructure/Data/AppDbContext.cs`

```csharp
using GraphQLDemo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLDemo.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Like> Likes => Set<Like>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User → Post: Allow cascade
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → Post: Allow cascade
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → User: Restrict to avoid conflict
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like → Post: Allow cascade
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Like → User: Restrict to avoid cascade conflict
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
```

## 🔧 6. Register DbContext in Program.cs

📁 `GraphQLDemo.API/Program.cs`

Add inside your `builder.Services` section:

```csharp
using GraphQLDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

And make sure you have this in your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GraphQLDemoDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## 🛠️ 7. Apply Migrations

Run the following commands:

```bash
# Move to API project
cd GraphQLDemo.API

# Create initial migration
dotnet ef migrations add InitialCreate --project ../GraphQLDemo.Infrastructure --startup-project .

# Apply to database
dotnet ef database update --project ../GraphQLDemo.Infrastructure --startup-project .
```

✅ Make sure you've installed `dotnet-ef` CLI if not already:

```bash
dotnet tool install --global dotnet-ef
```

## Navigation
- [Next: 04-setup-graphql-schema →](04-setup-graphql-schema.md)


