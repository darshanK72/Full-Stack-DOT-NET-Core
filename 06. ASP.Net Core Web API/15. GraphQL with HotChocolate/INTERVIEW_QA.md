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

What is GraphQL?

**Answer:** GraphQL is a query language and runtime for APIs where clients request exactly the fields they need in a single POST to a `/graphql` endpoint. A typed schema defines queries (reads), mutations (writes), and subscriptions (real-time pushes) with server-side resolvers backing each field.

- Clients send a document like `{ order(id: 1) { id total lines { sku quantity } } }` — shape drives the response.
- One endpoint replaces many REST resources for aggregated mobile or SPA clients.
- Strong typing enables tooling: introspection, schema stitching, and client code generation.
- Hot Chocolate is the common GraphQL server for ASP.NET Core 8.

---

## Q2. What is the difference between GraphQL and REST?

What is the difference between GraphQL and REST?

**Answer:** REST exposes many resource-oriented URLs with fixed response shapes per endpoint; GraphQL exposes one endpoint where the client selects nested fields in one request. REST uses HTTP verbs and status codes per resource; GraphQL typically POSTs to `/graphql` and returns 200 with errors in the body for partial failures.

- REST over-fetches when endpoints return more fields than the client needs; GraphQL requests only listed fields.
- REST under-fetches when a screen needs data from multiple endpoints; GraphQL nests related data in one round trip.
- REST caching uses HTTP semantics (GET, ETags); GraphQL POST requests require application-level caching strategies.
- GraphQL shifts complexity to the server (resolvers, N+1, query cost limits); REST keeps endpoints simpler and cache-friendly.

---

## Q3. What is a GraphQL schema?

What is a GraphQL schema?

**Answer:** The schema is the contract defining all types, fields, arguments, and root operations (`Query`, `Mutation`, `Subscription`) clients may request. Hot Chocolate builds the schema from C# types, attributes, and fluent configuration at startup.

- Each type field maps to a resolver method or property that fetches data.
- Schema is introspectable — tools query `__schema` and `__type` unless introspection is disabled in production.
- Breaking changes (removing fields) require versioning or deprecation policies like REST.
- `AddGraphQLServer()` registers types and generates the executable schema.

---

## Q4. What is a GraphQL query vs mutation?

What is a GraphQL query vs mutation?

**Answer:** Queries are read operations that fetch data without side effects; mutations are write operations that create, update, or delete data. By convention, queries may run in parallel; mutations run serially in order to avoid race conditions on related writes.

- Query root: `query { products { id name } }`.
- Mutation root: `mutation { createOrder(input: { ... }) { id status } }`.
- Both are POSTed to the GraphQL endpoint with a JSON body `{ "query": "..." }`.
- Side-effect-free reads belong on `Query`, not `Mutation` — aligns with HTTP safe/idempotent mental model.

---

## Q5. What is a resolver in GraphQL?

What is a resolver in GraphQL?

**Answer:** A resolver is the function that returns the value for a single field in the schema — given the parent object, field arguments, and request context. In Hot Chocolate, resolver methods on types or `[GraphQLName]` methods on query classes execute per field selection.

- Root query resolvers load entry points; nested field resolvers load related data (author → books).
- Resolvers receive `[Parent]`, `[Argument]`, and injected services (`[Service] AppDbContext`).
- Naive per-row database access in nested resolvers causes N+1 query explosions.
- Resolvers run within a request scope — align DI lifetimes with scoped `DbContext`.

---

## Q6. What is the N+1 problem in GraphQL?

What is the N+1 problem in GraphQL?

**Answer:** GraphQL N+1 occurs when a list field resolver runs a separate database query for each parent item — loading 50 authors triggers 50 additional queries for each author's books. Without batching, GraphQL list queries perform worse than a well-designed REST join endpoint.

- Root query returns N parents; each child field resolver queries independently.
- Symptom: 1 query for parents plus N queries for children in APM traces.
- Fix with DataLoader batching or eager loading at the root when the selection set is known.
- Same conceptual problem as EF lazy loading in REST, but easier to trigger because clients control field depth.

---

## Q7. What is DataLoader in Hot Chocolate?

What is DataLoader in Hot Chocolate?

**Answer:** DataLoader batches and caches loads within a single GraphQL request — collecting keys requested by field resolvers and issuing one query (`WHERE Id IN (...)`) instead of many. Hot Chocolate provides `BatchDataLoader`, `GroupedDataLoader`, and `CacheDataLoader` base classes.

- Register loaders in DI (scoped) and call `LoadAsync(key)` from field resolvers.
- Request-scoped cache prevents duplicate loads when the same key appears in multiple branches.
- Essential for production GraphQL APIs exposing nested relational data from EF Core.
- `.AddDataLoader<T>()` integrates loaders with the Hot Chocolate execution engine.

---

## Q8. What is over-fetching in REST vs GraphQL?

What is over-fetching in REST vs GraphQL?

**Answer:** Over-fetching happens when an API response includes more data than the client needs — REST list endpoints returning full entity graphs with unused columns and navigations. GraphQL lets clients specify fields, reducing payload size when clients request minimal selections.

- REST: `GET /api/users/1` always returns the same DTO shape regardless of whether the UI needs only `name`.
- GraphQL: client requests `{ user { name } }` and omits email, address, and permissions.
- REST can mitigate with sparse field parameters or separate lightweight endpoints — adds API surface area.
- GraphQL does not eliminate over-fetching on the server if resolvers still load full entities before projecting.

---

## Q9. What is under-fetching in REST?

What is under-fetching in REST?

**Answer:** Under-fetching occurs when a client needs data from multiple REST endpoints and must chain requests — for example, orders, then customers, then products for a dashboard. Each round trip adds latency and complicates mobile apps on slow networks.

- A screen needing 5 resources may require 5+ HTTP calls with REST.
- BFF (Backend for Frontend) aggregates REST calls server-side as an alternative to GraphQL.
- GraphQL nested queries fetch related data in one request when resolvers are efficient (DataLoader-backed).
- REST `_embed` or `include` query parameters (sparse fieldsets) partially address under-fetching.

---

## Q10. What is Hot Chocolate?

What is Hot Chocolate?

**Answer:** Hot Chocolate is a high-performance GraphQL server for .NET that integrates with ASP.NET Core 8 through `AddGraphQLServer()` and `MapGraphQL()`. It provides schema-first and code-first modeling, DataLoader, authorization, filtering/sorting, and Banana Cake Pop IDE.

- Successor ecosystem leader on .NET after GraphQL.NET; actively maintained with LTS-friendly releases.
- Supports global object identification (Relay), subscriptions via WebSockets, and Apollo Federation.
- Executes queries with middleware pipeline: parsing, validation, cost analysis, and resolver execution.
- NuGet: `HotChocolate.AspNetCore` for web hosting integration.

---

## Q11. What is GraphQL introspection?

What is GraphQL introspection?

**Answer:** Introspection lets clients query the schema itself — listing types, fields, arguments, and descriptions via special meta-fields like `__schema` and `__type`. Tools (Banana Cake Pop, GraphiQL, codegen) depend on it; attackers use it to discover hidden admin fields in production.

- Example: `{ __schema { types { name fields { name } } } }` reveals the full API surface.
- Disable or restrict introspection for anonymous users in production environments.
- Hiding Banana Cake Pop UI does not disable introspection — clients can still POST introspection queries.
- Pair introspection restrictions with field-level authorization on sensitive resolvers.

---

## Q12. What is query depth limiting?

What is query depth limiting?

**Answer:** Query depth limiting caps how many nested levels a GraphQL query may traverse — blocking `{ a { b { c { d { ... } } } } }` attacks that exponentially expand resolver work. Hot Chocolate provides `AddMaxExecutionDepth(n)` to enforce limits at execution time.

- Deep recursive schemas (comments on comments, org hierarchies) are abuse vectors without limits.
- Depth limits complement complexity/cost analysis — depth alone does not catch wide fan-out at one level.
- Tune limits against legitimate client queries; mobile apps rarely need depth above 10–15.
- Exceeded depth returns a GraphQL error before resolvers exhaust CPU or database connections.

---

## Q13. What is query complexity in GraphQL?

What is query complexity in GraphQL?

**Answer:** Query complexity assigns a cost score to each field and rejects queries exceeding a budget — penalizing wide lists and expensive resolvers. Hot Chocolate supports cost analysis middleware to prevent clients from requesting `users { friends { friends { friends } } }` at scale.

- Each field contributes weight; list fields multiply cost by expected or actual child count.
- Protects against queries that are shallow but wide (1000 items × 50 fields).
- Combine with rate limiting and authentication for public GraphQL endpoints.
- Complexity rules should reflect real database cost, not arbitrary constants.

---

## Q14. How does authorization work on GraphQL fields?

How does authorization work on GraphQL fields?

**Answer:** Hot Chocolate integrates ASP.NET Core authorization — apply `[Authorize]` on query types, mutation classes, or individual field resolvers. Policies and roles evaluate per field, so public `Query` types can expose both anonymous catalog fields and admin-only fields with different auth requirements.

- Field-level auth hides sensitive data without separate GraphQL schemas per role.
- Unauthorized fields return GraphQL errors in the `errors` array; HTTP status may remain 200.
- Use `[Authorize(Roles = "Admin")]` or named policies matching REST API auth configuration.
- Introspection may still reveal field names — security through obscurity is insufficient without auth on resolvers.

---

## Q15. What is `AddGraphQLServer`?

What is `AddGraphQLServer`?

**Answer:** `AddGraphQLServer()` registers Hot Chocolate's GraphQL executor, schema builder, and supporting services in DI. Chain configuration methods to register query types, mutations, DataLoaders, filtering, and instrumentation before building the host.

- Called in `Program.cs`: `builder.Services.AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>();`
- Returns `IRequestExecutorBuilder` for fluent registration of types, directives, and middleware.
- Integrates with ASP.NET Core logging, DataLoader scopes, and optional Apollo tracing.
- Required counterpart to `MapGraphQL()` endpoint mapping.

---

## Q16. What is `MapGraphQL`?

What is `MapGraphQL`?

**Answer:** `MapGraphQL()` adds endpoint routing for the GraphQL HTTP transport — typically POST `/graphql` — and optionally WebSocket endpoints for subscriptions. It connects incoming requests to Hot Chocolate's executor pipeline.

- `app.MapGraphQL()` after `app.Build()` exposes the schema to HTTP clients.
- `MapGraphQL("/api/graphql")` customizes the path.
- Enable Banana Cake Pop in Development with `.WithOptions(new GraphQLServerOptions { Tool = { Enable = true } })`.
- Place after auth middleware when endpoints require authenticated access.

---

## Q17. What is Banana Cake Pop?

What is Banana Cake Pop?

**Answer:** Banana Cake Pop is Hot Chocolate's built-in GraphQL IDE — a browser UI for exploring the schema, writing queries, and viewing responses. It replaces GraphQL Playground in modern Hot Chocolate versions and ships embedded with the server in Development.

- Accessible at `/graphql` when tooling is enabled — similar role to Swagger UI for REST.
- Disable or restrict in Production to avoid exposing schema details and ad-hoc query execution.
- Supports exporting schema SDL and testing mutations against local or staging servers.
- Not a substitute for securing introspection and query cost limits on public endpoints.

---

## Q18. When would you choose GraphQL over REST for an API?

When would you choose GraphQL over REST for an API?

**Answer:** Choose GraphQL when diverse clients (mobile, web, third-party) need flexible, nested data shapes from one endpoint and your team can invest in DataLoader batching, query limits, and resolver DI. Prefer REST when caching, simple CRUD, file uploads, and standard HTTP semantics matter more than client-driven field selection.

- Good fit: product catalog + user + cart screens with different field needs; rapid frontend iteration without new REST endpoints per screen.
- Poor fit: public APIs needing CDN caching, binary uploads, strict rate limiting by route, or teams without GraphQL operational experience.
- Hybrid architectures expose REST at the edge and GraphQL internally behind a BFF.
- ASP.NET Core 8 supports both in one host — GraphQL does not replace REST universally.

---

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Answer:** A successful resource creation with POST should return HTTP 201 Created and tell the client where the new resource lives — returning 200 OK omits that contract and breaks REST clients that rely on status codes and the Location header.

- Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a Location header pointing at the new resource URL.
- Include the created representation or a minimal payload in the response body when clients need immediate data without a follow-up GET.
- Returning 200 for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Answer:** GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients.

- Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally.
- Cached GET responses can replay destructive operations or stale mutations across clients.
- Use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Answer:** Business failures must map to appropriate 4xx or 5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics, retries, and monitoring.

- Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.
- HTTP status codes drive client retry logic, API gateways, and APM alerting; a 200 masks failures in dashboards.
- Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Answer:** EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts — serialize DTOs with explicit shapes and never leak database schema to clients.

- Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response.
- Circular references between entities cause JSON serializer loops or require fragile reference-handling settings.
- DTOs decouple the API contract from schema migrations and let you expose only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Answer:** ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT.

- `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` aligns server expectations with legacy client payloads.
- Enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when you must accept mixed casing.
- Silent binding failures produce 201/204 success responses with partially saved data and no validation error.

---

#### Gotcha 6. GET with `[FromBody]`

**Answer:** Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8 Web API.

- Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem.
- Use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.
- OpenAPI tools and browser fetch also discourage or block GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Answer:** CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests; authentication and authorization still protect the API.

- CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers.
- A public API without auth remains fully accessible to any non-browser client regardless of CORS policy.
- Register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for real security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Answer:** Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — you must specify explicit origins with `WithOrigins` and call `AllowCredentials`.

- `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests.
- List every trusted frontend origin explicitly, including local dev URLs and production domains.
- Credentialed cross-origin calls require both matching origins and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Answer:** Public Swagger UI discloses the full API surface, schemas, and try-it-out access — gate it behind authentication or disable it outside Development and Staging in ASP.NET Core 8.

- `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware.
- Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.
- Production APIs typically serve OpenAPI only to authenticated developers or internal tooling, not the public internet.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts.

- `[ApiController]` enables automatic model-state validation responses and `[FromBody]` inference for complex types.
- Controllers missing the attribute may return 200 with invalid models or require manual `ModelState` checks.
- Apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Answer:** Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation and deadlocks under load — always `await` async service and database calls in ASP.NET Core 8.

- Sync-over-async ties up request threads while I/O completes, reducing throughput on Kestrel under concurrent load.
- Deadlocks occur when the blocked thread holds a synchronization context the continuation needs to resume.
- Mark controller actions `async Task<IActionResult>` and propagate `await` through the service layer to EF Core and HTTP clients.

---

#### Gotcha 12. Liveness probe includes SQL check

**Answer:** If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL, Redis, and external service checks on readiness only.

- Liveness answers whether the process should be killed and restarted; a down database is not healed by restarting the app.
- Readiness removes the pod from the load balancer until dependencies recover without unnecessary restarts.
- Map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Answer:** Returning entities with lazy-loaded navigation properties triggers one SQL query per row — use projection with `Select`, explicit `Include`, or DTO mapping to fetch list data in a bounded number of queries.

- Serializing a list of `Order` entities with `Customer` navigation can execute 1 + N queries under default lazy loading.
- Project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed.
- For graphs that must be included, use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Answer:** Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — use keyset or cursor pagination ordered by a stable, indexed key for large datasets in Web API list endpoints.

- `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed between requests.
- Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response.
- Offset pagination remains acceptable for small, mostly static tables; expose cursor tokens in link headers or response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Answer:** Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — batch related loads with DataLoader or resolve joins at the root query.

- A list of 100 authors each resolving `books` individually executes 101 queries instead of one batched query.
- Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips.
- Eager-load or project at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Answer:** Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration in ASP.NET Core 8.

- Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol.
- Add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC.
- Configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

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

**Answer:**

**Answer:** The `Books` field resolver runs **once per parent author** and issues a separate SQL query each time — classic **GraphQL N+1**. Root `GetAuthors` loads authors; each nested field hits the database again.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| N+1 | Field resolver queries `Books` per author | 1 + N SQL round trips |
| Data access | New query in resolver instead of batching | DB saturation on wide queries |
| Design | Navigation exposed as lazy field resolver | Scales with selection set width |

**Fix (priority order):**

1. Register **`BatchDataLoader<int, Book[]>`** (or grouped loader) — collect author ids during field resolution, one `WHERE AuthorId IN (...)` query.
2. In Hot Chocolate: `.AddDataLoader<AuthorBooksDataLoader>()` and resolve via `dataLoader.LoadAsync(authorId)`.
3. Alternative for simple cases: project on root — `authors { books { title } }` loaded with single query using `Include` or split query at root (less flexible than DataLoader).
4. Monitor field-level query count in staging with Hot Chocolate execution diagnostics.

**Production takeaway:** **N+1 with DataLoader** is the defining GraphQL production pattern — resolvers must batch, not query per parent row.

---

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

**Answer:**

**Answer:** Registering `Query` as **Singleton** while it holds (or resolves) **scoped `AppDbContext`** creates a captive dependency — one disposed or shared context across requests. GraphQL resolvers and root types must align with DI lifetimes: **scoped** for data access, **singleton** only for stateless infrastructure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Singleton `Query` + scoped `DbContext` | `ObjectDisposedException`; undefined behavior |
| Concurrency | Shared instance across requests | Cross-request state bleed |
| Resolver DI | Field resolver pulls scoped service from singleton parent path | Lifetime mismatch |

**Fix (priority order):**

1. Remove singleton registration — Hot Chocolate resolves query types **per request** by default when registered through server builder; do not `AddSingleton<Query>()`.
2. Inject `AppDbContext` only into scoped services or use `[Service]` in resolvers within request scope.
3. Enable `ValidateScopes` on host build to fail startup on captive dependencies.
4. For expensive stateless helpers, inject singleton **services** into scoped resolvers, not the reverse.

**Production takeaway:** **Resolver DI lifetime** — data loaders and resolvers using EF are **scoped**; singleton is for caches and pure functions only.

---

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

**Answer:**

**Answer:** GraphQL exposes a **programmatic query API** — without **depth and complexity limits**, attackers craft expensive nested queries (or introspection-driven fan-out) that bypass REST route-level throttling. Anonymous access amplifies abuse.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No auth on `/graphql` | Public attack surface |
| DoS | No depth/complexity limits | Single POST can exhaust CPU/DB |
| Schema | Deep self-referential types | Exponential resolver work |

**Fix (priority order):**

1. Add **`AddMaxExecutionDepth(10)`** (tune per schema) and **`AddCostAnalysis`** / complexity rules in Hot Chocolate.
2. Require authentication — `[Authorize]` on server or field level; rate limit by client id.
3. Disable or restrict introspection in production (`ModifyRequestOptions` / disable schema introspection for anonymous).
4. Persisted queries or allow-list for public mobile clients — reject ad-hoc arbitrary documents.

**Production takeaway:** **Query depth/complexity limits** are mandatory for public GraphQL — REST's fixed endpoints limit work per request; GraphQL does not unless you enforce it.

---

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

**Answer:**

**Answer:** Disabling Banana Cake Pop does **not** disable **introspection** — clients can still POST `{ __schema { types { name fields { name } } } }` and discover `GetPayrollTotal` and `GetAllUsers`. Admin fields without auth are callable once names are known.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Schema exposure | Introspection enabled in production | Full API surface leaked to attackers |
| Authorization | Sensitive fields on public `Query` | IDOR / data exfiltration |
| Tooling | Confusion between UI tool off vs introspection off | False sense of security |

**Fix (priority order):**

1. Disable introspection for production: Hot Chocolate `ModifyRequestOptions(o => o.IntrospectionAllowed = false)` or require auth for introspection.
2. Add `[Authorize(Roles = "Admin")]` on sensitive fields and types.
3. Split admin schema to internal endpoint/VPN or separate GraphQL server.
4. Keep Banana Cake Pop dev-only; audit logged queries in prod.

**Production takeaway:** **Schema exposure** is a production control — treat introspection like Swagger UI on admin APIs: off or authenticated.

---

---

#### Q5. (P) Explain how **DataLoader** fixes the N+1 pattern in Q1. What does batching look like at the SQL level, and where do you register loaders in Hot Chocolate (`AddDataLoader`, scoped lifetime)?

---

**Answer:**

**Answer:** DataLoader **defers and batches** loads within one GraphQL request execution. When 50 `Books` fields resolve, each calls `LoadAsync(authorId)` — the loader collects ids until the scheduler yields, then runs **one query**: `SELECT * FROM Books WHERE AuthorId IN (@p0, @p1, ...)`, distributes results back to awaiters.

- Register: `builder.AddGraphQLServer().AddDataLoader<AuthorBooksBatchLoader>()` — loader class inherits `BatchDataLoader<int, IReadOnlyList<Book>>`.
- Lifetime: **scoped per request** — batch cache must not leak across GraphQL operations.
- SQL: single IN clause or join from parent ids; for many-to-one, `GroupedDataLoader` maps author → list.
- Contrast with Include-at-root: DataLoader batches **only what the client selected**.

**Production takeaway:** DataLoader turns N resolver queries into **1 batched query per resource type per request** — essential for GraphQL at scale.

---

---

#### Q6. (D) A product owner asks: "We already have REST — why add GraphQL?" Compare **over-fetching / under-fetching** trade-offs for a mobile app that needs user profile + last 5 orders + avatar URL. When would you keep REST, when GraphQL, when both?

---

**Answer:**

**Answer:** REST often forces **multiple round trips** (under-fetching) or **fat DTOs** (over-fetching) — `/users/{id}`, `/users/{id}/orders?take=5`, `/users/{id}/avatar` vs one `/users/{id}?include=everything` payload with unused fields. GraphQL lets the client request `{ user { name avatarUrl orders(take:5) { id total } } }` in **one HTTP call** with **no extra fields**.

- **Keep REST:** simple public API, CDN-cacheable resources, file uploads, teams without GraphQL operational maturity, strict rate limiting per route.
- **Add GraphQL:** many clients with different field needs, mobile/slow networks, rapid UI iteration without new endpoints — invest in DataLoader, limits, auth.
- **Both:** REST for writes/webhooks/cache-friendly reads; GraphQL for composite mobile BFF — common at scale behind gateway.

**Production takeaway:** GraphQL trades **endpoint simplicity** for **query flexibility** — operational cost (N+1, complexity attacks) must be budgeted.

---

---

#### Q7. (M) You must enforce authorization on GraphQL — some fields are public, `GetPayrollTotal` is admin-only, and users may only read their own `orders`. Compare **ASP.NET Core policy on the request**, **Hot Chocolate `[Authorize]` on fields**, and **manual checks inside resolvers**. What fails if you only put `[Authorize]` on the controller equivalent?

---

**Answer:**

**Answer:** GraphQL has **no per-action controller** — one endpoint executes many fields. **Request-level `[Authorize]`** on `MapGraphQL` blocks anonymous users entirely but cannot express "public catalog + private orders" on the same schema. **Field/type `[Authorize]`** (Hot Chocolate integrates ASP.NET Core policies) applies policy per field — correct default for mixed schemas. **Manual checks** in resolvers (`if (userId != parent.UserId) throw ...`) handle row-level rules policies cannot express alone.

- Fail if only middleware/controller auth: entire schema locked or entirely open — no field granularity.
- Best practice: authenticate at HTTP layer + **`[Authorize]` on sensitive fields** + manual resource checks where policy needs entity id from parent.
- Subscriptions and mutations need the same field-level rules as queries.

**Production takeaway:** **Auth on GraphQL** is **field-level by default** — REST's `[Authorize]` on `MeController` does not map to one GraphQL endpoint without field attributes.

---

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

**Answer:**

**Answer:** Within one scoped `DbContext`, the mutation **tracks** the updated `Product`; the subsequent `product(id: 1)` resolver may return **cached tracked state**, skip a fresh read, or conflict with `AsNoTracking` expectations — clients see inconsistent stock depending on resolver implementation. Separate operations in one document still share one request scope and one context.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracker | Same context serves mutation and query fields | Stale or inconsistent reads in one response |
| Resolver | `FindAsync` returns tracked entity without refresh | May not reflect DB if another writer exists |
| Design | Returning entity directly from mutation | Clients depend on mutation payload vs query field semantics |

**Fix (priority order):**

1. Return updated values from mutation selection — client should read `updateStock { stock }` not rely on follow-up field in same doc for critical data.
2. In query resolver after mutation: `ReloadAsync` or new query with `AsNoTracking()` if fresh read required.
3. Consider separate DbContext for read vs write in same request only with clear boundaries (advanced — usually document client pattern instead).
4. Expose `stock` via DataLoader batch refresh for product fields.

**Production takeaway:** GraphQL **request-scoped DbContext** means mutation + query in one document share tracker state — design clients and resolvers explicitly.

---

---
