# Swagger & OpenAPI — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is OpenAPI?](#q1-what-is-openapi)
2. [Q2. What is Swagger in the context of ASP.NET Core?](#q2-what-is-swagger-in-the-context-of-aspnet-core)
3. [Q3. What is the difference between OpenAPI and Swagger UI?](#q3-what-is-the-difference-between-openapi-and-swagger-ui)
4. [Q4. What does `AddEndpointsApiExplorer` do?](#q4-what-does-addendpointsapiexplorer-do)
5. [Q5. What does `AddSwaggerGen` do?](#q5-what-does-addswaggergen-do)
6. [Q6. What is a Swagger document (`swagger.json`)?](#q6-what-is-a-swagger-document-swaggerjson)
7. [Q7. What is Swashbuckle?](#q7-what-is-swashbuckle)
8. [Q8. How does Swagger discover API endpoints?](#q8-how-does-swagger-discover-api-endpoints)
9. [Q9. What is a schema in OpenAPI?](#q9-what-is-a-schema-in-openapi)
10. [Q10. What does `[ProducesResponseType]` contribute to OpenAPI?](#q10-what-does-producesresponsetype-contribute-to-openapi)
11. [Q11. What is the purpose of Swagger UI?](#q11-what-is-the-purpose-of-swagger-ui)
12. [Q12. What is a security scheme in OpenAPI?](#q12-what-is-a-security-scheme-in-openapi)
13. [Q13. What is schema ID collision in Swagger generation?](#q13-what-is-schema-id-collision-in-swagger-generation)
14. [Q14. What does `IncludeXmlComments` do?](#q14-what-does-includexmlcomments-do)
15. [Q15. What is the difference between documenting controllers vs minimal APIs in Swagger?](#q15-what-is-the-difference-between-documenting-controllers-vs-minimal-apis-in-swagger)
16. [Q16. Why should Swagger UI be restricted in production?](#q16-why-should-swagger-ui-be-restricted-in-production)
17. [Q17. What is `DocInclusionPredicate` in Swagger?](#q17-what-is-docinclusionpredicate-in-swagger)
18. [Q18. What is the relationship between DTOs and OpenAPI schemas?](#q18-what-is-the-relationship-between-dtos-and-openapi-schemas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is OpenAPI?

What is OpenAPI?

**Answer:** OpenAPI (formerly Swagger Specification) is a machine-readable standard for describing REST HTTP APIs — endpoints, parameters, request bodies, response schemas, authentication, and metadata. OpenAPI 3.x documents are JSON or YAML files consumed by tools for documentation, client generation, and testing.

- Defines paths, operations, components (schemas), security schemes, and tags in a structured format.
- Enables code generation of TypeScript, C#, Java, and other client SDKs from a single source of truth.
- ASP.NET Core 8 can produce OpenAPI documents via Swashbuckle or the built-in `Microsoft.AspNetCore.OpenApi` package.
- The spec describes the contract — it does not execute or validate requests at runtime.

---

## Q2. What is Swagger in the context of ASP.NET Core?

What is Swagger in the context of ASP.NET Core?

**Answer:** In ASP.NET Core, "Swagger" commonly refers to the OpenAPI document generation and Swagger UI tooling integrated via Swashbuckle.AspNetCore or the built-in OpenAPI support. It auto-discovers endpoints and produces interactive API documentation.

- `AddSwaggerGen()` configures Swashbuckle to generate an OpenAPI document from ApiExplorer metadata.
- `UseSwagger()` serves the JSON document; `UseSwaggerUI()` serves the interactive browser UI.
- .NET 8 also offers `AddOpenApi()` and `MapOpenApi()` as a lighter built-in alternative to Swashbuckle.
- Swagger UI lets developers explore and test endpoints without writing separate documentation.

---

## Q3. What is the difference between OpenAPI and Swagger UI?

What is the difference between OpenAPI and Swagger UI?

**Answer:** OpenAPI is the specification format — the `swagger.json` or `openapi.yaml` document describing the API contract. Swagger UI is a browser-based interactive tool that renders that document for exploration and testing.

- The OpenAPI document is machine-readable — consumed by code generators, gateways, and API management platforms.
- Swagger UI is human-facing — displays endpoints, schemas, and a "Try it out" feature for sending requests.
- You can serve the OpenAPI JSON without Swagger UI — for example, only exposing the document to CI pipelines.
- Swagger UI depends on a valid OpenAPI document — an empty or malformed document produces an empty UI.

---

## Q4. What does `AddEndpointsApiExplorer` do?

What does `AddEndpointsApiExplorer` do?

**Answer:** `AddEndpointsApiExplorer` registers the endpoint metadata explorer service that discovers minimal API routes and controller actions for OpenAPI generation. It implements `IApiDescriptionGroupCollectionProvider` consumed by Swashbuckle and the built-in OpenAPI generator.

- Required alongside `AddSwaggerGen()` or `AddOpenApi()` for minimal API endpoint discovery.
- Controller-based projects also benefit — it complements `AddMvcCore().AddApiExplorer()` for full ApiExplorer coverage.
- Without it, minimal API endpoints may be absent from the generated OpenAPI document.
- Call it during service registration: `builder.Services.AddEndpointsApiExplorer();`.

---

## Q5. What does `AddSwaggerGen` do?

What does `AddSwaggerGen` do?

**Answer:** `AddSwaggerGen` registers Swashbuckle services that build an OpenAPI document at runtime from ApiExplorer endpoint metadata, action parameters, return types, and serializer schema generators. Configuration callbacks customize documents, schemas, security, and XML comments.

- Registers `ISwaggerProvider` that produces `OpenApiDocument` objects per registered document name (e.g., `"v1"`).
- Schema generation reflects DTO property types, nullability, and data annotation constraints.
- Customization hooks include `CustomSchemaIds`, `IncludeXmlComments`, `DocInclusionPredicate`, and security definitions.
- The document is generated on first request to `/swagger/{docName}/swagger.json` and cached thereafter.

---

## Q6. What is a Swagger document (`swagger.json`)?

What is a Swagger document (`swagger.json`)?

**Answer:** A Swagger document is the serialized OpenAPI specification — typically `swagger.json` — listing all API paths, HTTP methods, parameters, request/response schemas, and security requirements. It is the machine-readable contract exported by Swashbuckle or `MapOpenApi()`.

- Served at `/swagger/v1/swagger.json` by default when using Swashbuckle with document name `"v1"`.
- Consumed by Swagger UI, NSwag, OpenAPI Generator, and API management platforms for SDK generation.
- Multiple documents can coexist for API versioning — separate `v1` and `v2` JSON files.
- The document reflects what ApiExplorer discovers — undocumented endpoints or missing attributes produce incomplete schemas.

---

## Q7. What is Swashbuckle?

What is Swashbuckle?

**Answer:** Swashbuckle.AspNetCore is the popular NuGet package that integrates Swagger/OpenAPI document generation and Swagger UI into ASP.NET Core applications. It bridges ApiExplorer metadata to OpenAPI 3.x documents with extensive customization options.

- Provides `AddSwaggerGen`, `UseSwagger`, and `UseSwaggerUI` extension methods.
- Generates schemas from .NET types using System.Text.Json or Newtonsoft.Json schema generators.
- Supports XML comment integration, custom schema filters, security definitions, and multi-document versioning.
- Alternative: .NET 8's built-in `Microsoft.AspNetCore.OpenApi` for document generation without the full Swashbuckle UI stack.

---

## Q8. How does Swagger discover API endpoints?

How does Swagger discover API endpoints?

**Answer:** Swagger discovers endpoints through the ASP.NET Core ApiExplorer infrastructure — `IApiDescriptionGroupCollectionProvider` collects metadata from controller actions and minimal API routes registered during application startup.

- Controller actions are discovered via `[Route]`, `[HttpGet]`, and related attributes plus parameter and return type metadata.
- Minimal API routes require `AddEndpointsApiExplorer()` and benefit from `.WithOpenApi()` for rich metadata.
- `[ApiExplorerSettings(IgnoreApi = true)]` excludes endpoints from the document.
- Discovery is reflection-based at startup — it does not execute controllers or inspect runtime behavior.

---

## Q9. What is a schema in OpenAPI?

What is a schema in OpenAPI?

**Answer:** An OpenAPI schema describes the structure of a data type — properties, types, formats, nullability, required fields, and constraints. Schemas appear in request bodies, response payloads, and parameter definitions within the `components.schemas` section.

- Generated from .NET DTO types — `string`, `int`, `DateTime`, nested objects, arrays, and enums.
- Nullable reference types and `bool?` affect `nullable: true` in OpenAPI 3 schemas when NRT is enabled.
- Schema `$ref` pointers reference shared component schemas to avoid duplication across operations.
- Schema ID collisions occur when two types share the same short name — resolved with `CustomSchemaIds`.

---

## Q10. What does `[ProducesResponseType]` contribute to OpenAPI?

What does `[ProducesResponseType]` contribute to OpenAPI?

**Answer:** `[ProducesResponseType]` adds response metadata to ApiExplorer — HTTP status code, response type, and content type — which Swashbuckle maps to OpenAPI response definitions with accurate schemas.

- Example: `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]` documents the 200 response schema.
- Multiple attributes document different status codes — 200, 400, 404, 409 — on the same action.
- Without it, Swashbuckle may infer return types incompletely — especially for `IActionResult` without generic type info.
- Improves generated client SDKs by documenting error response shapes alongside success responses.

---

## Q11. What is the purpose of Swagger UI?

What is the purpose of Swagger UI?

**Answer:** Swagger UI is an interactive browser interface that renders the OpenAPI document — listing endpoints, schemas, and providing a "Try it out" feature to send test requests. It accelerates development, QA, and partner integration without separate API documentation.

- Displays request parameters, body schemas, and response examples per operation.
- Supports authentication flows — Bearer token entry or OAuth2 authorization code with PKCE.
- Useful in Development and Staging — should be restricted or disabled in Production to avoid exposing the full API surface.
- Multiple documents appear as a dropdown when several SwaggerDoc versions are registered.

---

## Q12. What is a security scheme in OpenAPI?

What is a security scheme in OpenAPI?

**Answer:** A security scheme defines how clients authenticate to the API — Bearer JWT, API key, OAuth2 flows, or basic auth. Defined in `components.securitySchemes` and referenced globally or per-operation in the OpenAPI document.

- Bearer scheme: `AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer" })`.
- Global `AddSecurityRequirement` applies authentication to all operations unless overridden.
- Swagger UI renders an "Authorize" button based on registered security definitions.
- Security schemes describe authentication method — they do not store or validate actual credentials.

---

## Q13. What is schema ID collision in Swagger generation?

What is schema ID collision in Swagger generation?

**Answer:** Schema ID collision occurs when Swashbuckle generates the same schema identifier for two different .NET types with the same short name — for example, `ProductDto` in two namespaces. One schema overwrites the other, producing wrong properties in the OpenAPI document and broken client generation.

- Default schema IDs use the short type name without namespace.
- Fix with `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))` for unique IDs.
- Common when internal and public DTOs share names or when versioning introduces parallel types.
- Collisions are silent until client SDK regeneration fails or returns wrong models.

---

## Q14. What does `IncludeXmlComments` do?

What does `IncludeXmlComments` do?

**Answer:** `IncludeXmlComments` configures Swashbuckle to read XML documentation files generated from `///` summary comments and attach them to OpenAPI operation descriptions, parameter docs, and property descriptions.

- Requires `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the `.csproj` to emit the `.xml` file.
- Call `c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MyApi.xml"))` in `AddSwaggerGen`.
- Enriches the OpenAPI document with human-readable descriptions beyond what reflection provides.
- Multiple XML files can be included for controllers, models, and shared contract assemblies.

---

## Q15. What is the difference between documenting controllers vs minimal APIs in Swagger?

What is the difference between documenting controllers vs minimal APIs in Swagger?

**Answer:** Controller-based APIs inherit conventions from `[Route]`, `[HttpGet]`, parameter attributes, and `[ProducesResponseType]` — Swashbuckle discovers metadata automatically. Minimal APIs require explicit metadata via `.WithOpenApi()`, `[AsParameters]`, `[FromBody]`, and `.Produces<T>()` because lambda signatures lack convention-based inference.

- Controllers: `[ApiController]` + attribute routing provides rich default metadata with minimal extra configuration.
- Minimal APIs: `app.MapGet(...).WithOpenApi()` adds summaries, tags, and response types; complex parameters need `[AsParameters]` or explicit binding attributes.
- Both require `AddEndpointsApiExplorer()` for minimal API discovery.
- Minimal API lambda return types may appear as untyped or generic schemas without explicit `.Produces<T>(200)` metadata.

---

## Q16. Why should Swagger UI be restricted in production?

Why should Swagger UI be restricted in production?

**Answer:** Public Swagger UI exposes the full API surface — every endpoint, parameter, schema, and authentication scheme — to anyone who can reach the URL. This aids reconnaissance and targeted attacks against misconfigured or undocumented endpoints.

- Gate behind environment checks: `if (app.Environment.IsDevelopment()) { app.UseSwaggerUI(); }`.
- Production alternatives: internal VPN docs portal, IP-restricted access via reverse proxy, or auth-protected `/swagger` route.
- Even with `[Authorize]` on endpoints, Swagger documents their existence and parameter shapes.
- OpenAPI JSON can be served to CI/CD pipelines without exposing the interactive UI publicly.

---

## Q17. What is `DocInclusionPredicate` in Swagger?

What is `DocInclusionPredicate` in Swagger?

**Answer:** `DocInclusionPredicate` is a Swashbuckle filter function `(docName, apiDescription) => bool` that determines which API descriptions appear in which OpenAPI document. Essential for multi-version APIs where v1 and v2 operations must land in separate Swagger documents.

- Example: `(docName, apiDesc) => apiDesc.GroupName == docName` maps ApiExplorer group names to document names.
- Used with `Asp.Versioning.Mvc.ApiExplorer` where each API version produces a distinct group name (e.g., `"v1"`, `"v2"`).
- Without a predicate, all actions appear in every registered document or only in the default document.
- Pair with separate `SwaggerDoc("v1", ...)` and `SwaggerDoc("v2", ...)` registrations and matching Swagger UI endpoints.

---

## Q18. What is the relationship between DTOs and OpenAPI schemas?

What is the relationship between DTOs and OpenAPI schemas?

**Answer:** OpenAPI schemas are generated from the .NET types used in action parameters and return types — typically response and request DTOs. The DTO defines the public API contract; the OpenAPI schema is the machine-readable representation of that contract.

- Exposing EF entities generates schemas reflecting database columns — internal fields, navigation properties, and circular references leak into the document.
- Response DTOs produce stable, intentional schemas with only approved fields — map entities to DTOs before returning.
- `[ProducesResponseType(typeof(OrderResponseDto), 200)]` ensures the documented schema matches the actual response shape.
- DTO renames and removals are breaking API changes reflected in schema diffs — version DTOs per API version to keep schemas accurate.

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

#### Q1. (R) Review this `Program.cs` from a production Web API. Swagger UI is reachable at `/swagger` in all environments; security review flagged it before go-live.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

Deployment: public internet, no IP allowlist, JWT-protected endpoints documented with real example tokens in `appsettings.Development.json`.

---

**Answer:**

_Answer not found._

---

#### Q2. (R) Review Swagger schema generation. Two DTOs in different namespaces share the same class name; generated OpenAPI shows merged/wrong properties and TypeScript client fails to compile.

```csharp
namespace Acme.Api.Contracts.v1 { public class ProductDto { public int Id { get; set; } public string Sku { get; set; } = ""; } }
namespace Acme.Api.Internal { public class ProductDto { public int Id { get; set; } public decimal Cost { get; set; } public string SupplierCode { get; set; } = ""; } }

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });
});
```

Both types appear on different controller actions exposed in the same document.

---

**Answer:**

_Answer not found._

---

#### Q3. (P) A PATCH DTO uses `bool?` for tri-state fields and `DateOnly?` for optional dates. Swagger UI and NSwag-generated clients show wrong nullability — required fields where omission is valid. How do you align OpenAPI schema with actual JSON binding behavior in ASP.NET Core 8?

---

**Answer:**

_Answer not found._

---

#### Q4. (P) Review JWT authentication setup for Swagger UI on an internal admin API. Developers paste bearer tokens manually today; you want "Authorize" in Swagger UI without leaking production secrets in the repo.

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });
    // no security definition yet
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* ... */);
```

What do you add to OpenAPI and Swagger UI, and what must stay out of source control?

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this minimal API + Swashbuckle setup. Swagger document is empty — no operations listed — but controllers in the same project document correctly when migrated.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok("healthy"));

app.MapPost("/orders", (CreateOrderRequest req) =>
{
    return Results.Created($"/orders/{req.Id}", req);
});

app.Run();
```

`CreateOrderRequest` is a public record; endpoint uses lambda without `[FromBody]` attributes.

---

**Answer:**

_Answer not found._

---

#### Q6. (D) A team debates exposing EF Core entity types directly in Swagger instead of separate response DTOs to "move faster." Review the API surface and documentation implications for versioning, security, and client coupling.

```csharp
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(Order), 200)]
public async Task<ActionResult<Order>> Get(int id)
    => await _db.Orders.Include(o => o.LineItems).FirstOrDefaultAsync(o => o.Id == id);

// Order entity includes: InternalNotes, CostPrice, RowVersion, SupplierId
```

OpenAPI generates full `Order` schema including all mapped columns.

---

**Answer:**

_Answer not found._

---

#### Q7. (M) Walk through what `AddSwaggerGen` produces at startup vs runtime, how `IncludeXmlComments` affects the document, and why `c.CustomSchemaIds(type => type.FullName)` is commonly added in larger APIs — tie to schema collision and polymorphic `$ref` behavior.

---

**Answer:**

_Answer not found._

---

#### Q8. (R) Review multi-version OpenAPI setup. v1 and v2 controllers exist but Swagger UI only shows v1 operations; v2 clients get wrong schemas for shared DTO names.

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "API", Version = "v2" });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    // v2 endpoint not registered
});

// v2 controller: [ApiVersion("2.0")] on separate controller type
```

No `DocInclusionPredicate` or `ConfigureSwaggerOptions` ties actions to documents.

**Answer:**

_Answer not found._

---
