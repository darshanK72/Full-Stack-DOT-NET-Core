# GraphQL with HotChocolate — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is GraphQL?](#q1-what-is-graphql)
2. [Q2. What is the difference between GraphQL and REST?](#q2-what-is-the-difference-between-graphql-and-rest)
3. [Q3. What is a GraphQL schema?](#q3-what-is-a-graphql-schema)
4. [Q4. What is a GraphQL query vs mutation?](#q4-what-is-a-graphql-query-vs-mutation)
5. [Q5. What is a resolver in GraphQL?](#q5-what-is-a-resolver-in-graphql)
6. [Q6. What is the N+1 problem in GraphQL?](#q6-what-is-the-n1-problem-in-graphql)
7. [Q7. What is DataLoader in Hot Chocolate?](#q7-what-is-dataloader-in-hot-chocolate)
8. [Q8. What is over-fetching in REST vs GraphQL?](#q8-what-is-over-fetching-in-rest-vs-graphql)
9. [Q9. What is under-fetching in REST?](#q9-what-is-under-fetching-in-rest)
10. [Q10. What is Hot Chocolate?](#q10-what-is-hot-chocolate)
11. [Q11. What is GraphQL introspection?](#q11-what-is-graphql-introspection)
12. [Q12. What is query depth limiting?](#q12-what-is-query-depth-limiting)
13. [Q13. What is query complexity in GraphQL?](#q13-what-is-query-complexity-in-graphql)
14. [Q14. How does authorization work on GraphQL fields?](#q14-how-does-authorization-work-on-graphql-fields)
15. [Q15. What is `AddGraphQLServer`?](#q15-what-is-addgraphqlserver)
16. [Q16. What is `MapGraphQL`?](#q16-what-is-mapgraphql)
17. [Q17. What is Banana Cake Pop?](#q17-what-is-banana-cake-pop)
18. [Q18. When would you choose GraphQL over REST for an API?](#q18-when-would-you-choose-graphql-over-rest-for-an-api)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is GraphQL?

**Concepts**
- Client-driven field selection in a single request
- Typed schema defining Query, Mutation, and Subscription roots
- Single /graphql endpoint replacing many REST resources
- Hot Chocolate as the .NET GraphQL server

**Answer**

GraphQL is a query language and runtime for APIs where clients request exactly the fields they need in a single POST to a `/graphql` endpoint. A typed schema defines queries (reads), mutations (writes), and subscriptions (real-time pushes), with server-side resolvers backing each field. The reason this matters is that diverse clients — mobile, web, third-party — can each request exactly the shape they need from one endpoint rather than having the server decide what to return. Hot Chocolate is the common GraphQL server for ASP.NET Core and integrates with the standard DI and middleware pipeline.

---

## Q2. What is the difference between GraphQL and REST?

**Concepts**
- REST resource-oriented URLs vs single GraphQL endpoint
- Over-fetching with fixed REST response shapes
- Under-fetching requiring multiple REST round trips
- HTTP caching advantage of REST GET requests
- Query complexity attacks unique to GraphQL

**Answer**

REST exposes many resource-oriented URLs with fixed response shapes per endpoint; GraphQL exposes one endpoint where the client selects nested fields in one request. REST uses HTTP verbs and status codes per resource; GraphQL typically POSTs to `/graphql` and returns 200 with errors in the body even for partial failures. REST over-fetches when endpoints return more fields than the client needs, and under-fetches when a screen needs data from multiple endpoints requiring chained requests. GraphQL shifts those problems to the server — resolvers, N+1, and query cost limits — while REST keeps endpoints simpler and benefits from HTTP caching semantics that GraphQL POST requests cannot use natively.

---

## Q3. What is a GraphQL schema?

**Concepts**
- Schema as typed contract for all queryable fields
- Resolver method backing each field
- Introspectable schema via __schema and __type
- AddGraphQLServer building the executable schema from C# types

**Answer**

The schema is the contract defining all types, fields, arguments, and root operations (`Query`, `Mutation`, `Subscription`) clients may request. Hot Chocolate builds the schema from C# types, attributes, and fluent configuration at startup, so the contract is always in sync with the code. Each field maps to a resolver method or property that fetches data, and the schema is introspectable — tools query `__schema` and `__type` unless introspection is disabled in production. Breaking changes like removing fields require versioning or deprecation policies, just as REST versioning does for breaking contract changes.

---

## Q4. What is a GraphQL query vs mutation?

**Concepts**
- Query as safe, side-effect-free read operation
- Mutation as write operation with serial execution
- JSON body {"query": "..."} for both operations
- Side-effect placement matching HTTP safe/idempotent mental model

**Answer**

Queries are read operations that fetch data without side effects; mutations are write operations that create, update, or delete data. By convention, queries may run in parallel within a request; mutations run serially to avoid race conditions on related writes, which is why the distinction matters under concurrent execution. Both are POSTed to the GraphQL endpoint with a JSON body `{ "query": "..." }`. Side-effect-free reads belong on `Query`, not `Mutation` — keeping that discipline aligns GraphQL semantics with the HTTP safe/idempotent mental model and makes client caching logic easier to reason about.

---

## Q5. What is a resolver in GraphQL?

**Concepts**
- Resolver as per-field data fetching function
- Parent object, arguments, and injected services in resolver context
- Root vs nested field resolver responsibility
- Scoped DbContext lifetime alignment with resolvers

**Answer**

A resolver is the function that returns the value for a single field in the schema — given the parent object, field arguments, and request context. In Hot Chocolate, resolver methods on query types or `[GraphQLName]` methods execute per field selection. Root query resolvers load entry points; nested field resolvers load related data, such as an author's books. Resolvers receive `[Parent]`, `[Argument]`, and injected services via `[Service] AppDbContext`, and since they run within a request scope I need to align DI lifetimes accordingly — a scoped `DbContext` is the standard choice. Naive per-row database access in nested resolvers causes N+1 query explosions, which is the most common GraphQL production problem.

---

## Q6. What is the N+1 problem in GraphQL?

**Concepts**
- Root query returning N parents, each triggering child query
- APM trace showing 1 + N SQL round trips
- DataLoader batching as the fix
- Easier to trigger than REST because clients control field depth

**Answer**

GraphQL N+1 occurs when a list field resolver runs a separate database query for each parent item — loading 50 authors triggers 50 additional queries for each author's books. Without batching, GraphQL list queries can perform far worse than a well-designed REST join endpoint. The symptom is visible in APM traces as one query for the parent list plus N queries for each child field. The fix is DataLoader batching or eager loading at the root when the selection set is known. The problem is easier to trigger in GraphQL than in REST because clients control field depth, so a client adding a new nested selection can inadvertently introduce N+1 queries with no server-side change.

---

## Q7. What is DataLoader in Hot Chocolate?

**Concepts**
- DataLoader batching keys within a single request
- Single WHERE IN query replacing N individual queries
- BatchDataLoader and GroupedDataLoader base classes
- Request-scoped cache preventing duplicate loads

**Answer**

DataLoader batches and caches loads within a single GraphQL request — collecting keys requested by field resolvers and issuing one query (`WHERE Id IN (...)`) instead of many. Hot Chocolate provides `BatchDataLoader`, `GroupedDataLoader`, and `CacheDataLoader` base classes. I register loaders in DI as scoped services and call `LoadAsync(key)` from field resolvers; the request-scoped cache prevents duplicate loads when the same key appears in multiple branches of the query. `.AddDataLoader<T>()` integrates loaders with the Hot Chocolate execution engine. DataLoader is essential for production GraphQL APIs exposing nested relational data from EF Core.

---

## Q8. What is over-fetching in REST vs GraphQL?

**Concepts**
- Fixed REST endpoint shape returning unused fields
- GraphQL client-selected field minimization
- REST sparse field parameters as partial mitigation
- Server-side resolver still loading full entity without projection

**Answer**

Over-fetching happens when an API response includes more data than the client needs — REST list endpoints returning full entity graphs with columns and navigations the UI ignores. GraphQL lets clients specify fields, reducing payload size when clients request minimal selections. A REST `GET /api/users/1` always returns the same DTO shape regardless of whether the UI needs only `name`; a GraphQL `{ user { name } }` omits email, address, and permissions entirely. REST can partially mitigate this with sparse field parameters or separate lightweight endpoints, but that adds API surface area. GraphQL does not eliminate over-fetching on the server side if resolvers still load full entities before projecting — the benefit comes from combining field selection with server-side projection in resolvers.

---

## Q9. What is under-fetching in REST?

**Concepts**
- Multiple REST round trips to assemble one screen
- BFF aggregation as an alternative to GraphQL
- GraphQL nested queries collapsing round trips
- REST _embed and include parameters as partial mitigation

**Answer**

Under-fetching occurs when a client needs data from multiple REST endpoints and must chain requests — for example, orders, then customers, then products for a dashboard. Each round trip adds latency and complicates mobile apps on slow networks. A screen needing five resources may require five or more HTTP calls with REST. GraphQL nested queries fetch related data in one request when resolvers are efficient and DataLoader-backed. A BFF (Backend for Frontend) is an alternative that aggregates REST calls server-side, achieving the same round-trip reduction without introducing GraphQL's operational complexity.

---

## Q10. What is Hot Chocolate?

**Concepts**
- Hot Chocolate as the leading .NET GraphQL server
- AddGraphQLServer() and MapGraphQL() integration points
- DataLoader, filtering/sorting, and Banana Cake Pop IDE
- Apollo Federation and Relay global object identification support

**Answer**

Hot Chocolate is a high-performance GraphQL server for .NET that integrates with ASP.NET Core through `AddGraphQLServer()` and `MapGraphQL()`. It provides schema-first and code-first modeling, DataLoader, authorization, filtering/sorting, and the Banana Cake Pop IDE. It supports global object identification (Relay), subscriptions via WebSockets, and Apollo Federation for schema stitching across services. The NuGet package `HotChocolate.AspNetCore` provides the web hosting integration, and Hot Chocolate is the actively maintained ecosystem leader on .NET since taking over from GraphQL.NET.

---

## Q11. What is GraphQL introspection?

**Concepts**
- __schema and __type meta-fields exposing schema at runtime
- Tooling dependency on introspection
- Production introspection as attack surface
- Disabling UI vs disabling introspection distinction

**Answer**

Introspection lets clients query the schema itself — listing types, fields, arguments, and descriptions via meta-fields like `__schema` and `__type`. Tools like Banana Cake Pop, GraphiQL, and client code generators depend on it to discover the API surface. The risk is that attackers use it to enumerate hidden admin fields in production. I disable or restrict introspection for anonymous users in production environments — importantly, hiding Banana Cake Pop UI does not disable introspection, since clients can still POST introspection queries directly. I pair introspection restrictions with field-level authorization on sensitive resolvers so that even if the schema is known, unauthorized fields are enforced.

---

## Q12. What is query depth limiting?

**Concepts**
- Depth limit capping nested query traversal levels
- AddMaxExecutionDepth(n) in Hot Chocolate
- Deep recursive schema as exponential resolver work vector
- Depth limit complementing complexity analysis

**Answer**

Query depth limiting caps how many nested levels a GraphQL query may traverse, blocking deeply recursive queries like `{ a { b { c { d { ... } } } } }` that exponentially expand resolver work. Hot Chocolate provides `AddMaxExecutionDepth(n)` to enforce limits at execution time before resolvers run. Deep recursive schemas — comments on comments, org hierarchies — are abuse vectors without this limit. Depth limits complement complexity/cost analysis, since depth alone does not catch wide fan-out at a single level. I tune the limit against legitimate client queries; mobile apps rarely need depth above 10–15, so a limit of 15 is a reasonable starting point for most schemas.

---

## Q13. What is query complexity in GraphQL?

**Concepts**
- Cost score assigned per field to cap expensive queries
- List fields multiplying cost by child count
- Hot Chocolate cost analysis middleware
- Combination with rate limiting and authentication for public endpoints

**Answer**

Query complexity assigns a cost score to each field and rejects queries exceeding a budget, penalizing wide lists and expensive resolvers. Hot Chocolate supports cost analysis middleware to prevent clients from requesting `users { friends { friends { friends } } }` at scale. Each field contributes weight, and list fields multiply cost by expected or actual child count, which is why wide shallow queries are caught where depth limits would not apply. Complexity rules should reflect real database cost rather than arbitrary constants so the budget is meaningful. I combine complexity limits with rate limiting and authentication for public GraphQL endpoints, since no single mechanism is sufficient on its own.

---

## Q14. How does authorization work on GraphQL fields?

**Concepts**
- [Authorize] on query types, mutation classes, or individual resolvers
- Field-level auth for mixed public/private schema
- GraphQL errors array for unauthorized fields vs HTTP 401
- Introspection still revealing field names despite auth

**Answer**

Hot Chocolate integrates ASP.NET Core authorization — I apply `[Authorize]` on query types, mutation classes, or individual field resolvers. Policies and roles evaluate per field, so a public `Query` type can expose both anonymous catalog fields and admin-only fields with different auth requirements on the same schema. Unauthorized fields return GraphQL errors in the `errors` array; the HTTP status may remain 200, since GraphQL partial responses are still 200 by convention. I use `[Authorize(Roles = "Admin")]` or named policies matching my REST API auth configuration. Introspection may still reveal field names even when auth is enforced on resolvers — security through obscurity is insufficient, which is why I also restrict introspection in production.

---

## Q15. What is `AddGraphQLServer`?

**Concepts**
- AddGraphQLServer() registering executor and schema builder in DI
- IRequestExecutorBuilder fluent chain for types and middleware
- Logging, DataLoader scopes, and Apollo tracing integration
- Required counterpart to MapGraphQL() endpoint mapping

**Answer**

`AddGraphQLServer()` registers Hot Chocolate's GraphQL executor, schema builder, and supporting services in DI. I call it in `Program.cs` and chain configuration methods to register query types, mutations, DataLoaders, filtering, and instrumentation: `builder.Services.AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>()`. It returns `IRequestExecutorBuilder` for fluent registration of types, directives, and middleware. It integrates with ASP.NET Core logging and DataLoader scopes, and it is the required counterpart to `MapGraphQL()` which maps the HTTP endpoint — neither works without the other.

---

## Q16. What is `MapGraphQL`?

**Concepts**
- MapGraphQL() routing POST /graphql to Hot Chocolate executor
- WebSocket endpoint for subscriptions
- GraphQLServerOptions for Banana Cake Pop tool enablement
- Placement after auth middleware requirement

**Answer**

`MapGraphQL()` adds endpoint routing for the GraphQL HTTP transport — typically POST `/graphql` — and optionally WebSocket endpoints for subscriptions, connecting incoming requests to Hot Chocolate's executor pipeline. I call `app.MapGraphQL()` after `app.Build()`, and I can customize the path with `MapGraphQL("/api/graphql")`. In Development I enable Banana Cake Pop with `.WithOptions(new GraphQLServerOptions { Tool = { Enable = true } })`. I place `MapGraphQL` after auth middleware when endpoints require authenticated access, since the authorization middleware must run before the GraphQL executor sees the request.

---

## Q17. What is Banana Cake Pop?

**Concepts**
- Banana Cake Pop as Hot Chocolate's built-in GraphQL IDE
- Browser UI for schema exploration and query execution
- Disable or restrict in Production to prevent schema exposure
- Not a substitute for introspection restrictions on public endpoints

**Answer**

Banana Cake Pop is Hot Chocolate's built-in GraphQL IDE — a browser UI for exploring the schema, writing queries, and viewing responses. It replaces GraphQL Playground in modern Hot Chocolate versions and ships embedded with the server in Development. It is accessible at `/graphql` when tooling is enabled, playing a similar role to Swagger UI for REST APIs. I disable or restrict it in Production to avoid exposing schema details and ad-hoc query execution. Banana Cake Pop being off does not disable introspection — clients can still POST introspection queries directly, so both the UI and introspection need separate production controls.

---

## Q18. When would you choose GraphQL over REST for an API?

**Concepts**
- Diverse client field needs as primary GraphQL justification
- REST advantage for CDN caching and simple CRUD
- DataLoader and complexity limits as operational investment
- Hybrid architecture — REST at edge, GraphQL as internal BFF

**Answer**

I choose GraphQL when diverse clients — mobile, web, third-party — need flexible, nested data shapes from one endpoint and the team can invest in DataLoader batching, query limits, and resolver DI. Good fits include product catalog with user and cart screens having different field needs, or rapid frontend iteration without new REST endpoints per screen. I prefer REST when CDN caching, binary uploads, strict per-route rate limiting, or partner integrations via standard HTTP semantics matter more than client-driven field selection, or when the team lacks GraphQL operational experience. Hybrid architectures expose REST at the edge for caching and simplicity, and GraphQL internally behind a BFF for aggregation — ASP.NET Core supports both in one host, so the choice does not have to be all-or-nothing.

---

## Gotchas — GraphQL with HotChocolate (Interview Traps)

---

#### Gotcha 1. N+1 queries without DataLoader

**Concepts**
- Field resolver executing one database query per parent object
- 100 authors triggering 100 separate `SELECT books WHERE authorId = ?` queries
- DataLoader batching concurrent resolver calls into a single parameterized query
- `IDataLoader<TKey, TValue>` scoped to request, deduplicating repeated keys

**Answer**

Without DataLoader, a `books` field resolver on `Author` executes one `SELECT` per author — a list query returning 100 authors triggers 101 database round-trips. DataLoader solves this by collecting all the author IDs requested during one execution phase and dispatching a single `SELECT ... WHERE authorId IN (...)`. I register `BooksByAuthorDataLoader` in DI, inject it into the type's resolver, and `await loader.LoadAsync(author.Id)`. The DataLoader is request-scoped so concurrent resolver calls within the same request are batched; repeated requests for the same key within one request are deduplicated automatically.

---

#### Gotcha 2. Field-level `[Authorize]` bypassed by `UseProjection`

**Concepts**
- `[Authorize]` on a field — requires the caller to have permission to resolve that field
- `UseProjection()` with `Include` — EF Core may eagerly load the related data regardless of auth check
- Authorized field accessible via an included navigation despite not being resolved by the client
- Field authorization and projection must both be enforced

**Answer**

`[Authorize]` on a HotChocolate field prevents the field's resolver from executing for unauthorized callers. However, `UseProjection()` with EF Core may load the related navigation property via an `Include` regardless of whether the field is requested or authorized, and a resolver that returns the parent entity exposes the unauthorized navigation through JSON serialization. The field authorization check runs at the resolver level, not the database level. I ensure that projections exclude unauthorized fields by checking authorization before including navigations, or by using separate queries for authorized and unauthorized contexts rather than relying solely on field-level decorators.

---

#### Gotcha 3. Schema introspection enabled in production

**Concepts**
- Introspection query — `{ __schema { types { name } } }` reveals full schema
- Production exposure — enumerates all types, fields, and their nullability
- `options.EnableSchemaIntrospection = false` (or environment gate) for production
- Persisted queries as an alternative to disabling introspection for authorized clients

**Answer**

GraphQL schema introspection exposes the complete type system — every query, mutation, field name, argument, and type nullability — to any client that can reach the endpoint. This is valuable for developers but a reconnaissance goldmine in production. I disable introspection in non-development environments with `options.EnableSchemaIntrospection = builder.Environment.IsDevelopment()` in the HotChocolate configuration, or gate the introspection endpoint behind an authorization policy so only authenticated internal developers can enumerate the schema.

---

#### Gotcha 4. Mutation not idempotent — duplicate execution on network retry

**Concepts**
- Mutations in GraphQL — not idempotent by specification
- Client retry on network timeout — mutation may execute twice
- Idempotency key in mutation input type — server deduplicates on key
- `createOrder` executed twice triggering duplicate charges

**Answer**

GraphQL mutations are not idempotent by design. A network timeout after the mutation executes but before the client receives the response causes a client retry that creates a duplicate order or charge. I add an `idempotencyKey: String!` field to mutation input types for state-changing operations (create, charge, send). The resolver checks whether a result for that key already exists and returns the cached result instead of re-executing the mutation, giving clients safe retry semantics. This is the GraphQL equivalent of the `Idempotency-Key` header pattern used in REST APIs.

---

#### Gotcha 5. DataLoader not scoped to request — batching across requests

**Concepts**
- `IDataLoader` should be request-scoped — one batch per execution
- Singleton DataLoader — accumulates keys across requests, batches from different users mixed
- Transient DataLoader — no batching benefit; creates new loader on every injection
- `[UseDataLoader]` / `[BindService]` with correct lifetime in DI

**Answer**

`IDataLoader` must be scoped to the request to batch all resolver calls for a single execution. A singleton DataLoader accumulates keys across concurrent requests and may batch data from different tenants together — a multi-tenancy security issue where one user's DataLoader resolves another's data. A transient DataLoader creates a new instance on every injection, providing no batching since each resolver call gets its own isolated loader. I register DataLoaders as scoped services so they live exactly one request and batch only the keys requested within that execution.

---

#### Gotcha 6. Missing pagination on large collections — schema returns full list

**Concepts**
- `IQueryable<Book>` field without `[UsePaging]` — resolves entire table by default
- `[UsePaging]` — HotChocolate cursor-based pagination via Relay spec connection type
- `[UseOffsetPaging]` — offset-based `page`/`pageSize` pagination
- Unpaginated field — memory exhaustion and slow response under realistic data volumes

**Answer**

A HotChocolate field that returns `IQueryable<Book>` without `[UsePaging]` or `[UseOffsetPaging]` executes a query that fetches the entire table. In development with seeded data (100 rows) the field is fast; in production with 10 million rows it causes an out-of-memory exception or a multi-second query that blocks the thread pool. I apply `[UsePaging]` on collection fields by default and set a maximum page size in the global options: `options.SetMaxPageSize(100)`. Callers must specify a `first` or `last` argument; the field raises an error if pagination arguments are missing.

---

#### Gotcha 7. Subscription without a distributed backplane — single-node only

**Concepts**
- In-memory subscription provider — events only reach subscribers on the same pod
- Multi-pod deployment — subscriber connected to Pod A misses event published on Pod B
- Redis or Azure Service Bus backplane for cross-pod subscription delivery
- `AddRedisSubscriptions()` in HotChocolate for distributed pub/sub

**Answer**

The default HotChocolate subscription provider uses an in-process pub/sub mechanism — an event published on Pod A is only delivered to subscribers connected to Pod A. In a multi-pod deployment, a client connected to Pod B subscribed to `orderUpdated` never receives events published on Pod A. I add `AddRedisSubscriptions()` (or the Azure Service Bus provider) which routes events through a shared message broker, ensuring all pods receive all events and deliver them to their local subscribers. For single-pod deployments the in-memory provider is fine, but any horizontal scaling requires the distributed backplane.

---

#### Gotcha 8. `IError` vs exception handling in resolvers

**Concepts**
- Throwing `Exception` from resolver — HotChocolate converts to generic error, hides detail
- `IError` / `ErrorBuilder` — structured error with code, message, and extensions
- `GraphQLException` — wraps one or more `IError` instances
- `QueryError` vs field error — field error does not abort the entire query

**Answer**

Throwing an unhandled `Exception` from a resolver causes HotChocolate to return a generic error message with the exception details hidden (in non-development environments) to prevent leaking internal information. For expected domain errors (resource not found, permission denied, business rule violation), I return structured `IError` results using `ErrorBuilder.New().SetMessage("Order not found").SetCode("ORDER_NOT_FOUND").Build()` wrapped in `new GraphQLException(error)`. Field-level errors allow the rest of the query to succeed while the failing field returns null with error information in the `errors` array, which is the correct GraphQL behavior for partial success.

---

#### Gotcha 9. Non-nullable GraphQL type on a nullable EF Core column

**Concepts**
- `String!` in GraphQL schema — guarantees a non-null value; runtime null throws
- EF Core column allowing NULL — resolver may return `null` for `String!` field
- HotChocolate enforcing non-nullability — throws `UnexpectedErrorException` at runtime
- C# nullable annotations on resolver return types controlling schema nullability

**Answer**

A HotChocolate field declared as `String!` (non-nullable) throws at runtime if the resolver returns `null`. When the underlying EF Core entity has a nullable column (e.g. `Address?`), a resolver returning `entity.Address` can produce `null` for the `String!` field, causing HotChocolate to replace the field value with an error and propagate nullability up through parent fields. I align schema nullability with data reality: optional EF Core columns map to nullable `String` GraphQL fields, and I add `[Required]` or a not-null constraint at the database level if the field should truly never be null in the schema contract.

---

#### Gotcha 10. Schema stitching exposing internal service errors to clients

**Concepts**
- Schema stitching / gateway — combines multiple downstream GraphQL services
- Downstream service error — forwarded to client with internal service detail
- `RequestExecutorBuilder.AddRemoteSchema` — stitches remote schema
- Error transformation — strip internal fields before forwarding to external clients

**Answer**

In a stitched GraphQL gateway, an error from a downstream internal service (including stack traces, internal service names, or sensitive error codes) is forwarded directly to the external client unless an error transformation is applied at the gateway layer. I add an error filter on the gateway that strips or replaces internal error extensions before the response is sent to the client — similar to the ProblemDetails sanitization pattern in REST APIs. Only safe fields (`message`, `code`, client-facing `locations`, and `path`) are forwarded; `exception`, `stackTrace`, and internal `extensions` are removed for unauthenticated or external callers.

---

## Scenario-Based Questions (Karat Format)

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

**Concepts**
- Field resolver running once per parent author — N+1 pattern
- BatchDataLoader<int, Book[]> collecting keys before querying
- AddDataLoader registration in Hot Chocolate
- Root-level Include as simpler alternative for fixed selection sets

**Answer**

The `Books` field resolver runs once per parent author and issues a separate SQL query each time, which is the GraphQL N+1 pattern. Root `GetAuthors` loads all authors in one query, then each nested `books` field resolution hits the database again — 50 authors produce 51 queries. The fix is a `BatchDataLoader<int, IReadOnlyList<Book>>`: each field resolver calls `loader.LoadAsync(authorId)`, the loader collects all author IDs until the execution engine yields, then issues one `WHERE AuthorId IN (@p0, @p1, ...)` query and distributes results back to the individual awaiters. I register it with `builder.AddGraphQLServer().AddDataLoader<AuthorBooksDataLoader>()` and ensure it is scoped per request so the batch cache does not leak across operations. For simple cases where the client always requests books together with authors, projecting at the root with a single EF `Include` is also valid, but it loads books even when the client did not select that field — DataLoader is more efficient when selection varies.

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

**Concepts**
- Singleton Query capturing scoped DbContext — captive dependency
- Hot Chocolate per-request query type resolution
- ValidateScopes fail-fast at startup
- Resolver DI lifetime — scoped for data access, singleton for pure functions

**Answer**

Registering `Query` as a singleton while it holds a scoped `AppDbContext` creates a captive dependency — the context is created once and shared across all concurrent requests rather than being per-request. Since `DbContext` is not thread-safe and has a limited lifetime, this causes `ObjectDisposedException` when the context from a long-lived singleton is used after it would normally have been disposed, and cross-request data leaks when change tracker state bleeds between concurrent requests. Hot Chocolate resolves query types per request by default when they are registered through the server builder — the junior developer's manual singleton registration overrides that behavior incorrectly. I would remove the `AddSingleton<Query>()` call entirely and let Hot Chocolate manage the type's lifetime. I would also enable `ValidateScopes` on the host builder so that captive dependency mistakes like this throw at startup rather than manifesting at runtime under load. Any service doing EF data access must be scoped; singleton is reserved for stateless infrastructure like pure-function helpers or caches.

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

**Concepts**
- GraphQL as programmable query API requiring explicit cost controls
- AddMaxExecutionDepth for depth limiting
- Cost analysis middleware for wide fan-out protection
- Anonymous access amplifying abuse surface

**Answer**

GraphQL exposes a programmable query API — unlike REST where each URL has a fixed computational cost, a single GraphQL POST can trigger exponential resolver work through deeply nested self-referential types. Without depth and complexity limits, the self-referential `manager { manager { manager { ... } } }` chain on `EmployeeType` can recurse arbitrarily deep, and the `projects { tasks { assignee }` fan-out multiplies work at each level. I would add `AddMaxExecutionDepth(10)` tuned to the deepest legitimate client query, and cost analysis middleware with per-field weights reflecting real database cost. I would also require authentication on the endpoint — `[Authorize]` at the server or field level — so the attack surface is not exposed to anonymous clients. For public mobile clients I would implement persisted queries or an allow-list so only pre-approved documents can be executed, which eliminates ad-hoc deep query attacks entirely.

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

**Concepts**
- Banana Cake Pop UI off vs introspection off distinction
- ModifyRequestOptions disabling introspection for production
- Field-level [Authorize] for sensitive resolvers
- Schema exposure as production control separate from UI tooling

**Answer**

Disabling Banana Cake Pop does not disable introspection — clients can still POST `{ __schema { types { name fields { name } } } }` and discover `GetPayrollTotal` and `GetAllUsers`. Once those field names are known, they are callable directly since there is no authorization on them. I would disable introspection in production using Hot Chocolate's `ModifyRequestOptions(o => o.IntrospectionAllowed = false)`, or restrict it to authenticated users only. I would also add `[Authorize(Roles = "Admin")]` to `GetAllUsers` and `GetPayrollTotal` immediately — field-level authorization is independent of introspection and ensures that even if the schema is somehow known, the resolvers reject unauthorized callers. For long-term hygiene I would split admin-only fields to an internal endpoint or VPN-only GraphQL server so they are not reachable from the public internet at all.

---

#### Q5. (P) Explain how **DataLoader** fixes the N+1 pattern in Q1. What does batching look like at the SQL level, and where do you register loaders in Hot Chocolate (`AddDataLoader`, scoped lifetime)?

**Concepts**
- DataLoader deferred key collection before batch execution
- Single WHERE IN query replacing N individual queries
- BatchDataLoader<TKey, TValue> and GroupedDataLoader base classes
- Scoped lifetime preventing cross-request cache leaks

**Answer**

DataLoader defers and batches loads within one GraphQL request execution. When 50 `Books` fields resolve, each calls `loader.LoadAsync(authorId)` — the loader collects the IDs rather than querying immediately, since the Hot Chocolate execution engine continues dispatching resolver work. Once all field resolutions for the current batch have called `LoadAsync`, the scheduler yields and the loader executes a single `SELECT * FROM Books WHERE AuthorId IN (@p0, @p1, ..., @p49)`, then distributes the results back to the individual awaiters keyed by author ID. For many-to-one relationships I use `GroupedDataLoader`, which maps each key to a list of values. I register the loader with `builder.AddGraphQLServer().AddDataLoader<AuthorBooksDataLoader>()`, where `AuthorBooksDataLoader` extends `BatchDataLoader<int, IReadOnlyList<Book>>`. The loader must be scoped per request — its internal cache must not leak across GraphQL operations, since different requests may legitimately see different data for the same key.

---

#### Q6. (D) A product owner asks: "We already have REST — why add GraphQL?" Compare **over-fetching / under-fetching** trade-offs for a mobile app that needs user profile + last 5 orders + avatar URL. When would you keep REST, when GraphQL, when both?

**Concepts**
- Multiple REST round trips for multi-resource screens
- GraphQL single request with client-selected fields
- REST caching, simplicity, and partner integration advantage
- Hybrid architecture — REST at edge, GraphQL for aggregation

**Answer**

With REST, the mobile app needs at least three round trips — `/users/{id}`, `/users/{id}/orders?take=5`, `/users/{id}/avatar` — or a purpose-built BFF endpoint that aggregates them server-side, since each REST endpoint has a fixed response shape. GraphQL lets the client request `{ user { name avatarUrl orders(take: 5) { id total } } }` in one HTTP call, returning exactly those fields and nothing more. The trade-offs are real in both directions. I would keep REST when CDN caching matters — GraphQL POST requests are not cacheable by proxies — when the API serves external partners who cannot regenerate stubs, when the team lacks GraphQL operational experience, or when the endpoints are simple CRUD. I would add GraphQL when multiple clients have divergent field needs and the team can invest in DataLoader, complexity limits, and auth. The hybrid pattern — REST for writes and cache-friendly reads at the edge, GraphQL for composite mobile or dashboard BFFs — is common at scale and avoids forcing GraphQL everywhere it does not add value.

---

#### Q7. (M) You must enforce authorization on GraphQL — some fields are public, `GetPayrollTotal` is admin-only, and users may only read their own `orders`. Compare **ASP.NET Core policy on the request**, **Hot Chocolate `[Authorize]` on fields**, and **manual checks inside resolvers**. What fails if you only put `[Authorize]` on the controller equivalent?

**Concepts**
- Request-level [Authorize] locking entire schema or leaving it open
- Field-level [Authorize] for mixed public/private schema
- Manual resolver check for row-level ownership rules
- No per-action controller equivalent in a single-endpoint GraphQL API

**Answer**

GraphQL has no per-action controller — one endpoint executes many fields, so request-level `[Authorize]` on `MapGraphQL` is all-or-nothing: it either blocks anonymous users from the entire schema or leaves everything open, with no ability to express "public catalog and private orders on the same endpoint." Field-level `[Authorize]` in Hot Chocolate applies policy per resolver — `[Authorize(Roles = "Admin")]` on `GetPayrollTotal` while `GetCatalog` remains anonymous — which is the right default for mixed schemas. Manual checks inside resolvers handle row-level rules that policies cannot express: after loading the user's orders, I verify `order.UserId == currentUserId` and throw an authorization exception for any order that does not belong to the caller. The best practice is to layer all three: authenticate at the HTTP layer so the principal is populated, use field-level `[Authorize]` for role and policy gates, and add manual resource checks where the rule depends on the entity's own data. Subscriptions and mutations need the same field-level rules as queries — there is no separate authorization surface for them.

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

**Concepts**
- Request-scoped DbContext shared between mutation and query fields
- EF change tracker returning cached entity on FindAsync
- Mutation payload as authoritative source vs follow-up field query
- AsNoTracking or ReloadAsync for fresh read after mutation

**Answer**

Within one scoped `DbContext`, the mutation tracks the updated `Product` in the change tracker. When the subsequent `product(id: 1)` field resolver calls `FindAsync` on the same context, EF returns the cached tracked entity rather than issuing a fresh SQL query — so the client receives the same object the mutation already modified, which may diverge from the actual database state if another writer also changed the row between the mutation's `SaveChangesAsync` and the field resolution. The root cause is that the mutation and the follow-up query field share one `DbContext` scope, so the change tracker's state is the source of truth rather than the database. I would fix this in two ways: first, the client should read the authoritative stock from the mutation's own payload — `updateStock { id stock }` — rather than relying on a follow-up field in the same document. Second, if the query field must issue a fresh read, the resolver should use `AsNoTracking()` or call `db.Entry(product).ReloadAsync()` to bypass the cache. For complex cases where read and write resolvers must share a request scope, I would design them with explicit tracking boundaries rather than assuming EF will always return a fresh read.
