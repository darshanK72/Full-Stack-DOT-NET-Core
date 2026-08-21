# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/15. GraphQL with HotChocolate`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [07. ASP.Net Core Web API/10. Authentication & Authorization in APIs](../10.%20Authentication%20&%20Authorization%20in%20APIs/.gitkeep) (JWT, policies — apply same auth concepts to GraphQL)

---

#### Q1. (R) Review this Hot Chocolate query type. APM logs 1 query for authors and 50 follow-up queries when the client requests 50 authors each with `books { title }`.

```csharp
public class Query
{
    public async Task<IEnumerable<Author>> GetAuthors([Service] AppDbContext db)
        => await db.Authors.ToListAsync();
}

public class AuthorType : ObjectType<Author>
{
    protected override void Configure(IObjectTypeDescriptor<Author> d)
    {
        d.Field(a => a.Books)
            .Resolve(async ctx =>
            {
                var db = ctx.Service<AppDbContext>();
                var authorId = ctx.Parent<Author>().Id;
                return await db.Books.Where(b => b.AuthorId == authorId).ToListAsync();
            });
    }
}
```

---

#### Q2. (R) Review this resolver registration in `Program.cs`. Under concurrent GraphQL requests, you see `ObjectDisposedException` on `AppDbContext` and occasional cross-request data leaks in logs.

```csharp
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(connectionString));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<AuthorType>()
    .AddType<BookType>();

// Query.cs — registered implicitly; AuthorType resolver uses ctx.Service<AppDbContext>()
public class Query
{
    private readonly AppDbContext _db;
    public Query(AppDbContext db) => _db = db;

    public Task<Author?> GetAuthorById(int id) =>
        _db.Authors.FirstOrDefaultAsync(a => a.Id == id);
}
```

A junior developer also registered `Query` as **Singleton** "because it has no mutable state."

---

#### Q3. (R) Review production GraphQL exposure. Security scan flags `/graphql` — anonymous clients can POST deeply nested queries; one request pinned CPU at 100% for two minutes.

```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddType<CompanyType>()
    .AddType<DepartmentType>()
    .AddType<EmployeeType>()
    .AddType<ProjectType>()
    .AddType<TaskType>();

app.MapGraphQL("/graphql");
```

No `[Authorize]`, no depth limit, no complexity budget. `EmployeeType` resolves `manager { manager { manager { ... } } }` and `projects { tasks { assignee { ... } } }`.

---

#### Q4. (R) Review this schema configuration for production vs development. Pen testers retrieved the full schema and built queries for admin-only fields using introspection.

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGraphQL("/graphql").WithOptions(new GraphQLServerOptions
    {
        Tool = { Enable = true }
    });
}
else
{
    app.MapGraphQL("/graphql");
}

// Admin fields on Query — no field-level auth yet
public class Query
{
    public Task<IEnumerable<User>> GetAllUsers([Service] AppDbContext db) =>
        db.Users.ToListAsync();

    public Task<decimal> GetPayrollTotal([Service] PayrollService payroll) =>
        payroll.GetCompanyTotalAsync();
}
```

Banana Cake Pop is disabled in production, but introspection is still enabled by default.

---

#### Q5. (P) Explain how **DataLoader** fixes the N+1 pattern in Q1. What does batching look like at the SQL level, and where do you register loaders in Hot Chocolate (`AddDataLoader`, scoped lifetime)?

---

#### Q6. (D) A product owner asks: "We already have REST — why add GraphQL?" Compare **over-fetching / under-fetching** trade-offs for a mobile app that needs user profile + last 5 orders + avatar URL. When would you keep REST, when GraphQL, when both?

---

#### Q7. (M) You must enforce authorization on GraphQL — some fields are public, `GetPayrollTotal` is admin-only, and users may only read their own `orders`. Compare **ASP.NET Core policy on the request**, **Hot Chocolate `[Authorize]` on fields**, and **manual checks inside resolvers**. What fails if you only put `[Authorize]` on the controller equivalent?

---

#### Q8. (R) Review this mutation and follow-up query in one GraphQL request. Clients report stale `inventoryCount` on `Product` immediately after `updateStock` succeeds.

```csharp
public class Mutation
{
    public async Task<Product> UpdateStock(
        int productId,
        int delta,
        [Service] AppDbContext db)
    {
        var product = await db.Products.FindAsync(productId);
        product!.Stock += delta;
        await db.SaveChangesAsync();
        return product;
    }
}

// Client document (single request):
// mutation { updateStock(productId: 1, delta: -2) { id stock } }
// { product(id: 1) { id stock } }
```

`Product` field resolver reads from the same scoped `AppDbContext` without `AsNoTracking`; EF change tracker already holds the entity from the mutation.

---
