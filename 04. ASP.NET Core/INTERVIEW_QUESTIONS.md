# ASP.NET Core — Interview Questions (Extended Reference Bank)

Organized by the curriculum chapters under `05. ASP.NET Core`.  
Overlapping questions are deduplicated; each topic appears once in its best-fit chapter.  
**Gotchas** at the end are real interview traps — patterns candidates commonly miss.

> **Purpose:** Long-term reference bank — questions only (no answers). Coverage-driven; chapter count varies by topic depth.

---

## Chapter 01. Introduction to ASP.NET Core

1. What is ASP.NET Core?
2. How does ASP.NET Core differ from ASP.NET Framework?
3. What is Kestrel, and what role does it play in ASP.NET Core?
4. Why do production deployments often place nginx or IIS in front of Kestrel?
5. Who terminates TLS in a typical reverse-proxy deployment?
6. What is the unified hosting model introduced by `WebApplication.CreateBuilder`?
7. What is the difference between the old `Startup.cs` pattern and the modern minimal hosting model?
8. Walk through the major stages of an HTTP request in ASP.NET Core (high level).
9. When would you choose Minimal APIs over MVC controllers?
10. What does `ASPNETCORE_ENVIRONMENT` control?
11. What makes ASP.NET Core suitable for Linux containers and cloud deployment?
12. What is the ASP.NET Core request pipeline?
13. What is endpoint routing?
14. What are the main components registered in `Program.cs`?
15. What is cross-platform hosting in the context of ASP.NET Core?
16. How does ASP.NET Core handle dependency injection by default?
17. What is the difference between in-process and out-of-process IIS hosting?
18. What architectural shifts are required when porting a .NET Framework Web API to ASP.NET Core?

---

## Chapter 02. Project Structure & Program.cs

1. What is the purpose of `Program.cs` in an ASP.NET Core application?
2. What is the difference between the `builder` phase and the `app` phase in `Program.cs`?
3. What does `WebApplication.CreateBuilder(args)` return and configure?
4. What happens when you call `builder.Build()`?
5. What happens when you call `app.Run()`?
6. What is `launchSettings.json`, and does it apply in production?
7. How does ASP.NET Core load `appsettings.json` and environment-specific overrides?
8. What is the default configuration provider precedence order?
9. What is the difference between registering services and registering middleware?
10. Why must `MapControllers()` or `MapGet()` be called for endpoints to work?
11. What is `ASPNETCORE_URLS`, and how does it relate to Kestrel binding?
12. What is the purpose of `Properties/launchSettings.json` profiles?
13. How do you organize a growing `Program.cs` without losing clarity?
14. What is the difference between `Startup.cs` and putting everything in `Program.cs`?
15. When do misconfigured DI registrations typically surface — at build, startup, or first request?
16. What is the `WebApplication` type?
17. How does `builder.Environment` differ from reading config manually?
18. What files are typically part of a new ASP.NET Core Web API project structure?

---

## Chapter 03. Middleware Pipeline

1. What is middleware in ASP.NET Core?
2. How does the middleware pipeline process an HTTP request?
3. What is a `RequestDelegate`?
4. What does calling `_next(context)` do in custom middleware?
5. What happens when middleware returns without calling `_next`?
6. Why does middleware order matter?
7. What is the recommended order for routing, authentication, and authorization middleware?
8. What is the difference between `Use`, `Run`, and `Map`?
9. What does `Map("/path", ...)` do to the pipeline?
10. What is `MapWhen`, and when would you use it?
11. How do you register custom middleware in `Program.cs`?
12. What is the difference between middleware and MVC filters?
13. Where should exception-handling middleware be placed in the pipeline?
14. What is `UseForwardedHeaders`, and why must it run early?
15. What is built-in rate limiting middleware, and where does it belong in the pipeline?
16. What does "short-circuiting the pipeline" mean?
17. How does middleware differ from an endpoint filter?
18. Can middleware access DI-registered services? How?

---

## Chapter 04. Dependency Injection & Service Lifetimes

1. What is dependency injection in ASP.NET Core?
2. What is the built-in DI container in ASP.NET Core?
3. What are the three service lifetimes in ASP.NET Core DI?
4. What is the difference between Singleton, Scoped, and Transient?
5. When should you register a service as Scoped?
6. What is a "captive dependency," and why is it a problem?
7. How does ASP.NET Core create a scope per HTTP request?
8. What happens when you register the same interface twice?
9. What is `IHttpClientFactory`, and why should you use it instead of `new HttpClient()`?
10. What are keyed services in .NET 8?
11. What is `IServiceScopeFactory`, and when do you need it?
12. What is `IDbContextFactory<TContext>`, and when is it preferred over injecting `DbContext` directly?
13. What do `ValidateOnBuild` and `ValidateScopes` do?
14. How do you register an interface with its implementation?
15. What is constructor injection?
16. Can you inject a Scoped service into a Singleton? What happens?
17. What is the difference between `AddSingleton`, `AddScoped`, and `AddTransient`?
18. How does DI work in Minimal API route handlers?

---

## Chapter 05. Configuration & Options Pattern

1. What is `IConfiguration` in ASP.NET Core?
2. What configuration sources does ASP.NET Core load by default?
3. How does configuration key precedence work when the same key exists in multiple sources?
4. What is the Options pattern?
5. What is the difference between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>`?
6. When would you use `IOptionsMonitor<T>` over `IOptions<T>`?
7. How do you bind a configuration section to a strongly typed class?
8. What does `Configure<TOptions>(configuration.GetSection("..."))` do?
9. What are named options, and when are they needed?
10. How do environment variables map to configuration keys?
11. What is `ReloadOnChange` on JSON configuration files?
12. What is the purpose of User Secrets in development?
13. How should production secrets be managed?
14. What is options validation (`ValidateDataAnnotations`, `ValidateOnStart`)?
15. What is the difference between reading `configuration["Key"]` and injecting `IOptions<T>`?
16. How does `appsettings.{Environment}.json` override base settings?
17. What is `IConfigureOptions<T>`?
18. Can singleton services safely use `IOptionsSnapshot<T>`? Why or why not?

---

## Chapter 06. Logging & Diagnostics

1. What is `ILogger<T>` in ASP.NET Core?
2. How is logging configured in ASP.NET Core?
3. What are the standard log levels in .NET logging?
4. What is structured logging?
5. Why should you use message templates instead of string interpolation in log calls?
6. What is the difference between `_logger.LogError(ex.Message)` and `_logger.LogError(ex, "...")`?
7. What is the difference between `throw;` and `throw ex;` in a catch block?
8. What is `ILogger.BeginScope`, and what is it used for?
9. How does ASP.NET Core assign a `TraceIdentifier` to each request?
10. What is a correlation ID, and where is it typically set?
11. How do you configure log levels per namespace in `appsettings.json`?
12. What logging providers ship with ASP.NET Core by default?
13. What is OpenTelemetry, and how does it relate to ASP.NET Core?
14. What is distributed tracing?
15. What should you never log in a production application?
16. What is the difference between logging and diagnostics?
17. How does Application Insights integrate with ASP.NET Core logging?
18. What is `LoggerMessage` source generators, and why use them?

---

## Chapter 07. Routing & Endpoints

1. What is routing in ASP.NET Core?
2. What is endpoint routing?
3. What is the difference between attribute routing and conventional routing?
4. How does `[Route("api/[controller]")]` work?
5. What are route constraints, and why use them?
6. What is the difference between `{id}` and `{id:int}` in a route template?
7. How does ASP.NET Core decide which endpoint handles a request?
8. What happens when two routes match the same request?
9. What is `MapControllers()`?
10. What is `MapGroup()` in Minimal APIs?
11. What is link generation, and why does it matter for `CreatedAtAction`?
12. How do you return HTTP 201 Created with a `Location` header?
13. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?
14. What role do forwarded headers play in URL generation behind a reverse proxy?
15. What is route order / route precedence?
16. How do HTTP methods map to controller actions or minimal API endpoints?
17. What is the difference between endpoint routing and legacy routing middleware?
18. What is `AmbiguousMatchException`, and what causes it?

---

## Chapter 08. Model Binding & Validation

1. What is model binding in ASP.NET Core?
2. How does ASP.NET Core bind route values, query strings, and request bodies to action parameters?
3. What is the difference between `[FromBody]`, `[FromQuery]`, and `[FromRoute]`?
4. Can a GET request have a body, and should you use `[FromBody]` on GET?
5. What does the `[ApiController]` attribute change about model validation?
6. What is `ModelState`, and how is it used?
7. What are data annotation attributes for validation?
8. What HTTP status code does `[ApiController]` return for validation failures by default?
9. What is RFC 7807 ProblemDetails?
10. How do you enable ProblemDetails responses in ASP.NET Core?
11. What is the default JSON property naming policy in ASP.NET Core Web APIs?
12. How do you configure camelCase JSON serialization?
13. What is `IValidatableObject`?
14. What is FluentValidation, and how does it differ from data annotations?
15. How does complex type binding from query strings work (e.g., nested objects)?
16. What is the difference between model binding errors and validation errors?
17. What is `[ValidateNever]` used for?
18. How does ASP.NET Core handle invalid JSON in the request body?

---

## Chapter 09. Filters

1. What are filters in ASP.NET Core MVC?
2. What is the MVC filter pipeline execution order?
3. What is an authorization filter?
4. What is an action filter?
5. What is a resource filter?
6. What is a result filter?
7. What is an exception filter?
8. What is the difference between middleware and filters?
9. When would you use a filter instead of middleware?
10. How do you register a global filter?
11. How do you apply a filter to a single action or controller?
12. What is `IAsyncActionFilter`, and how does it differ from `IActionFilter`?
13. How does `[Authorize]` relate to authorization filters?
14. What is the difference between authentication middleware and authorization filters?
15. Do Minimal APIs use MVC filters?
16. What are endpoint filters in Minimal APIs?
17. How does DI work with filters?
18. Can a filter short-circuit a request? How?

---

## Chapter 10. Exception Handling

1. How does ASP.NET Core handle unhandled exceptions by default?
2. What is `UseExceptionHandler` middleware?
3. What is `DeveloperExceptionPage`, and when is it enabled?
4. What is the difference between Development and Production exception behavior?
5. What is RFC 7807 ProblemDetails?
6. How do you return ProblemDetails from an API?
7. What is `IExceptionHandler` in .NET 8+?
8. What is the difference between `throw;` and `throw ex;`?
9. Why should you log the full exception object, not just `ex.Message`?
10. Where should global exception handling middleware be placed in the pipeline?
11. What is an exception filter, and how does it differ from exception middleware?
12. What information should never be exposed to external API clients in error responses?
13. How do you map domain exceptions to HTTP status codes centrally?
14. What is `AddProblemDetails()`?
15. What happens when an exception is thrown in middleware vs in a controller action?
16. How do you customize error responses per exception type?
17. What is the difference between client errors (4xx) and server errors (5xx)?
18. How does `[ApiController]` affect exception handling for validation failures?

---

## Chapter 11. Static Files & Request Pipeline

1. What is the purpose of the `wwwroot` folder?
2. What does `UseStaticFiles()` do?
3. What is the difference between `UseDefaultFiles()` and `UseStaticFiles()`?
4. What security risk does `UseDirectoryBrowser()` pose?
5. Where should static file middleware be placed in the pipeline?
6. How do you serve a Single Page Application (SPA) with ASP.NET Core?
7. What is SPA fallback routing, and why is it needed?
8. How do you prevent SPA fallback from intercepting API routes?
9. What are MIME types, and how does ASP.NET Core determine them for static files?
10. What is `StaticFileOptions`?
11. What is `FileExtensionContentTypeProvider`?
12. What does `ServeUnknownFileTypes` do, and when is it risky?
13. What is a path traversal attack in the context of file serving?
14. How do you set cache-control headers for static assets?
15. Why should hashed JS/CSS files be cached aggressively but `index.html` should not?
16. What is the difference between serving files from `wwwroot` vs a custom folder?
17. Can static files be served without placing them in `wwwroot`?
18. What happens if sensitive files (e.g., `.env`, `appsettings.Production.json`) are placed in `wwwroot`?

---

## Chapter 12. Hosting, Kestrel & Environments

1. What is Kestrel?
2. What is the role of IIS in hosting ASP.NET Core on Windows?
3. What is a reverse proxy, and why use one with ASP.NET Core?
4. Who terminates TLS in a typical nginx + Kestrel deployment?
5. What is `ASPNETCORE_ENVIRONMENT`?
6. What is `IHostEnvironment` / `IWebHostEnvironment`?
7. How does the environment name affect application behavior?
8. What is `ASPNETCORE_URLS`?
9. How do you configure Kestrel to listen on a specific port?
10. What is the difference between Kestrel endpoint configuration and IIS bindings?
11. What are forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`)?
12. What does `UseForwardedHeaders()` do, and why must it run early?
13. What is the difference between in-process and out-of-process IIS hosting?
14. What are health checks in ASP.NET Core?
15. What is the difference between a liveness probe and a readiness probe?
16. How do container `EXPOSE` directives relate to Kestrel listening ports?
17. What breaks if Production is accidentally set to Development?
18. What belongs in `appsettings.Development.json` vs environment variables in Production?

---

## Chapter 13. Minimal APIs

1. What are Minimal APIs in ASP.NET Core?
2. How do Minimal APIs differ from controller-based APIs?
3. How do you define a GET endpoint with Minimal APIs?
4. What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?
5. How does parameter binding work in Minimal API route handlers?
6. What is the difference between `Results.Ok()` and `TypedResults.Ok()`?
7. What is `IResult`, and why use it?
8. What are endpoint filters in Minimal APIs?
9. How do you add validation to a Minimal API endpoint?
10. How do you organize Minimal APIs with `MapGroup`?
11. How do you apply authorization to Minimal API endpoints?
12. How does OpenAPI/Swagger discover Minimal API endpoints?
13. What is `ExcludeFromDescription()` used for?
14. When would you choose Minimal APIs over controllers?
15. When would Minimal APIs become a poor long-term choice?
16. How is DI used in Minimal API handlers?
17. What is `AddEndpointsApiExplorer()`?
18. How do you return HTTP 201 Created from a Minimal API?

---

## Chapter 14. Background & Hosted Services

1. What is a hosted service in ASP.NET Core?
2. What is `IHostedService`?
3. What is `BackgroundService`, and how does it differ from `IHostedService`?
4. What is the difference between `StartAsync` and `ExecuteAsync`?
5. How do you register a hosted service in DI?
6. Why can't you inject a Scoped service directly into a Singleton hosted service?
7. What is `IServiceScopeFactory`, and how is it used in background work?
8. What is `Channel<T>`, and how is it used for in-process queuing?
9. How does graceful shutdown work for hosted services?
10. What is the role of `CancellationToken` in `BackgroundService`?
11. What happens when Kubernetes sends SIGTERM to a pod?
12. What is `PeriodicTimer`, and when would you use it in a hosted service?
13. What is the difference between polling and event-driven background processing?
14. When should background work stay in-process vs move to an external queue/broker?
15. What is the outbox pattern?
16. What problems arise from unbounded parallelism in a background worker?
17. How does a hosted service relate to the ASP.NET Core application lifetime?
18. What is the difference between `IHostedService` and a `Task.Run` fire-and-forget call?

---

## Chapter 15. WebSockets & Real-Time Transport

1. What are WebSockets, and how do they differ from regular HTTP requests?
2. How do you enable WebSockets in ASP.NET Core?
3. Where must `UseWebSockets()` be placed in the middleware pipeline?
4. What happens during a WebSocket upgrade request?
5. What server resources are consumed by an open WebSocket connection?
6. What is SignalR?
7. What is the difference between SignalR and raw WebSockets?
8. When would you choose SignalR over raw WebSockets?
9. What is a SignalR backplane, and why is it needed?
10. How do you scale WebSocket/SignalR applications across multiple server instances?
11. How is authentication handled for WebSocket connections?
12. What are WebSocket message size limits in ASP.NET Core/Kestrel?
13. What is WebSocket backpressure, and why does it matter for broadcasts?
14. How do you detect and clean up stale WebSocket connections?
15. What is the difference between WebSocket and Server-Sent Events (SSE)?
16. What is long polling, and how does it compare to WebSockets?
17. What is a SignalR Hub?
18. What security risks exist when clients self-identify via the first WebSocket message?

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

Many candidates register `UseAuthentication` and `UseAuthorization` before `UseRouting`, which breaks endpoint-aware auth in modern ASP.NET Core.

#### Gotcha 2. Scoped service in a Singleton

Injecting a scoped `DbContext` or repository into a singleton service creates a captive dependency that outlives the request scope and causes stale state or thread-safety bugs.

#### Gotcha 3. `new HttpClient()` in a singleton

Creating `HttpClient` with `new` in a long-lived service causes socket exhaustion under load; `IHttpClientFactory` manages handler lifetimes correctly.

#### Gotcha 4. `IOptions<T>` vs reload

Capturing `IOptions<T>.Value` at construction time means configuration reloads from `appsettings.json` are invisible until the service is recreated — use `IOptionsMonitor<T>` when settings must react to change.

#### Gotcha 5. GET with `[FromBody]`

Model binders ignore request bodies on GET by default in many clients and proxies; using `[FromBody]` on GET is an anti-pattern that fails silently in production.

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

ASP.NET Core Web API defaults to camelCase JSON property names — PascalCase keys from some clients do not bind unless case-insensitive matching is enabled.

#### Gotcha 7. `throw ex` vs `throw`

Rethrowing with `throw ex` resets the stack trace to the catch line, hiding the original failure location in logs and diagnostics.

#### Gotcha 8. Kestrel as the only production layer

Running Kestrel exposed directly without a reverse proxy skips TLS termination, rate limiting, and static-file edge caching that production deployments expect.

#### Gotcha 9. `launchSettings.json` in production

Environment variables and URLs in `launchSettings.json` apply only when launching from Visual Studio or `dotnet run` with a profile — they are not deployed to production hosts.

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

A non-nullable `bool` cannot distinguish "property omitted" from "explicit false" — PATCH endpoints need `bool?` or separate DTOs for partial updates.

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

Without forwarded headers, `HttpContext.Request.Scheme` stays `http` and client IP is the proxy address — HTTPS redirects and audit logs break behind nginx or IIS.

#### Gotcha 12. Static files in `wwwroot` are public

Any file placed under `wwwroot` is served to unauthenticated clients — secrets and config files must never be copied there.

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

SPA fallback registered before API endpoint mapping returns `index.html` for `/api/*` 404s — map API routes first or scope fallback to non-API paths.

#### Gotcha 14. Background service without scope factory

A singleton `BackgroundService` that injects scoped services directly fails at startup or uses disposed instances — create a scope per work unit with `IServiceScopeFactory`.

#### Gotcha 15. SignalR without a backplane on multiple instances

Broadcasting from one server instance does not reach clients connected to another — multi-instance SignalR requires a Redis or Azure Service Bus backplane.
