# 06. ASP.Net Core Web API

REST principles, API controllers, versioning, OpenAPI/Swagger, CORS, auth, gRPC, GraphQL.

## Topics

| # | Topic | Questions | Q&A File |
|---|-------|-----------|----------|
| 01 | Introduction to REST & Web API | 26 | [README.md](./01.%20Introduction%20to%20REST%20%26%20Web%20API/README.md) |
| 02 | API Controllers & Action Results | 27 | [README.md](./02.%20API%20Controllers%20%26%20Action%20Results/README.md) |
| 03 | Routing & API Conventions | 26 | [README.md](./03.%20Routing%20%26%20API%20Conventions/README.md) |
| 04 | Content Negotiation & Formatters | 26 | [README.md](./04.%20Content%20Negotiation%20%26%20Formatters/README.md) |
| 05 | Model Binding & Validation | 27 | [README.md](./05.%20Model%20Binding%20%26%20Validation/README.md) |
| 06 | Swagger & OpenAPI | 26 | [README.md](./06.%20Swagger%20%26%20OpenAPI/README.md) |
| 07 | API Versioning | 26 | [README.md](./07.%20API%20Versioning/README.md) |
| 08 | CORS | 26 | [README.md](./08.%20CORS/README.md) |
| 09 | Problem Details & Error Responses | 27 | [README.md](./09.%20Problem%20Details%20%26%20Error%20Responses/README.md) |
| 10 | Authentication & Authorization in APIs | 28 | [README.md](./10.%20Authentication%20%26%20Authorization%20in%20APIs/README.md) |
| 11 | File Upload & Streaming Responses | 26 | [README.md](./11.%20File%20Upload%20%26%20Streaming%20Responses/README.md) |
| 12 | Health Checks | 25 | [README.md](./12.%20Health%20Checks/README.md) |
| 13 | Integration with EF Core | 27 | [README.md](./13.%20Integration%20with%20EF%20Core/README.md) |
| 14 | API Testing & Integration Tests | 27 | [README.md](./14.%20API%20Testing%20%26%20Integration%20Tests/README.md) |
| 15 | GraphQL with HotChocolate | 26 | [README.md](./15.%20GraphQL%20with%20HotChocolate/README.md) |
| 16 | gRPC Web APIs | 26 | [README.md](./16.%20gRPC%20Web%20APIs/README.md) |

---

> Each Q&A file has a **Table of Contents** at the top linking to every question.
> Files that include scenario-based Karat questions have a **Scenario-Based Questions** section at the bottom.

---

# 06. ASP.NET Core Web API — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [Introduction to REST & Web API](01.%20Introduction%20to%20REST%20%26%20Web%20API/INTERVIEW_QA.md) | REST constraints, HTTP verbs, status codes, and the difference between RPC and resource-oriented design |
| 02 | [API Controllers & Action Results](02.%20API%20Controllers%20%26%20Action%20Results/INTERVIEW_QA.md) | `ControllerBase`, `[ApiController]`, `IActionResult` vs typed results, and built-in helper methods |
| 03 | [Routing & API Conventions](03.%20Routing%20%26%20API%20Conventions/INTERVIEW_QA.md) | Attribute routing, route templates, `[Route]`, `[Http*]`, and conventional vs attribute routing trade-offs |
| 04 | [Content Negotiation & Formatters](04.%20Content%20Negotiation%20%26%20Formatters/INTERVIEW_QA.md) | `Accept` / `Content-Type` headers, `IOutputFormatter`, and adding custom formatters |
| 05 | [Model Binding & Validation](05.%20Model%20Binding%20%26%20Validation/INTERVIEW_QA.md) | `[FromBody]`, `[FromQuery]`, `[FromRoute]`, `ModelState`, Data Annotations, and FluentValidation |
| 06 | [Swagger & OpenAPI](06.%20Swagger%20%26%20OpenAPI/INTERVIEW_QA.md) | Swashbuckle, `SwaggerGen`, XML comments, operation filters, and generating OpenAPI documents |
| 07 | [API Versioning](07.%20API%20Versioning/INTERVIEW_QA.md) | `Asp.Versioning.Mvc`, URL / header / query-string strategies, deprecation, and Sunset headers |
| 08 | [CORS](08.%20CORS/INTERVIEW_QA.md) | Same-origin policy, preflight requests, `AddCors` / `UseCors`, and named policies |
| 09 | [Problem Details & Error Responses](09.%20Problem%20Details%20%26%20Error%20Responses/INTERVIEW_QA.md) | RFC 7807, `ProblemDetails`, `ValidationProblemDetails`, `IExceptionHandler`, and centralized error handling |
| 10 | [Authentication & Authorization in APIs](10.%20Authentication%20%26%20Authorization%20in%20APIs/INTERVIEW_QA.md) | JWT bearer auth, `[Authorize]`, policies, claims, and token validation middleware |
| 11 | [File Upload & Streaming Responses](11.%20File%20Upload%20%26%20Streaming%20Responses/INTERVIEW_QA.md) | `IFormFile`, multipart, `FileStreamResult`, `StreamContent`, and avoiding large-file buffering |
| 12 | [Health Checks](12.%20Health%20Checks/INTERVIEW_QA.md) | `AddHealthChecks`, built-in and custom checks, `MapHealthChecks`, liveness vs readiness probes |
| 13 | [Integration with EF Core](13.%20Integration%20with%20EF%20Core/INTERVIEW_QA.md) | Registering `DbContext`, async patterns, scope lifetime, migrations in APIs, and N+1 avoidance |
| 14 | [API Testing & Integration Tests](14.%20API%20Testing%20%26%20Integration%20Tests/INTERVIEW_QA.md) | `WebApplicationFactory`, `HttpClient` test harness, test doubles, and in-process server testing |
| 15 | [GraphQL with HotChocolate](15.%20GraphQL%20with%20HotChocolate/INTERVIEW_QA.md) | Schema-first vs code-first, queries, mutations, subscriptions, and DataLoader batching |
| 16 | [gRPC Web APIs](16.%20gRPC%20Web%20APIs/INTERVIEW_QA.md) | Protobuf contracts, `Grpc.AspNetCore`, streaming RPCs, gRPC-Web for browser clients, and REST transcoding |

---

## Table of Contents
- [CQ1. How does API versioning affect OpenAPI document generation, and what is the hidden breaking-change trap with SDK clients?](#cq1-how-does-api-versioning-affect-openapi-document-generation-and-what-is-the-hidden-breaking-change-trap-with-sdk-clients)
- [CQ2. What is the exact response pipeline for an unauthenticated cross-origin request, and why does a CORS misconfiguration surface as a network error rather than a 401?](#cq2-what-is-the-exact-response-pipeline-for-an-unauthenticated-cross-origin-request-and-why-does-a-cors-misconfiguration-surface-as-a-network-error-rather-than-a-401)
- [CQ3. How does `[ApiController]` short-circuit model validation before the action runs, and how do you customise the ProblemDetails shape it returns?](#cq3-how-does-apicontroller-short-circuit-model-validation-before-the-action-runs-and-how-do-you-customise-the-problemdetails-shape-it-returns)
- [CQ4. Why does returning `IQueryable<T>` directly from a controller cause a `DbContext` disposed exception, and what is the correct pattern?](#cq4-why-does-returning-iqueryablet-directly-from-a-controller-cause-a-dbcontext-disposed-exception-and-what-is-the-correct-pattern)
- [CQ5. How do you build a production health-check endpoint that validates the database, a cache, and is protected from public exposure?](#cq5-how-do-you-build-a-production-health-check-endpoint-that-validates-the-database-a-cache-and-is-protected-from-public-exposure)
- [CQ6. How do you write integration tests for an authenticated, versioned API using `WebApplicationFactory`?](#cq6-how-do-you-write-integration-tests-for-an-authenticated-versioned-api-using-webapplicationfactory)
- [CQ7. How do `IActionResult`, `ActionResult<T>`, and `TypedResults` differ in OpenAPI schema inference, and what is the `object` return gotcha?](#cq7-how-do-iactionresult-actionresultt-and-typedresults-differ-in-openapi-schema-inference-and-what-is-the-object-return-gotcha)
- [CQ8. How do URL-segment, query-string, and header versioning strategies each affect Swagger document generation, and how do you prevent route conflicts?](#cq8-how-do-url-segment-query-string-and-header-versioning-strategies-each-affect-swagger-document-generation-and-how-do-you-prevent-route-conflicts)
- [CQ9. How does content negotiation route to XML vs JSON formatters, and how do you guarantee `ProblemDetails` is always returned as `application/problem+json`?](#cq9-how-does-content-negotiation-route-to-xml-vs-json-formatters-and-how-do-you-guarantee-problemdetails-is-always-returned-as-applicationproblemjson)
- [CQ10. When must you use `[FromRoute]`, `[FromQuery]`, `[FromBody]` explicitly, what is the nested complex-type binding gotcha, and how does `[AsParameters]` simplify minimal API handlers?](#cq10-when-must-you-use-fromroute-fromquery-frombody-explicitly-what-is-the-nested-complex-type-binding-gotcha-and-how-does-asparameters-simplify-minimal-api-handlers)
- [CQ11. Why can `AllowAnyOrigin()` not be combined with `AllowCredentials()`, and what is the complete preflight flow for credentialed cross-origin requests?](#cq11-why-can-allowanyorigin-not-be-combined-with-allowcredentials-and-what-is-the-complete-preflight-flow-for-credentialed-cross-origin-requests)
- [CQ12. How does returning `IAsyncEnumerable<T>` from a controller enable progressive JSON streaming, and how does it compare to `ToListAsync()` with EF Core?](#cq12-how-does-returning-iasyncenumerablet-from-a-controller-enable-progressive-json-streaming-and-how-does-it-compare-to-tolistasync-with-ef-core)
- [CQ13. What is the difference between `IFormFile` and reading `Request.Body` directly, how does `[RequestSizeLimit]` interact with Kestrel limits, and how do you handle multipart forms with mixed file and text fields?](#cq13-what-is-the-difference-between-iformfile-and-reading-requestbody-directly-how-does-requestsizelimit-interact-with-kestrel-limits-and-how-do-you-handle-multipart-forms-with-mixed-file-and-text-fields)
- [CQ14. How does field-level vs type-level `[Authorize]` work in HotChocolate, and what is the relationship-bypass gotcha with `UseProjection()`?](#cq14-how-does-field-level-vs-type-level-authorize-work-in-hotchocolate-and-what-is-the-relationship-bypass-gotcha-with-useprojection)
- [CQ15. How does gRPC carry authentication — what is the difference between channel credentials and call credentials, and when is mutual TLS preferred over JWT?](#cq15-how-does-grpc-carry-authentication--what-is-the-difference-between-channel-credentials-and-call-credentials-and-when-is-mutual-tls-preferred-over-jwt)
- [CQ16. Why does `UseInMemoryDatabase` hide EF Core translation bugs in integration tests, and how do Respawn and `ICollectionFixture` address this?](#cq16-why-does-useinmemorydatabase-hide-ef-core-translation-bugs-in-integration-tests-and-how-do-respawn-and-icollectionfixture-address-this)
- [CQ17. How do you keep health-check endpoints unversioned, and what is the K8s startup/liveness/readiness three-probe pattern in ASP.NET Core?](#cq17-how-do-you-keep-health-check-endpoints-unversioned-and-what-is-the-k8s-startuplivenessreadiness-three-probe-pattern-in-aspnet-core)
- [CQ18. How do you document `ProblemDetails` error responses in Swagger using `ProducesResponseType` and `IOperationFilter`, and what does `ProducesDefaultResponseType` add?](#cq18-how-do-you-document-problemdetails-error-responses-in-swagger-using-producesresponsetype-and-ioperationfilter-and-what-does-producesdefaultresponsetype-add)

---

## CQ1. How does API versioning affect OpenAPI document generation, and what is the hidden breaking-change trap with SDK clients?

**Concepts**
- One Swagger document per API version — each version gets its own `/swagger/v{n}/swagger.json`
- `SwaggerGen` `DocInclusionPredicate` — filters which actions appear in which document
- `[ApiVersion("2.0", Deprecated = true)]` marks an entire version as deprecated in the OpenAPI `info` block
- Additive JSON change (new optional field) — not a breaking change by HTTP semantics, but breaks generated SDK clients
- `required` in OpenAPI schema vs `[Required]` in the C# model — these are not always in sync

**Answer**

When you add `Asp.Versioning.Mvc` and configure Swashbuckle, each API version gets its own OpenAPI document at a distinct URL (e.g., `/swagger/v1/swagger.json`, `/swagger/v2/swagger.json`). The `DocInclusionPredicate` inspects the `ApiVersionModel` metadata on each action to decide which document it belongs to, so v1 controllers never appear in the v2 document and vice versa. Marking a version deprecated with `[ApiVersion("1.0", Deprecated = true)]` sets the `deprecated: true` flag in the generated `info` object, which most OpenAPI tooling renders visually. The non-obvious trap is what happens when you add a new *optional* field to a response DTO in v1 without bumping the version. By REST and HTTP semantics this is a non-breaking additive change; however, SDK clients generated from the OpenAPI document via `openapi-generator` or NSwag re-generate the model type. If a consuming team committed a generated SDK and your new field is `required: true` in the schema — perhaps because your C# property carries `[Required]` and `SwaggerGen` honours it — the generated client now has a non-nullable property that old server instances do not populate, causing null-reference or deserialization exceptions on the client side. The correct rule is: any field that is new in the current version must be optional (nullable in C#, `required: false` in the schema), and truly new contracts belong in a new version.

---

## CQ2. What is the exact response pipeline for an unauthenticated cross-origin request, and why does a CORS misconfiguration surface as a network error rather than a 401?

**Concepts**
- CORS preflight — browser sends `OPTIONS` request before the real request
- `UseCors` middleware runs before `UseAuthentication` in the pipeline
- No CORS headers on the preflight response → browser blocks the real request entirely
- Client sees `TypeError: Failed to fetch` / `NetworkError` — not an HTTP status code
- `ProblemDetails` 401/403 is only reached if CORS succeeds; the browser never sends credentials to a blocked origin

**Answer**

The ASP.NET Core middleware pipeline processes CORS before authentication because `UseCors` must appear before `UseAuthentication` and `UseAuthorization` in `Program.cs`. For a cross-origin request, the browser first sends an `OPTIONS` preflight to check whether the server permits the origin, method, and headers. The CORS middleware intercepts `OPTIONS` requests, evaluates the registered policy, and either returns a `200 OK` with the appropriate `Access-Control-Allow-*` headers or returns a `200 OK` with none of those headers. It does not return a 4xx to the preflight regardless of the outcome. If the CORS policy is misconfigured — wrong origin string, missing `AllowCredentials()`, or the `[EnableCors]` attribute referencing a policy name that does not exist — the preflight response contains no CORS headers. The browser interprets their absence as a policy rejection, discards the response, and raises a network-level error on the JavaScript side (`TypeError: Failed to fetch` in Chrome, `NS_ERROR_DOM_BAD_URI` in Firefox). The actual HTTP 401 or 403 that authentication middleware would have produced never reaches the browser because the browser refuses to send the real request at all. This is why a CORS bug presents as an opaque network failure in DevTools with no status code, rather than a visible 401. The `ProblemDetails` error body your `IExceptionHandler` carefully crafts for unauthorized requests is equally invisible — it only matters once CORS succeeds.

---

## CQ3. How does `[ApiController]` short-circuit model validation before the action runs, and how do you customise the ProblemDetails shape it returns?

**Concepts**
- `[ApiController]` activates `ModelStateInvalidFilter` — an action filter that runs before the action method body
- `ModelState.IsValid == false` → filter returns `400 BadRequest` directly; the action method is never invoked
- `ApiBehaviorOptions.InvalidModelStateResponseFactory` — delegate to replace the default factory
- `ValidationProblemDetails` — `errors` dictionary maps property name → string array of messages
- Customising the factory does not affect the `[Required]` short-circuit on bound model construction

**Answer**

Applying `[ApiController]` to a controller class opts in to several opinionated conventions, the most significant being automatic model-state validation. Under the hood, the `[ApiController]` attribute registers `ModelStateInvalidFilter` as an action filter. This filter executes during the filter pipeline — after model binding but before the action method body runs. If `ModelState.IsValid` returns false, the filter constructs a `ValidationProblemDetails` response with status 400 and returns it immediately; the action method is never called. This is why guarding with `if (!ModelState.IsValid) return BadRequest(ModelState)` inside an `[ApiController]` action is redundant. The default response factory wraps the `ModelState` errors into the RFC 7807 `ValidationProblemDetails` shape under the `errors` key. To customise this — for example to flatten all errors into a single array, add a correlation ID, or change the `type` URI — you override `ApiBehaviorOptions.InvalidModelStateResponseFactory` in `Program.cs`:

```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ctx =>
    {
        var pd = new ValidationProblemDetails(ctx.ModelState)
        {
            Type = "https://errors.example.com/validation",
            Instance = ctx.HttpContext.Request.Path
        };
        pd.Extensions["traceId"] = Activity.Current?.Id;
        return new BadRequestObjectResult(pd);
    };
});
```

This delegate receives the full `ActionContext`, so you have access to the request, services, and the structured `ModelState` dictionary. The key constraint is that this factory only applies to the automatic short-circuit path; any `BadRequest(...)` call you make explicitly inside an action is not routed through it.

---

## CQ4. Why does returning `IQueryable<T>` directly from a controller cause a `DbContext` disposed exception, and what is the correct pattern?

**Concepts**
- `DbContext` is scoped — one instance per HTTP request, disposed at end-of-request
- `IQueryable<T>` is deferred — SQL executes on the first enumeration, not when the LINQ is written
- ASP.NET Core serialises the response *after* the controller method returns and the DI scope is in teardown
- The serialiser (System.Text.Json) enumerates the `IQueryable`, triggering SQL execution on an already-disposed `DbContext`
- `await ToListAsync()` materialises the query while the scope is still alive

**Answer**

`DbContext` is registered with scoped lifetime, meaning the DI container creates one instance per HTTP request and disposes it when the request scope ends. `IQueryable<T>` is a deferred-execution query — no SQL runs until something enumerates the sequence. When you return `IQueryable<T>` directly from a controller action, ASP.NET Core completes the action method, hands the `IQueryable` to the output formatter (System.Text.Json or Newtonsoft.Json), and begins writing the response. At this point the DI scope is being torn down and the `DbContext` instance is disposed. When the serialiser starts enumerating the query, EF Core attempts to open a database connection using that disposed `DbContext`, throwing `ObjectDisposedException` or `InvalidOperationException: Cannot access a disposed context`. The fix is straightforward: materialise the query inside the action method while the request scope and `DbContext` are both alive:

```csharp
// Wrong — IQueryable returned; executes after scope teardown
return Ok(_db.Products.Where(p => p.IsActive));

// Correct — materialised before the action returns
var products = await _db.Products.Where(p => p.IsActive).ToListAsync();
return Ok(products);
```

A related gotcha occurs with streaming: if you use `yield return` in an `async IAsyncEnumerable<T>` action, EF Core's `AsAsyncEnumerable()` is safe because ASP.NET Core's streaming support keeps the request scope alive across the full enumeration. The scoped `DbContext` is only disposed after the last item is written. That pattern is the only legitimate way to stream EF Core results without materialising the full list first.

---

## CQ5. How do you build a production health-check endpoint that validates the database, a cache, and is protected from public exposure?

**Concepts**
- `AddHealthChecks()` — registers the health-check service and individual check registrations
- `AddDbContextCheck<TContext>()` — EF Core ping using `CanConnectAsync()` under the hood
- `AddStackExchangeRedisCache()` / third-party `AspNetCore.HealthChecks.Redis` — cache connectivity check
- `MapHealthChecks("/health")` — endpoint routing; supports separate liveness and readiness paths
- `RequireAuthorization()` on the health endpoint — applies an authorization policy to protect internal details
- Tags — filter which checks run at which endpoint (e.g., `"live"` vs `"ready"`)

**Answer**

A production health-check setup in .NET 10 typically exposes two endpoints with different scopes: a shallow liveness probe that confirms the process is running, and a deeper readiness probe that validates dependencies. The EF Core package includes `AddDbContextCheck<AppDbContext>()`, which calls `CanConnectAsync()` — this sends a trivial query to the database and is sufficient to detect connection pool exhaustion or a failed primary. For Redis, the `AspNetCore.HealthChecks.Redis` community package provides `AddRedis(connectionString)`. Tags allow each check to be associated with a probe tier:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(tags: ["ready"])
    .AddRedis(redisConnectionString, tags: ["ready"]);

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false   // no checks — process is alive if it responds
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).RequireAuthorization("InternalOnly");
```

Protecting readiness endpoints matters because the `HealthReport` response body exposes dependency names, connection string fragments from exception messages, and infrastructure topology. Applying `RequireAuthorization("InternalOnly")` with a policy that checks for a specific claim (issued by a sidecar or internal load balancer) prevents external callers from enumerating internal service graph details. The liveness endpoint at `/health/live` can be left open because it returns only `Healthy` or `Unhealthy` with no details.

---

## CQ6. How do you write integration tests for an authenticated, versioned API using `WebApplicationFactory`?

**Concepts**
- `WebApplicationFactory<TProgram>` — boots the full ASP.NET Core pipeline in-process without a real server
- `builder.ConfigureTestServices` — replaces or adds services after the app's own `Program.cs` registrations
- Fake `IAuthenticationHandler` — returns a synthetic `ClaimsPrincipal` without a real JWT issuer
- `ApiVersion` request header / query string — must be set on every `HttpClient` request in versioned tests
- `CreateClient()` vs `CreateDefaultClient()` — the default client does not follow redirects; configure base address once

**Answer**

`WebApplicationFactory<TProgram>` spins up the full ASP.NET Core middleware pipeline in-process, hitting real middleware, real filters, and real route matching — which means authentication and versioning both execute exactly as in production. To bypass JWT validation without a real token server, replace the authentication handler in `ConfigureTestServices`:

```csharp
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser"),
                             new Claim("scope", "api.read") };
        var identity = new ClaimsIdentity(claims, "Test");
        var ticket  = new AuthenticationTicket(new ClaimsPrincipal(identity), "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// In the factory override:
builder.ConfigureTestServices(services =>
{
    services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
});
```

For API versioning, every `HttpClient` request must carry the version signal the API expects. With URL path versioning the base address already embeds the version segment. With header versioning, add it to each request:

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "/products");
request.Headers.Add("x-api-version", "2.0");
var response = await client.SendAsync(request);
```

A common test-authoring mistake is forgetting to add the version header and then wondering why the action returns `400 ApiVersionUnspecified` instead of the expected payload — the versioning middleware fires before the controller action, so an unversioned request never reaches your logic at all. Encapsulate version-header injection into a helper extension method on `HttpClient` to keep tests DRY.

---

## CQ7. How do `IActionResult`, `ActionResult<T>`, and `TypedResults` differ in OpenAPI schema inference, and what is the `object` return gotcha?

**Concepts**
- `IActionResult` — non-generic; Swashbuckle cannot infer the response body type; emits `{}` or no schema for 200 responses
- `ActionResult<T>` — wraps `T` or `IActionResult`; `ResponseTypeModelConvention` extracts `T` for the 200 schema; implicit conversion operators allow `return dto;` or `return NotFound();`
- `TypedResults` (minimal APIs) — static factory; return type is part of the C# delegate signature; NativeAOT-friendly OpenAPI source generator reads it without reflection
- `Results<Ok<T>, NotFound>` union type — emits both 200 and 404 schemas in the OpenAPI document in a single declaration
- `object` anti-pattern — returning `object` from any route handler loses the generic parameter and collapses the schema to `{}`

**Answer**

When you declare a controller action returning `IActionResult`, the ASP.NET Core OpenAPI pipeline sees only an opaque interface and cannot determine the JSON schema for the 200 response. Swashbuckle emits no schema or `{}`, which means generated SDK clients receive an `object` type with no useful model. Switching to `ActionResult<T>` gives Swashbuckle enough information: the implicit conversion operators on `ActionResult<T>` allow `return dto;` or `return NotFound();` from the same method, and `ResponseTypeModelConvention` extracts the generic parameter `T` to emit the correct 200 schema. In .NET 8+ minimal APIs, `TypedResults` goes further: because the entire return type of the route handler delegate is part of the C# signature, the OpenAPI source generator (enabled with `app.MapOpenApi()`) reads the schema at build time without runtime reflection, making it NativeAOT-compatible. Declaring `Results<Ok<ProductDto>, NotFound>` as the return type causes the generator to emit both a 200 schema for `ProductDto` and a 404 entry in one pass. The `object` anti-pattern surfaces when developers write `return Ok(someObject)` where `someObject` is typed as `object` at the call site: even `ActionResult<T>` falls back to a schema-less response because the generic parameter becomes `object`. The rule is simple: always bind the concrete DTO type to the generic parameter, and prefer `TypedResults` in new minimal-API routes for the most accurate schema output.

---

## CQ8. How do URL-segment, query-string, and header versioning strategies each affect Swagger document generation, and how do you prevent route conflicts?

**Concepts**
- URL-segment versioning (`/api/v{version}/resource`) — version is part of the path; each version yields distinct OpenAPI paths; requires `IApiVersionDescriptionProvider` to enumerate document names
- Query-string versioning (`?api-version=2.0`) — single route; Swashbuckle must inject `api-version` as a query parameter into every generated operation via `AddVersionedApiExplorer`
- Header versioning (`api-version: 2.0`) — invisible in URL; Swagger UI cannot set custom request headers without a JavaScript `requestInterceptor` workaround
- Route conflict — `[ApiVersionNeutral]` action on the same path as a versioned action causes `AmbiguousMatchException` at request time
- `Asp.Versioning.Mvc` + `IApiVersionDescriptionProvider` — one Swashbuckle document per `ApiVersionDescription`; `DocInclusionPredicate` filters actions per document

**Answer**

`Asp.Versioning.Mvc` supports three versioning strategies, each creating a different challenge for Swagger. URL-segment versioning embeds the version in the path (`/api/v1/orders`), so each version naturally produces a distinct set of paths in the OpenAPI document and Swashbuckle maps them cleanly; however, a route conflict arises when an `[ApiVersionNeutral]` endpoint and a versioned controller share the same resource path — the route matcher raises `AmbiguousMatchException` at request time unless you use a different path prefix or add an explicit route constraint. Query-string versioning appends `?api-version=1.0`; the `AddVersionedApiExplorer` pipeline injects `api-version` as a required query parameter into every generated OpenAPI operation so callers in Swagger UI can set the version through the standard parameter form. Header versioning passes the version in a request header, which Swagger UI's built-in input form cannot send — you must add a `securityDefinition` workaround or configure the Swagger UI `requestInterceptor` JavaScript function to inject the header on every request. In production, URL-segment versioning is the most tooling-compatible choice. When configuring Swashbuckle, iterate `IApiVersionDescriptionProvider.ApiVersionDescriptions` to register one `SwaggerDoc` per version, and use `DocInclusionPredicate` to match each action to the correct document by inspecting the `ApiVersionModel` metadata on each endpoint.

---

## CQ9. How does content negotiation route to XML vs JSON formatters, and how do you guarantee `ProblemDetails` is always returned as `application/problem+json`?

**Concepts**
- `DefaultOutputFormatterSelector` — walks the registered formatter list against the `Accept` header; first match wins
- `AddXmlDataContractSerializerFormatters()` — registers an XML output formatter; `Accept: application/xml` requests now receive XML
- `406 Not Acceptable` — returned only when `ReturnHttpNotAcceptable = true` (not the default) and no formatter matches
- `IProblemDetailsService` — always writes `application/problem+json` regardless of the client `Accept` header; RFC 7807 mandates this
- Custom `IOutputFormatter.CanWriteResult` — must guard against intercepting responses already committed to `application/problem+json`

**Answer**

Content negotiation in ASP.NET Core is handled by `DefaultOutputFormatterSelector`, which inspects the `Accept` header and walks the registered formatter list until it finds a match for the requested media type. If you call `AddXmlDataContractSerializerFormatters()`, a client sending `Accept: application/xml` receives the response body as XML. If no formatter matches the requested media type and `ReturnHttpNotAcceptable` is set to `true` in `MvcOptions` (it defaults to `false`), the pipeline returns `406 Not Acceptable`; otherwise it falls back to the first registered formatter. The subtlety arises with error responses: when `[ApiController]` automatic validation or an `IExceptionHandler` produces a `ProblemDetails` object, it bypasses the normal content-negotiation path. The built-in `IProblemDetailsService` always writes `application/problem+json` regardless of the client `Accept` header, which is correct per RFC 7807. The risk is a custom `IOutputFormatter` that broadly handles `application/json`: if its `CanWriteResult` method does not check whether the content type is already set to `application/problem+json`, it may intercept problem responses and re-serialise them under a different content-type header, producing RFC non-compliance. The safe guard is to return `false` from `CanWriteResult` when `context.ContentType` is already `application/problem+json`. Production APIs that serve both JSON and XML should test error paths with an XML-accepting client to confirm that 400, 401, and 500 responses still arrive as `application/problem+json`.

---

## CQ10. When must you use `[FromRoute]`, `[FromQuery]`, `[FromBody]` explicitly, what is the nested complex-type binding gotcha, and how does `[AsParameters]` simplify minimal API handlers?

**Concepts**
- `[ApiController]` inference — complex types → `[FromBody]`; simple/primitive types → `[FromRoute]` if a matching token exists, otherwise `[FromQuery]`
- `[FromQuery] ComplexFilter filter` — each property binds individually from query parameters by property name
- Nested object binding gotcha — `filter.PriceRange.Min` expects `?priceRange.min=10`; flat keys (`?minPrice=10`) are not bound automatically
- `[AsParameters]` (minimal APIs) — decorates a `record` or `class`; each constructor parameter is bound from its own attributed source; eliminates parameter explosion
- `[Required]` on `[FromQuery]` properties — enforced by `ModelState` under `[ApiController]`; not automatically checked in minimal APIs without a validation filter

**Answer**

When `[ApiController]` is applied, the framework infers binding sources from parameter type and position: simple types bind from route tokens or query string; complex types default to `[FromBody]`. Explicit attributes override this inference and are required whenever the default is wrong — for example, binding a string `id` from the query string when a route token named `id` also exists. The tricky case is binding a complex filter object from the query string: `[FromQuery] ProductFilter filter` causes each property of `ProductFilter` to bind individually from query parameters matched by property name (`?categoryId=5&sort=asc`). Nested objects are the gotcha: if `ProductFilter` contains a nested `PriceRange` with `Min` and `Max`, the binder expects `?priceRange.min=10&priceRange.max=50` using dot-notation. If the client sends flat keys, binding silently fails to populate the nested properties, leaving them at default values with no validation error. You must annotate the nested properties with `[FromQuery(Name = "minPrice")]`, which requires modifying the nested type — not always possible for shared DTOs. In .NET 7+ minimal APIs, `[AsParameters]` solves the parameter-explosion problem elegantly: annotate a `record` and each constructor parameter is bound from its declared source attribute, so a handler that would otherwise take six flat parameters accepts a single strongly typed argument. Note that `[Required]` on a `[FromQuery]` parameter is validated by `ModelState` under `[ApiController]`; in minimal APIs without a validation middleware or filter, `[Required]` is not automatically checked — call `Results.ValidationProblem` manually or register an `IEndpointFilter` that runs `Validator.ValidateObject`.

---

## CQ11. Why can `AllowAnyOrigin()` not be combined with `AllowCredentials()`, and what is the complete preflight flow for credentialed cross-origin requests?

**Concepts**
- Credentialed cross-origin request — browser sends `credentials: 'include'`; attaches cookies and `Authorization` headers
- CORS preflight — browser sends `OPTIONS` before the real request; server must respond with `Access-Control-Allow-Origin: <exact-origin>` (not `*`) and `Access-Control-Allow-Credentials: true`
- `AllowAnyOrigin()` + `AllowCredentials()` — throws `InvalidOperationException` at startup in ASP.NET Core; wildcard origin with credentials violates the CORS specification
- `WithOrigins(...)` — required when `AllowCredentials()` is set; origins must be exact (scheme + host + port)
- OAuth callback redirect — the browser applies CORS to the redirect target; the redirect origin must be in the allowed list

**Answer**

When a browser sends a fetch request with `credentials: 'include'`, it attaches cookies and `Authorization` headers and first performs an `OPTIONS` preflight to check whether the server permits the origin, method, and headers. The server's preflight response must include both `Access-Control-Allow-Origin: <exact-origin>` (not the wildcard `*`) and `Access-Control-Allow-Credentials: true`. ASP.NET Core enforces this constraint at application startup: calling `.AllowAnyOrigin().AllowCredentials()` throws `InvalidOperationException` with the message "The CORS protocol does not allow specifying a wildcard (any) origin and credentials at the same time." The wildcard `*` in `Access-Control-Allow-Origin` tells the browser the response is safe for any origin, but exposing credentialed content to any origin is a security boundary violation — a malicious site could silently read authenticated responses. The browser independently enforces the same rule and rejects a credentialed response that carries `Access-Control-Allow-Origin: *`, so a server that bypassed ASP.NET Core's guard would still fail at the browser. In production, replace `AllowAnyOrigin` with `WithOrigins("https://app.example.com")`, specifying every legitimate client origin including scheme and port. For multi-tenant SaaS, load origins from configuration and pass them to `WithOrigins(allowedOrigins.ToArray())`. A related OAuth mistake is omitting the redirect URI's origin: when the authorization server issues a `302` redirect to the client app, the browser follows it cross-origin and applies CORS to the final destination — if that origin is not in the policy, the redirect is blocked before the authorization code can be exchanged.

---

## CQ12. How does returning `IAsyncEnumerable<T>` from a controller enable progressive JSON streaming, and how does it compare to `ToListAsync()` with EF Core?

**Concepts**
- `IAsyncEnumerable<T>` in controllers — natively supported since ASP.NET Core 6; `ObjectResult` detects it and uses `JsonSerializer.SerializeAsync` with streaming
- Chunk flushing — System.Text.Json writes `[`, each element, and `]` progressively; the client can begin parsing before all rows are produced
- Request scope lifetime — DI scope (and `DbContext`) remains alive until `ObjectResult.ExecuteResultAsync` completes enumeration; `AsAsyncEnumerable()` is safe
- `ToListAsync()` — materialises the entire result set into a `List<T>` in the server heap before a single byte is written; safe for small result sets; fails on very large ones with `OutOfMemoryException`
- Streaming trade-offs — no resumability on connection drop; `X-Total-Count` response headers cannot be set after body writing has started

**Answer**

ASP.NET Core's `ObjectResult` serialisation pipeline recognises `IAsyncEnumerable<T>` as a first-class return type. When System.Text.Json serialises it, it writes the opening `[`, then iterates the sequence asynchronously — writing each element and flushing the response buffer — before closing with `]`. This gives the client progressive delivery: it can begin parsing items before the server has finished producing them, which matters for large exports, log-tailing endpoints, or real-time feed APIs. Because the serialiser iterates the sequence inside `ObjectResult.ExecuteResultAsync`, which runs within the request pipeline, the DI request scope remains alive throughout. An EF Core query expressed as `dbContext.Orders.AsAsyncEnumerable()` is safe because the `DbContext` is not disposed until after the last item is written — unlike returning `IQueryable<T>` directly, which causes a disposed-context exception as described in CQ4. The contrast with `ToListAsync()` is that the entire result set is materialised into a `List<T>` in the server's heap before a single byte is written to the response. This is preferable for small result sets where a single allocation is cheaper than streaming overhead, and it allows setting response headers like `X-Total-Count` before writing the body. For large datasets, however, `ToListAsync` risks `OutOfMemoryException`. The key trade-off with streaming is that responses are not resumable if the connection drops mid-stream, and you cannot set headers after body writing has started — choose streaming deliberately, only when the dataset is large enough to justify the complexity and the client is built to handle a streaming JSON array.

---

## CQ13. What is the difference between `IFormFile` and reading `Request.Body` directly, how does `[RequestSizeLimit]` interact with Kestrel limits, and how do you handle multipart forms with mixed file and text fields?

**Concepts**
- `IFormFile` — abstracts a multipart file entry; buffers small files in memory and large files to a temp file on disk (controlled by `FormOptions.MemoryBufferThreshold`)
- Reading `Request.Body` directly — bypasses multipart parsing; efficient for single-part binary uploads; requires `[DisableFormValueModelBinding]` to prevent double-read
- Kestrel `MaxRequestBodySize` — default 30 MB; enforced at the socket level before model binding; returns `400 Bad Request` when exceeded
- `[RequestSizeLimit(bytes)]` / `[DisableRequestSizeLimit]` — action-level attributes; override the Kestrel per-request limit; `FormOptions.MultipartBodyLengthLimit` must also be increased for multipart
- Mixed multipart binding — `[FromForm]` on a model with both `string` and `IFormFile` properties; binder maps each part by field name

**Answer**

`IFormFile` is the standard approach for file uploads in ASP.NET Core: the framework's multipart form-data parser reads the request body, buffers small parts in memory and large ones to a temporary disk file via `FormOptions`, and exposes each file part as an `IFormFile` with properties like `FileName`, `ContentType`, and `Length`. For a form that mixes text fields and file fields, bind a view model decorated with `[FromForm]` that contains both `string` properties and `IFormFile` properties; the binder resolves each multipart section by field name, so a model with `string Name`, `string Description`, and `IFormFile Attachment` maps correctly from a single `multipart/form-data` request. The size-limit interaction is the production gotcha: Kestrel enforces `MaxRequestBodySize` (default 30 MB) at the socket level before the model binder runs. Exceeding this limit returns a raw `400 Bad Request` with no body. Override it per-action with `[RequestSizeLimit(100_000_000)]` for 100 MB, or remove the limit entirely with `[DisableRequestSizeLimit]`; you must also increase `FormOptions.MultipartBodyLengthLimit` for multipart bodies because that is a separate ceiling enforced by the form parser. For very large single-part binary uploads — video files or disk images — bypassing `IFormFile` is more efficient: apply `[DisableFormValueModelBinding]` to prevent the form parser from buffering the body, then read `HttpContext.Request.Body` as a raw `Stream` and pipe it directly to blob storage using `CopyToAsync`, avoiding the temporary-file overhead that `IFormFile` incurs. This pattern keeps memory usage flat regardless of upload size.

---

## CQ14. How does field-level vs type-level `[Authorize]` work in HotChocolate, and what is the relationship-bypass gotcha with `UseProjection()`?

**Concepts**
- `[Authorize]` on a type — every resolver for every field on that type requires the specified policy; protects the entire subgraph rooted at that type
- `[Authorize]` on a field — only the specific resolver for that field checks the policy; siblings on the same type are unaffected
- Relationship bypass — an authorized type's field can be accessed indirectly through a non-restricted navigation on another type if only field-level auth is used
- `UseProjection()` — translates client field selections into EF Core `Select()` expressions server-side; data is fetched before HotChocolate evaluates field-level auth
- Safe pattern — place `[Authorize]` at the type level for sensitive entities; use global query filters for row-level security

**Answer**

HotChocolate integrates with ASP.NET Core's authorization policy infrastructure via `services.AddAuthorization()` and the `[Authorize]` attribute. Placing `[Authorize]` on a GraphQL type class marks every field resolver on that type as requiring the specified policy — any query that reaches the type, regardless of the path taken, triggers the authorization check. Placing `[Authorize]` on a single field descriptor restricts only that resolver; sibling fields on the same type remain open. The bypass gotcha is a common architectural mistake: suppose `Order` is publicly accessible and its `customer` field is protected with `[Authorize("AdminOnly")]`. An anonymous client cannot query `order { customer { email } }` directly. However, if `Product` has a `recentOrders` navigation that returns `[Order]`, and `Order.customer` is protected only at the field level, the `Product.recentOrders` resolver returns `Order` objects; HotChocolate then enforces field-level auth when the client requests `customer` on each order — this works correctly with pure per-field resolvers. The risk is `UseProjection()`: it translates the client's entire field selection into an EF Core `Select()` expression and executes a single SQL query that joins to the `Customer` table before HotChocolate evaluates field-level authorization. Data flows from the database into memory before the auth check occurs. The correct fix for sensitive entities is to place `[Authorize]` at the type level so HotChocolate stops traversal before executing any resolver, or to use EF Core global query filters (`modelBuilder.Entity<Customer>().HasQueryFilter(...)`) that enforce row-level security at the SQL layer regardless of how the entity is reached.

---

## CQ15. How does gRPC carry authentication — what is the difference between channel credentials and call credentials, and when is mutual TLS preferred over JWT?

**Concepts**
- gRPC metadata — key-value pairs sent with every RPC; `Authorization: Bearer <token>` is the standard JWT carrier; analogous to HTTP headers
- Channel credentials — applied once at `GrpcChannel` creation; govern transport security; `SslCredentials` with optional client certificate for mTLS
- Call credentials — applied per-RPC call via `CallCredentials.FromInterceptor`; carry the bearer token; **require** an underlying TLS channel — rejected over plaintext HTTP/2
- Mutual TLS (mTLS) — client presents an X.509 certificate during the TLS handshake; Kestrel validates it against a trust store; no per-call token required
- `UseAuthentication` + `UseGrpc` — JWT bearer middleware validates the `Authorization` metadata header the same way it validates REST headers

**Answer**

gRPC transports authentication data in metadata headers — the gRPC equivalent of HTTP headers. For JWT bearer authentication, the client sets `Authorization: Bearer <token>` as a metadata entry on each RPC call, and on the server side the ASP.NET Core `UseAuthentication` middleware reads this header and validates it through the same `JwtBearerOptions` configuration used for REST APIs; gRPC service methods then apply `[Authorize]` just like MVC controllers. Credential objects in the `Grpc.Net.Client` library are split into two layers. Channel credentials govern transport security: `SslCredentials` is attached when creating the `GrpcChannel` and establishes the TLS connection. Call credentials carry per-call identity: a `CallCredentials.FromInterceptor` delegate runs before each RPC and attaches the bearer token to the call's metadata. Critically, call credentials require an encrypted channel — `Grpc.Net.Client` rejects call credentials over plaintext HTTP/2 as a security guard because sending a bearer token in cleartext is equivalent to a password in a URL. Mutual TLS (mTLS) adds client authentication at the TLS layer itself: the Kestrel server is configured with `ClientCertificateMode.RequireCertificate` and validates the client certificate against an `X509Certificate2` trust store; the client passes its certificate as part of `SslCredentials`. mTLS is preferred for internal service-to-service gRPC where both sides are within a controlled network: there is no per-call token to rotate, no token expiry to handle, and the identity is bound to the certificate rather than a string token. In practice, most production setups combine TLS at the channel level for transport security with JWT at the call level for fine-grained identity, and reserve mTLS for high-trust infrastructure calls between known services.

---

## CQ16. Why does `UseInMemoryDatabase` hide EF Core translation bugs in integration tests, and how do Respawn and `ICollectionFixture` address this?

**Concepts**
- `UseInMemoryDatabase` — evaluates LINQ in-process without translating to SQL; raw SQL calls, computed columns, and provider-specific functions pass in-memory but fail against a real database
- Respawn — `Respawner.CreateAsync` introspects foreign-key graph and issues ordered `DELETE`/`TRUNCATE` statements; faster than `EnsureDeleted()`/`EnsureCreated()` per test
- `WebApplicationFactory.ConfigureTestServices` — replaces `DbContext` registration to point to a test database connection string
- `ICollectionFixture<T>` — shares one `WebApplicationFactory` + `Respawner` instance across all tests in a `[Collection]`; avoids cold-start ASP.NET Core host rebuild per test method
- Fake JWT — `TestAuthHandler` returning a parameterised `ClaimsPrincipal`; claims (role, scope) injected per test via `HttpRequestMessage` headers

**Answer**

`UseInMemoryDatabase` replaces the real SQL provider with an in-memory key-value store that EF Core emulates by evaluating LINQ client-side. This means raw SQL calls (`FromSqlRaw`, `ExecuteSqlRaw`), database-computed columns, provider-specific functions (`AT TIME ZONE`, `STRING_AGG`), certain complex joins, and case-sensitive collations all pass silently in the in-memory store but fail when run against the real database. Integration tests using `UseInMemoryDatabase` may be entirely green while hiding translation bugs that surface only in staging. The correct approach is to point integration tests at a real (typically Docker-hosted) database instance. `ConfigureTestServices` replaces the `DbContext` registration with one pointing to a test connection string; the test runner applies the schema via `MigrateAsync()` once per test session. Between tests, Respawn resets state: `Respawner.CreateAsync(connection)` introspects the database's foreign-key graph and issues `DELETE` or `TRUNCATE` statements in dependency order, which is an order of magnitude faster than dropping and recreating the schema per test. Fake JWT authentication is handled by a `TestAuthHandler` (as in CQ6) parameterised so different test cases inject different claims — `role: Admin` or `scope: api.write` — without multiple handler classes; pass the desired claims in a custom request header that the handler reads from `Context.Request.Headers`. Share the `WebApplicationFactory` and `Respawner` across all tests in a file using `ICollectionFixture<T>`: the ASP.NET Core host is built once per test collection, and each test calls `Respawner.ResetAsync()` in `InitializeAsync` to restore a clean database state, avoiding the cold-start cost on every test method.

---

## CQ17. How do you keep health-check endpoints unversioned, and what is the K8s startup/liveness/readiness three-probe pattern in ASP.NET Core?

**Concepts**
- `MapHealthChecks` — maps to endpoint routing directly, bypassing the MVC/controller layer; unaffected by `Asp.Versioning.Mvc`
- K8s startup probe — runs first; high `failureThreshold × periodSeconds` covers cold start (EF Core migrations, cache warm-up); once it succeeds, Kubernetes switches to liveness
- K8s liveness probe — shallow; failure → pod restart; should check only that the process responds, not external dependencies
- K8s readiness probe — deep; failure → pod removed from Service load-balancer endpoints (no restart); checks database, cache, downstream services
- `Predicate` in `HealthCheckOptions` — filters which registered checks run at a given endpoint; `_ => false` means always healthy (process-alive check)

**Answer**

Health-check routes mapped via `MapHealthChecks("/health/live")` use ASP.NET Core's endpoint routing system directly, bypassing the MVC and controller layer. Because `Asp.Versioning.Mvc` operates as an MVC convention applied to controller actions, health-check routes are inherently unversioned — no `api-version` parameter is required or checked. This is the correct production behaviour: Kubernetes probes have no concept of API versions. The three-probe Kubernetes model maps cleanly to three ASP.NET Core endpoints. The startup probe runs first and gives the container time to complete slow initialisation — EF Core migrations, JIT warm-up, cache population — without triggering a premature restart. Configure it with a high `failureThreshold` and `periodSeconds` whose product covers the worst-case startup time. Map it to a shallow endpoint (`Predicate = _ => false`) that returns `Healthy` as soon as the process accepts connections:

```csharp
app.MapHealthChecks("/health/startup", new HealthCheckOptions { Predicate = _ => false });
```

Once the startup probe succeeds, Kubernetes switches to the liveness probe. A failing liveness probe triggers a pod restart, so it must be equally shallow — checking only that the process is alive, never external dependencies. A transient database timeout that fails the liveness probe and restarts a healthy pod is the classic production incident from conflating liveness and readiness. The readiness probe validates all dependencies tagged `"ready"` (database, Redis, downstream services) and a failure removes the pod from the load-balancer rotation without restarting it, giving the dependency time to recover. Protect the readiness endpoint with `RequireAuthorization("InternalOnly")` so external callers cannot enumerate dependency names from the health report body.

---

## CQ18. How do you document `ProblemDetails` error responses in Swagger using `ProducesResponseType` and `IOperationFilter`, and what does `ProducesDefaultResponseType` add?

**Concepts**
- `[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]` — C# 11 generic attribute; emits a typed 400 schema in the OpenAPI document
- `[ProducesDefaultResponseType]` — marks the default (catch-all) response as the type provided; commonly used to declare `ProblemDetails` for 500 and other undeclared codes
- `IOperationFilter` — Swashbuckle extension point; auto-adds 401/403/500 entries to every operation matching a rule (e.g., operations with a security requirement)
- `operation.Responses.TryAdd(...)` — safe add that does not overwrite an action-level declaration
- Convention-based vs attribute-based trade-off — `IOperationFilter` is DRY but invisible at the call site; `[ProducesResponseType]` attributes are explicit but verbose

**Answer**

Explicitly documenting error responses in the OpenAPI document is important for SDK generation and client error handling. For individual actions, the generic `[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]` attribute (available since .NET 8 / C# 11) emits a typed 400 schema so generated clients know the error body is a `ValidationProblemDetails`, not an opaque `object`. Applying these attributes to every action is verbose. The idiomatic solution is an `IOperationFilter` that programmatically appends standard error responses to every operation matching a rule:

```csharp
public class SecurityResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Security?.Count > 0)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
        }
        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Internal Server Error",
            Content =
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ProblemDetails), context.SchemaRepository)
                }
            }
        });
    }
}
```

Register the filter with `options.OperationFilter<SecurityResponsesOperationFilter>()` inside `AddSwaggerGen`. `TryAdd` prevents overwriting a more specific action-level declaration. `ProducesDefaultResponseType` is the complementary attribute for actions: it marks the OpenAPI `default` response (the catch-all for any status code not listed explicitly) as returning `ProblemDetails`, communicating to SDK generators that *any* unlisted error code follows the RFC 7807 shape. The practical combination is to use `IOperationFilter` for 401, 403, and 500 across the board, `[ProducesResponseType<ValidationProblemDetails>(400)]` on actions that perform model validation, and `[ProducesDefaultResponseType<ProblemDetails>]` on actions that may throw domain-specific errors with varied status codes.

---
