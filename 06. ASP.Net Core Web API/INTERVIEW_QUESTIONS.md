# ASP.NET Core Web API — Interview Questions (Extended Reference Bank)

Organized by the curriculum chapters under `07. ASP.Net Core Web API`.  
Overlapping questions are deduplicated; each topic appears once in its best-fit chapter.  
**Gotchas** at the end are real interview traps — patterns candidates commonly miss.

> **Purpose:** Long-term reference bank — questions only (no answers). Coverage-driven; chapter count varies by topic depth.  
> **Prerequisite:** Generic ASP.NET Core hosting, middleware, and DI are covered in `05. ASP.NET Core`.

---

## Chapter 01. Introduction to REST & Web API

1. What is REST?
2. What is a Web API?
3. What are the main REST architectural constraints?
4. What is HTTP idempotency, and which methods are idempotent?
5. What is the difference between PUT and POST?
6. What is the difference between PUT and PATCH?
7. What does it mean for an HTTP method to be "safe"?
8. When should an API return HTTP 201 Created vs 200 OK?
9. When should an API return HTTP 204 No Content?
10. What is the difference between HTTP 400 Bad Request and 404 Not Found?
11. What is HATEOAS?
12. What is RPC-style API design vs RESTful resource design?
13. What is the difference between REST and SOAP?
14. Why should GET requests not perform state-changing operations?
15. What is a REST resource vs a REST collection?
16. What is the purpose of the Location header on a 201 response?
17. What is API versioning and why is it needed?
18. What makes an endpoint "RESTful" vs merely "HTTP-based"?

---

## Chapter 02. API Controllers & Action Results

1. What is an API controller in ASP.NET Core?
2. What does the `[ApiController]` attribute do?
3. What is the difference between `ControllerBase` and `Controller`?
4. What is `IActionResult`?
5. What is `ActionResult<T>` and how does it differ from `IActionResult`?
6. What is `CreatedAtAction` and when do you use it?
7. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?
8. What does the `Ok()` helper return?
9. What does `NoContent()` return and when is it appropriate?
10. What is the difference between `NotFound()` and `BadRequest()`?
11. What HTTP status does `Conflict()` map to?
12. Why should API actions return DTOs instead of EF entities?
13. What is the Location header used for in API responses?
14. What is the difference between synchronous and asynchronous controller actions?
15. What problem does blocking on `.Result` cause in API controllers?
16. What is the difference between returning `Ok(entity)` and `CreatedAtAction` for POST?
17. What does `[ProducesResponseType]` do on an API action?
18. What is a "thin controller" in Web API design?

---

## Chapter 03. Routing & API Conventions

1. What is attribute routing in ASP.NET Core Web API?
2. What does the `[Route("api/[controller]")]` template mean?
3. What is the difference between attribute routing and conventional routing for Web APIs?
4. Why is attribute routing preferred for REST APIs?
5. What is the `[controller]` token in a route template?
6. What is RESTful route design for resource URLs?
7. What is route ambiguity and how can it occur in API routing?
8. What are nested resource routes?
9. What is `RouteOptions.LowercaseUrls`?
10. What is `UsePathBase` and how does it affect API URLs?
11. What do `[HttpGet]`, `[HttpPost]`, etc. specify?
12. What is the difference between route templates on controller vs action?
13. What is link generation in ASP.NET Core routing?
14. Why should API routes use nouns instead of verbs?
15. What is a route constraint (e.g., `{id:int}`)?
16. What happens when two actions match the same route?
17. What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?
18. How does `[ApiController]` affect route parameter binding?

---

## Chapter 04. Content Negotiation & Formatters

1. What is content negotiation in ASP.NET Core Web API?
2. What is the Accept HTTP header used for?
3. What is the Content-Type header used for in API requests?
4. What is the default JSON serializer in ASP.NET Core 8?
5. What is camelCase JSON naming and why is it used in Web APIs?
6. What is the difference between input formatters and output formatters?
7. What HTTP status code is returned when content negotiation fails?
8. What does `[Produces("application/json")]` do?
9. What does `[Consumes("application/xml")]` do?
10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?
11. How does model binding relate to input formatters?
12. What is an output formatter?
13. What is the difference between JSON and XML responses in Web APIs?
14. What does `[JsonPropertyName]` do?
15. What is `AddJsonOptions` used for?
16. When would you register a custom output formatter?
17. What is the default response format for ASP.NET Core Web API?
18. What is the difference between serialization and model binding?

---

## Chapter 05. Model Binding & Validation

1. What is model binding in ASP.NET Core Web API?
2. What does `[FromBody]` do?
3. What does `[FromQuery]` do?
4. What does `[FromRoute]` do?
5. What is the difference between `[FromBody]` and `[FromQuery]`?
6. How does `[ApiController]` affect automatic model validation?
7. What HTTP status code does automatic validation failure return?
8. What is `ValidationProblemDetails`?
9. What are data annotations for validation?
10. What is the difference between `[Required]` and optional properties?
11. What is `IValidatableObject`?
12. What is FluentValidation and how does it integrate with Web APIs?
13. Why can't GET requests reliably use `[FromBody]`?
14. What is complex type binding from query strings?
15. What is the difference between model binding and validation?
16. What does `[ValidateNever]` do?
17. What is PATCH semantics for partial updates?
18. How does camelCase JSON map to PascalCase C# properties?

---

## Chapter 06. Swagger & OpenAPI

1. What is OpenAPI?
2. What is Swagger in the context of ASP.NET Core?
3. What is the difference between OpenAPI and Swagger UI?
4. What does `AddEndpointsApiExplorer` do?
5. What does `AddSwaggerGen` do?
6. What is a Swagger document (`swagger.json`)?
7. What is Swashbuckle?
8. How does Swagger discover API endpoints?
9. What is a schema in OpenAPI?
10. What does `[ProducesResponseType]` contribute to OpenAPI?
11. What is the purpose of Swagger UI?
12. What is a security scheme in OpenAPI?
13. What is schema ID collision in Swagger generation?
14. What does `IncludeXmlComments` do?
15. What is the difference between documenting controllers vs minimal APIs in Swagger?
16. Why should Swagger UI be restricted in production?
17. What is `DocInclusionPredicate` in Swagger?
18. What is the relationship between DTOs and OpenAPI schemas?

---

## Chapter 07. API Versioning

1. What is API versioning?
2. What is URL path versioning?
3. What is header-based API versioning?
4. What is query string API versioning?
5. What is media type (Accept header) API versioning?
6. What is the difference between breaking and non-breaking API changes?
7. What does `DefaultApiVersion` configure?
8. What does `AssumeDefaultVersionWhenUnspecified` do?
9. What is the `[ApiVersion]` attribute?
10. What is `ReportApiVersions`?
11. Why is API versioning needed?
12. What are the trade-offs of URL path vs header versioning?
13. What is a deprecation strategy for old API versions?
14. What is the Sunset HTTP header?
15. What is an additive vs breaking change in JSON APIs?
16. How does CDN caching interact with query-string versioning?
17. What is `Asp.Versioning.Mvc`?
18. What is the difference between versioning the URL vs versioning the response schema?

---

## Chapter 08. CORS

1. What is CORS?
2. Why do browsers enforce CORS for Web APIs?
3. What is a cross-origin request?
4. What is a CORS preflight request?
5. When does a browser send an OPTIONS preflight?
6. What is the `Access-Control-Allow-Origin` header?
7. What is the difference between `AllowAnyOrigin` and `WithOrigins`?
8. Why can't `AllowAnyOrigin` be used with `AllowCredentials`?
9. What does `AllowHeaders` configure?
10. What does `WithExposedHeaders` do?
11. What is the correct middleware order for `UseCors` in a Web API?
12. What is the difference between simple and non-simple CORS requests?
13. Does CORS protect the API server from unauthorized access?
14. What is `Access-Control-Allow-Credentials`?
15. What is the difference between CORS errors and 401 Unauthorized?
16. What does `Access-Control-Allow-Methods` specify?
17. When should CORS be configured at the API vs API gateway?
18. What is a CORS policy in ASP.NET Core?

---

## Chapter 09. Problem Details & Error Responses

1. What is RFC 7807 Problem Details?
2. What is the `ProblemDetails` class in ASP.NET Core?
3. What is `ValidationProblemDetails`?
4. What is the difference between `ProblemDetails` and a custom `{ error: "..." }` object?
5. What HTTP status code does `[ApiController]` return for validation failures?
6. What is centralized exception handling for Web APIs?
7. What is `IExceptionHandler` in .NET 8?
8. What should Production error responses exclude?
9. What is the difference between 400 Bad Request and 404 Not Found for APIs?
10. When should an API return 409 Conflict?
11. What is the `type` field in ProblemDetails?
12. What is the `title` field in ProblemDetails?
13. What is the `detail` field in ProblemDetails?
14. What is the difference between Development and Production error responses?
15. What is `AddProblemDetails()` used for?
16. What is the `errors` dictionary in `ValidationProblemDetails`?
17. What is the difference between returning `NotFound()` and a custom ProblemDetails for 404?
18. How do API clients reliably parse validation errors?

---

## Chapter 10. Authentication & Authorization in APIs

1. What is JWT Bearer authentication for Web APIs?
2. What is the difference between authentication and authorization in APIs?
3. What is an API key and when is it used?
4. What is the difference between Bearer tokens and API keys?
5. What does `[Authorize]` do on an API controller?
6. What does `[AllowAnonymous]` do?
7. What is the difference between role-based and policy-based authorization in APIs?
8. What are OAuth2 scopes vs role claims?
9. What is `JwtBearerDefaults.AuthenticationScheme`?
10. What HTTP header carries JWT tokens?
11. What is the difference between 401 Unauthorized and 403 Forbidden?
12. What is `TokenValidationParameters`?
13. What is a custom `AuthenticationHandler` for API keys?
14. What is `[Authorize(AuthenticationSchemes = "...")]`?
15. What is resource-based authorization in APIs?
16. What is multi-tenant authorization for Web APIs?
17. How do mobile apps typically authenticate to REST APIs?
18. What is the difference between cookie auth and Bearer token auth for APIs?

---

## Chapter 11. File Upload & Streaming Responses

1. What is `IFormFile` in ASP.NET Core Web API?
2. What is `multipart/form-data`?
3. What does `[FromForm]` do for file uploads?
4. What is the `[RequestSizeLimit]` attribute?
5. What is the difference between buffering and streaming a file download?
6. What is the `Content-Disposition` header?
7. What is the difference between attachment and inline Content-Disposition?
8. What HTTP status does 413 Payload Too Large indicate?
9. What are `FormOptions` in ASP.NET Core?
10. What role do Kestrel limits play in request body size?
11. What is `IAsyncEnumerable` streaming for API responses?
12. What is the difference between `File()` and `PhysicalFileResult`?
13. What is a streaming response in Web APIs?
14. What is `[DisableFormValueModelBinding]`?
15. What is the difference between uploading via JSON vs multipart?
16. What is range request support for large files?
17. What is `MemoryBufferThreshold` in FormOptions?
18. How does a reverse proxy affect large file uploads?

---

## Chapter 12. Health Checks

1. What are health checks in ASP.NET Core Web API?
2. What is the difference between liveness and readiness probes?
3. What is `AddHealthChecks` used for?
4. What is `MapHealthChecks`?
5. What is `HealthCheckOptions.Predicate`?
6. What are health check tags?
7. Why should readiness checks include dependencies like SQL?
8. Why should liveness checks avoid external dependencies?
9. What is `AddDbContextCheck`?
10. What happens if liveness and readiness use the same failing check?
11. What is HealthChecksUI?
12. Should health endpoints be publicly accessible?
13. What is a `ResponseWriter` in health checks?
14. How do Kubernetes probes use health check endpoints?
15. What is `HealthStatus` (Healthy, Degraded, Unhealthy)?
16. What is the difference between `/health/live` and `/health/ready`?
17. What timeout considerations apply to health checks under load?
18. What information should external health endpoints expose?

---

## Chapter 13. Integration with EF Core

1. What is the typical DbContext lifetime in a Web API request?
2. Why should API controllers avoid returning EF entities directly?
3. What is the N+1 query problem in API endpoints?
4. What is `AsNoTracking` and when should read-only API actions use it?
5. What is the difference between `Include` and projection (`Select`) in API queries?
6. What is `SaveChangesAsync` in the context of API POST/PUT actions?
7. What is `DbUpdateConcurrencyException` in Web APIs?
8. What is the repository pattern for Web APIs?
9. What is `IQueryable` and why is returning it from repositories risky?
10. What is the difference between scoped DbContext and `IDbContextFactory`?
11. What is a transaction boundary in an API checkout flow?
12. What is pagination with Skip and Take?
13. What causes unstable pagination in concurrent APIs?
14. What is DTO projection with EF Core Select?
15. What happens when DbContext is injected into a Singleton service?
16. What is lazy loading and why is it problematic for APIs?
17. What is the difference between `FindAsync` and `FirstOrDefaultAsync` in APIs?
18. What is `AddDbContextFactory` used for in Web APIs?

---

## Chapter 14. API Testing & Integration Tests

1. What is `WebApplicationFactory` in ASP.NET Core?
2. What is the difference between unit tests and integration tests for APIs?
3. What is an in-memory test server for Web APIs?
4. What does `CreateClient()` on WebApplicationFactory return?
5. What is the test pyramid for API development?
6. What is Mock `HttpMessageHandler` used for?
7. Why should integration tests not use the production database?
8. What is Testcontainers for API testing?
9. What is the difference between EF InMemory and real SQL for API tests?
10. What is `ConfigureWebHost` in WebApplicationFactory?
11. How do you test authenticated API endpoints?
12. What is `PostAsJsonAsync` in integration tests?
13. What causes flaky parallel integration tests?
14. What is the difference between mocking a service vs mocking HttpClient?
15. What is a test fixture for API integration tests?
16. What is seed data in API integration tests?
17. What is the difference between testing controllers directly vs testing via HTTP?
18. Why must HttpClient instances be disposed properly in tests?

---

## Chapter 15. GraphQL with HotChocolate

1. What is GraphQL?
2. What is the difference between GraphQL and REST?
3. What is a GraphQL schema?
4. What is a GraphQL query vs mutation?
5. What is a resolver in GraphQL?
6. What is the N+1 problem in GraphQL?
7. What is DataLoader in Hot Chocolate?
8. What is over-fetching in REST vs GraphQL?
9. What is under-fetching in REST?
10. What is Hot Chocolate?
11. What is GraphQL introspection?
12. What is query depth limiting?
13. What is query complexity in GraphQL?
14. How does authorization work on GraphQL fields?
15. What is `AddGraphQLServer`?
16. What is `MapGraphQL`?
17. What is Banana Cake Pop?
18. When would you choose GraphQL over REST for an API?

---

## Chapter 16. gRPC Web APIs

1. What is gRPC?
2. What is the difference between gRPC and REST?
3. What are Protocol Buffers?
4. What is a `.proto` file?
5. What is gRPC-Web?
6. Why can't browsers use native gRPC directly?
7. What is a unary gRPC call?
8. What is server streaming in gRPC?
9. What is `RpcException`?
10. What are gRPC status codes?
11. What is a deadline in gRPC?
12. How does cancellation work in gRPC?
13. What is backward compatibility in Protocol Buffers?
14. What is `ServerCallContext`?
15. What is the difference between gRPC and JSON HTTP APIs?
16. What is `AddGrpc` used for?
17. What is `MapGrpcService`?
18. When should you choose gRPC over REST?

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

Creating a resource with POST should return HTTP 201 Created with a Location header — returning 200 OK hides the new resource URL from REST clients and breaks standard client libraries.

#### Gotcha 2. GET that mutates state

DELETE or state change on GET breaks HTTP safety rules, breaks caching proxies, and creates security holes when URLs are prefetched or logged.

#### Gotcha 3. `{ success: false }` with HTTP 200

Business failures must map to appropriate 4xx/5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics.

#### Gotcha 4. Returning EF entities from API actions

Entities expose navigation properties, internal fields, and circular references — serialize DTOs with explicit shapes and never leak database schema to clients.

#### Gotcha 5. PascalCase JSON with default camelCase policy

ASP.NET Core 8 defaults to camelCase JSON — PascalCase keys from some clients bind as default values, causing silent data loss on POST/PUT.

#### Gotcha 6. GET with `[FromBody]`

Many clients and proxies ignore GET bodies — filters sent as JSON in GET requests fail silently; use query strings or POST for complex filters.

#### Gotcha 7. CORS as server security

CORS is enforced by browsers only — it does not stop curl, Postman, or server-to-server calls; authentication and authorization still protect the API.

#### Gotcha 8. `AllowAnyOrigin` with credentials

Browsers reject `Access-Control-Allow-Origin: *` when credentials are sent — you must specify explicit origins with `WithOrigins` and `AllowCredentials`.

#### Gotcha 9. Swagger UI exposed in Production

Public Swagger UI discloses full API surface, schemas, and try-it-out access — gate behind auth or disable outside Development/Staging.

#### Gotcha 10. Missing `[ApiController]` on some controllers

Without `[ApiController]`, automatic 400 ValidationProblemDetails, binding source inference, and attribute routing behaviors differ — mixed controllers produce inconsistent error contracts.

#### Gotcha 11. Blocking on `.Result` in async actions

Blocking on `.Result` or `.Wait()` in API actions causes thread-pool starvation and deadlocks under load — always await async service calls.

#### Gotcha 12. Liveness probe includes SQL check

If liveness fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL/Redis checks on readiness only.

#### Gotcha 13. N+1 queries in list endpoints

Returning entities with lazy-loaded navigation properties triggers one query per row — use projection (`Select`) or explicit `Include` with DTO mapping.

#### Gotcha 14. Unstable pagination with Skip/Take

Concurrent inserts between pages cause duplicate or skipped rows — use keyset/cursor pagination for large datasets.

#### Gotcha 15. GraphQL N+1 without DataLoader

Field resolvers that query the database per parent row explode SQL under load — batch with DataLoader or join at the root query.

#### Gotcha 16. gRPC in browser without gRPC-Web

Native gRPC uses HTTP/2 binary framing browsers do not expose — browser clients need gRPC-Web middleware plus CORS configuration.
