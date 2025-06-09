# 🔍 06 - Advanced Post Query with Pagination, Filtering & Sorting (Hot Chocolate Playground)

## 🎯 Goal
Perform an advanced `posts` query in GraphQL using:

- Filtering (`eq`, `lt`, `contains`, etc.)
- Sorting
- Pagination (`first`, `after`, `before`, `last`, `pageInfo`)
- Projections (selecting specific fields)

## 📂 Prerequisite Setup

Ensure your resolver method has the proper middleware:

```csharp
[UsePaging(IncludeTotalCount = true)]
[UseProjection]
[UseFiltering]
[UseSorting]
public IQueryable<Post> GetPosts([Service] AppDbContext context) => context.Posts;
```

And in your `Program.cs`, configure the GraphQL server with:

```csharp
builder.Services
    .AddGraphQLServer()
    .AddProjections()
    .SetPagingOptions(new PagingOptions
    {
        IncludeTotalCount = true
    })
    .AddFiltering()
    .AddSorting();
```

## 🧪 GraphQL Playground Query Examples

### ✅ Basic Pagination - First Page

```graphql
query {
  posts(first: 2) {
    totalCount
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    nodes {
      id
      content
      createdAt
      userId
      user {
        username
        email
      }
    }
  }
}
```

### 🔁 Fetch Next Page Using after

```graphql
query {
  posts(first: 2, after: "END_CURSOR_HERE") {
    nodes {
      id
      content
      createdAt
      userId
      user {
        username
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```
Replace "END_CURSOR_HERE" with the actual `endCursor` from the previous query.

### 🔙 Fetch Previous Page Using before

```graphql
query {
  posts(last: 2, before: "START_CURSOR_HERE") {
    nodes {
      id
      content
      createdAt
      userId
      user {
        username
      }
    }
    pageInfo {
      hasPreviousPage
      startCursor
    }
  }
}
```

## 🔎 Filtering Examples

### 1. Filter by exact `UserId`

```graphql
query {
  posts(where: { userId: { eq: "USER_GUID_HERE" } }) {
    nodes {
      id
      content
      createdAt
      user {
        username
      }
    }
  }
}
```

### 2. Filter by content (e.g., contains)

```graphql
query {
  posts(where: { content: { contains: "test" } }) {
    nodes {
      content
      createdAt
      user {
        username
      }
    }
  }
}
```

### 3. Filter by createdAt before a date

```graphql
query {
  posts(where: { createdAt: { lt: "2025-06-03T10:08:45.207Z" } }) {
    nodes {
      content
      createdAt
      user {
        username
      }
    }
  }
}
```

## ↕️ Sorting Example

```graphql
query {
  posts(order: { createdAt: DESC }) {
    nodes {
      id
      content
      createdAt
      user {
        username
      }
    }
  }
}
```

## 🔄 Combined Example (Pagination + Filtering + Sorting)

```graphql
query {
  posts(
    first: 5,
    where: { content: { contains: "test" } },
    order: { createdAt: DESC }
  ) {
    totalCount
    pageInfo {
      hasNextPage
      endCursor
    }
    nodes {
      id
      content
      createdAt
      user {
        username
        email
      }
    }
  }
}
```

## 📘 Summary

| Feature     | Status                                             |
|------------|---------------------------------------------------|
| Pagination | ✅ (`first`, `last`, `after`, `before`, `pageInfo`) |
| Filtering  | ✅ (`eq`, `lt`, `contains`, etc.)                   |
| Sorting    | ✅ (`order`)                                        |
| Projection | ✅ (select specific fields)                         |
| Relations  | ✅ (include related entities like user)             |


## Navigation
- [Next: 07-authentication-authorization →](07-authentication-authorization.md)
