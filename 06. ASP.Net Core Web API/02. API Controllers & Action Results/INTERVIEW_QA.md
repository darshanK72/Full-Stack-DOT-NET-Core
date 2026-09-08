# API Controllers & Action Results — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is an API controller in ASP.NET Core?](#q1-what-is-an-api-controller-in-aspnet-core)
2. [Q2. What does the `[ApiController]` attribute do?](#q2-what-does-the-apicontroller-attribute-do)
3. [Q3. What is the difference between `ControllerBase` and `Controller`?](#q3-what-is-the-difference-between-controllerbase-and-controller)
4. [Q4. What is `IActionResult`?](#q4-what-is-iactionresult)
5. [Q5. What is `ActionResult<T>` and how does it differ from `IActionResult`?](#q5-what-is-actionresultt-and-how-does-it-differ-from-iactionresult)
6. [Q6. What is `CreatedAtAction` and when do you use it?](#q6-what-is-createdataction-and-when-do-you-use-it)
7. [Q7. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?](#q7-what-is-the-difference-between-createdataction-and-createdatroute)
8. [Q8. What does the `Ok()` helper return?](#q8-what-does-the-ok-helper-return)
9. [Q9. What does `NoContent()` return and when is it appropriate?](#q9-what-does-nocontent-return-and-when-is-it-appropriate)
10. [Q10. What is the difference between `NotFound()` and `BadRequest()`?](#q10-what-is-the-difference-between-notfound-and-badrequest)
11. [Q11. What HTTP status does `Conflict()` map to?](#q11-what-http-status-does-conflict-map-to)
12. [Q12. Why should API actions return DTOs instead of EF entities?](#q12-why-should-api-actions-return-dtos-instead-of-ef-entities)
13. [Q13. What is the Location header used for in API responses?](#q13-what-is-the-location-header-used-for-in-api-responses)
14. [Q14. What is the difference between synchronous and asynchronous controller actions?](#q14-what-is-the-difference-between-synchronous-and-asynchronous-controller-actions)
15. [Q15. What problem does blocking on `.Result` cause in API controllers?](#q15-what-problem-does-blocking-on-result-cause-in-api-controllers)
16. [Q16. What is the difference between returning `Ok(entity)` and `CreatedAtAction` for POST?](#q16-what-is-the-difference-between-returning-okentity-and-createdataction-for-post)
17. [Q17. What does `[ProducesResponseType]` do on an API action?](#q17-what-does-producesresponsetype-do-on-an-api-action)
18. [Q18. What is a "thin controller" in Web API design?](#q18-what-is-a-thin-controller-in-web-api-design)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is an API controller in ASP.NET Core?

**Concepts**
- `[ApiController]` + route attributes + `ControllerBase` inheritance
- `AddControllers()` and `MapControllers()` pipeline registration
- Actions returning `IActionResult`, `ActionResult<T>`, or concrete result types
- Constructor injection providing scoped services
- Routing, model binding, validation, and filter pipeline participation

**Answer**

An API controller is a class decorated with `[ApiController]` and route attributes that handles HTTP requests and returns data responses — JSON, XML, or files — instead of views. It inherits from `ControllerBase` and contains action methods mapped to HTTP verbs. I register it via `builder.Services.AddControllers()` and `app.MapControllers()` in ASP.NET Core. Constructor injection provides scoped services like repositories, and the controller participates in the model binding, validation, content negotiation, and filter pipelines that ASP.NET Core sets up automatically.

---

## Q2. What does the `[ApiController]` attribute do?

**Concepts**
- Automatic `400 ValidationProblemDetails` for invalid model state
- Binding source inference: complex type → `[FromBody]`, route parameter → `[FromRoute]`
- Attribute routing requirement — conventional routing not used
- `ProblemDetails`-compatible error responses
- Consistent behavior across all controllers via assembly-level application

**Answer**

`[ApiController]` enables opinionated API behaviors: automatic `400` responses for validation failures with `ValidationProblemDetails`, binding source inference so complex types are bound from the body without explicit `[FromBody]`, and attribute routing requirement — conventional `{controller}/{action}` routing is not used. It signals the framework to apply Web API conventions rather than MVC view conventions. All controllers in a project should consistently use `[ApiController]` because mixing it with controllers that lack it produces inconsistent error contracts that clients cannot rely on.

---

## Q3. What is the difference between `ControllerBase` and `Controller`?

**Concepts**
- `ControllerBase` providing API helpers without view methods
- `Controller` inheriting `ControllerBase` and adding `View()`, `ViewData`, `TempData`
- Web API controllers inheriting `ControllerBase`; MVC Razor views inheriting `Controller`
- Accidental view dependencies introduced by using `Controller` in API projects

**Answer**

`ControllerBase` provides core API features — action results, model binding, HTTP context — without view-related helpers. `Controller` inherits `ControllerBase` and adds `View()`, `PartialView()`, `ViewBag`, and `TempData` for HTML rendering. Web APIs should inherit `ControllerBase` since there are no views to render, and using `Controller` in an API project introduces accidental view dependencies. Both support authorization attributes, filters, and model binding identically.

---

## Q4. What is `IActionResult`?

**Concepts**
- Non-generic interface representing an HTTP response produced by an action
- Deferred execution — result pipeline runs after the action returns
- Concrete implementations: `OkObjectResult`, `NotFoundResult`, `CreatedAtActionResult`
- Helper methods returning `IActionResult` instances
- OpenAPI requiring `[ProducesResponseType]` when return type is non-generic

**Answer**

`IActionResult` is the non-generic interface representing an HTTP response — status code, headers, and optional body — produced by executing a controller action. Actions return `IActionResult` or `Task<IActionResult>` to defer response execution until the result pipeline runs. Concrete implementations include `OkObjectResult`, `NotFoundResult`, `CreatedAtActionResult`, and `FileResult`. Helper methods like `Ok()`, `NotFound()`, and `BadRequest()` return these instances. Because `IActionResult` carries no type information about the success body, OpenAPI tooling needs `[ProducesResponseType]` attributes to infer response schemas.

---

## Q5. What is `ActionResult<T>` and how does it differ from `IActionResult`?

**Concepts**
- Generic union type documenting success response type `T`
- Implicit conversion allowing both `return dto` and `return NotFound()`
- Swagger documenting `T` as the 200 response schema automatically
- `IActionResult` preferred when responses vary widely in shape

**Answer**

`ActionResult<T>` is a generic union type that documents the success response type `T` while still allowing error results like `NotFound()` or `BadRequest()`. Returning `return customerDto;` implicitly wraps as `200 OK` with a typed body, while `return NotFound();` still compiles via implicit conversion. Swagger documents `T` as the `200` response schema automatically, which improves OpenAPI-generated client code. I prefer `ActionResult<T>` when the happy path returns a known DTO type, and `IActionResult` when responses vary widely — file downloads, redirects, or heterogeneous shapes.

---

## Q6. What is `CreatedAtAction` and when do you use it?

**Concepts**
- Returns `201 Created` with `Location` header from routing system
- Route values must include all template parameters
- Preferred over manual URL strings for proxy and path-base correctness
- Used after POST (or PUT that creates) when client needs the new resource URI

**Answer**

`CreatedAtAction` returns HTTP `201 Created` with a `Location` header generated by resolving a named action and route values through the routing system. I use it after POST when the client needs the canonical URI of the new resource: `return CreatedAtAction(nameof(GetById), new { id = order.Id }, orderDto)`. The `Location` header points to the GET action that retrieves the created resource, and route values must include all template parameters or the URL generation produces a wrong path. I prefer it over manual URL strings because it respects path base, lowercase URL options, and route refactors automatically.

---

## Q7. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

**Concepts**
- `CreatedAtAction` generating `Location` by action method name and route values
- `CreatedAtRoute` generating `Location` by named route (`Name = "..."`)
- Named routes surviving action method renames when route name stays constant
- `Created(string uri, value)` as fragile alternative bypassing routing

**Answer**

`CreatedAtAction` generates the `Location` URL by action method name and route values — `CreatedAtAction(nameof(GetById), new { id = 5 }, dto)`. `CreatedAtRoute` generates it by a named route defined with `Name = "..."` on a route attribute — `CreatedAtRoute("GetOrderById", new { id = 5 }, dto)` requires `[HttpGet("{id}", Name = "GetOrderById")]`. Named routes survive action method renames if the route name stays constant, which makes them more stable for long-lived public APIs. Both participate in `LinkGenerator` and respect `UsePathBase` when configured. `Created(string uri, object value)` bypasses routing entirely and breaks when URLs change behind gateways or path base is added.

---

## Q8. What does the `Ok()` helper return?

**Concepts**
- `Ok()` producing `OkResult` — 200 with empty body
- `Ok(dto)` producing `OkObjectResult` — 200 with serialized body
- Content negotiation selecting output formatter based on `Accept` header
- `Ok()` appropriate for reads and updates returning a representation
- `CreatedAtAction` preferred over `Ok()` for POST resource creation

**Answer**

`Ok()` returns HTTP `200 OK` — either with no body (`OkResult`) or with a serialized object (`OkObjectResult`). Content negotiation selects the output formatter based on the `Accept` header and `[Produces]` attributes. I use `200` for successful reads and updates that return a representation. I do not use `Ok()` for resource creation on POST — that is what `CreatedAtAction` is for, since `200` omits the `Location` header and REST clients expect `201`.

---

## Q9. What does `NoContent()` return and when is it appropriate?

**Concepts**
- `204 No Content` with empty body
- Appropriate for DELETE and PUT/PATCH without echo-back
- Must not include a response body
- Alternative: `Ok()` when returning updated entity is necessary

**Answer**

`NoContent()` returns HTTP `204` with an empty body, signaling successful processing without a response payload. It is appropriate for DELETE — `return NoContent();` after removing a resource — and for PUT or PATCH when the client does not need the updated entity echoed back. HTTP `204` must not include a message body; only headers like `ETag` may accompany it. I return `Ok()` with the updated DTO instead when the client expects the updated representation in the response.

---

## Q10. What is the difference between `NotFound()` and `BadRequest()`?

**Concepts**
- `404 Not Found` — resource absent at the given identifier
- `400 Bad Request` — request invalid, malformed, or failing validation
- `[ApiController]` auto-returning `400` for annotation validation failures
- Wrong status code breaking client retry logic and monitoring alerts

**Answer**

`NotFound()` returns HTTP `404` when the requested resource does not exist at the given identifier — `GET /api/orders/999` where order 999 is absent. `BadRequest()` returns HTTP `400` when the request itself is invalid — bad JSON, missing required fields, or business rule violation on input. `BadRequest(ModelState)` or `ValidationProblem()` includes field-level error details. `[ApiController]` auto-returns `400` for annotation validation failures without explicit `BadRequest()` calls. Using the wrong status code breaks client retry logic and monitoring alerts that distinguish client errors from missing resources.

---

## Q11. What HTTP status does `Conflict()` map to?

**Concepts**
- `409 Conflict` — resource state conflict preventing completion
- Typical cases: duplicate unique key, version mismatch, concurrent update failure
- Distinct from `400` (bad input) and `404` (resource missing)
- Client may fetch current resource state and retry after receiving `409`

**Answer**

`Conflict()` returns HTTP `409 Conflict`, indicating the request could not be completed due to a conflict with the current state of the resource. Typical cases include creating a user with an email that already exists, or optimistic concurrency failures when an `ETag` or row version does not match. This is distinct from `400` — the input itself is valid, but the current resource state prevents the operation. Clients may fetch the current resource state and retry with updated values after receiving `409`.

---

## Q12. Why should API actions return DTOs instead of EF entities?

**Concepts**
- DTOs exposing only fields clients need — stable public contract
- EF entities exposing navigation properties, shadow fields, circular references
- Circular references causing `JsonException` at runtime
- `Select` projection in EF Core queries avoiding over-fetching and N+1
- Different read and write DTO shapes (`CreateOrderDto` vs `OrderResponseDto`)

**Answer**

DTOs expose only the fields clients need, decoupling the HTTP contract from the database schema and preventing leaks of internal data, navigation properties, and circular references. EF entities change with migrations; DTOs provide a stable, intentional API surface. Navigation properties may serialize `InternalNotes`, audit fields, or lazy-loaded navigation graphs unintentionally, and circular references between entities cause `JsonException` at runtime. Projecting to DTOs with `Select` in EF Core queries also avoids over-fetching and N+1 problems since only the required columns are queried.

---

## Q13. What is the Location header used for in API responses?

**Concepts**
- URI of the newly created resource on `201 Created` responses
- Generated by `CreatedAtAction`, `CreatedAtRoute`, and `Created` helpers
- Must reflect publicly reachable URI accounting for reverse proxy path bases
- Integration tests asserting `Location` header value and verifying follow-up GET

**Answer**

The `Location` header specifies the URI of a resource — most commonly on `201 Created` responses to point clients to the newly created item. Clients use it to fetch the resource without parsing the response body for an ID. I generate it via `CreatedAtAction`, `CreatedAtRoute`, or `Created` helpers rather than manual strings so it respects path base, lowercase URL options, and route refactors. An incorrect `Location` header causes `404` on the client's next request, so integration tests should assert the `Location` header value and verify the follow-up GET against it succeeds.

---

## Q14. What is the difference between synchronous and asynchronous controller actions?

**Concepts**
- Sync actions blocking the request thread during I/O
- Async actions returning `Task<IActionResult>` and releasing thread during I/O
- `async`/`await` all the way through the call chain
- Kestrel handling thousands of concurrent connections efficiently with async actions

**Answer**

Synchronous actions block the request thread during I/O — the thread sits idle waiting for the database, file, or HTTP call to complete. Asynchronous actions return `Task<IActionResult>` or `Task<ActionResult<T>>` and release the thread back to the pool while awaiting I/O, so Kestrel can use that thread for other requests. Under concurrent load this makes a meaningful difference in throughput. ASP.NET Core expects async actions for database, HTTP, and file operations, and the action signature must use `async`/`await` all the way through the call chain — a sync wrapper around an async service call still blocks.

---

## Q15. What problem does blocking on `.Result` cause in API controllers?

**Concepts**
- `.Result` / `.Wait()` blocking thread pool thread while async operation continues
- Thread-pool starvation under concurrent load
- Classic deadlock from synchronization context contention
- Low-concurrency tests missing the problem that production traffic exposes

**Answer**

Calling `.Result` or `.Wait()` on an incomplete `Task` inside a request blocks the thread pool thread while the async operation continues on another thread, causing thread-pool starvation under load — each blocked thread holds a slot, so Kestrel's ability to accept new requests degrades. The classic deadlock occurs when the blocked thread holds a synchronization context that the continuation needs to resume. Low-concurrency integration tests miss this because the problem only manifests under parallel requests. The fix is to make the action `async Task<ActionResult<T>>` and `await` the service call, propagating the async pattern through the entire call chain.

---

## Q16. What is the difference between returning `Ok(entity)` and `CreatedAtAction` for POST?

**Concepts**
- `Ok(entity)` returning `200` without `Location` header — wrong for create
- `CreatedAtAction` returning `201` with `Location` pointing to new resource URI
- REST create semantics requiring `201` with address of new resource
- OpenAPI declaring `201` for POST create endpoints

**Answer**

`Ok(entity)` on POST returns HTTP `200` without a `Location` header, treating creation as a generic success operation. `CreatedAtAction` returns HTTP `201` with a `Location` header pointing to the new resource URI, following REST create semantics. `200` on create hides the canonical URL from REST clients and mobile SDKs that rely on `Location` for subsequent operations. I use `Ok()` on POST only for non-create operations like search or command processing — resource creation always gets `CreatedAtAction` and `201`.

---

## Q17. What does `[ProducesResponseType]` do on an API action?

**Concepts**
- Declaring possible HTTP status codes and response body types for OpenAPI generation
- Essential when return type is non-generic `IActionResult`
- Multiple attributes documenting different outcomes on one action
- Metadata only — does not change runtime behavior

**Answer**

`[ProducesResponseType]` declares possible HTTP status codes and response body types for an action, feeding Swagger/OpenAPI generation and API explorer metadata. It is essential when the return type is bare `IActionResult` and the success type is not inferable — for example, `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]`. Multiple attributes document different outcomes: `200`, `404`, `400` on the same action. OpenAPI-generated client code uses this to produce typed response handling. It is purely metadata — it does not change runtime behavior or validation.

---

## Q18. What is a "thin controller" in Web API design?

**Concepts**
- Controller validating input, authorizing caller, calling one service method, mapping result
- Business logic, data access, and orchestration belonging in services or handlers
- Domain exceptions mapped to HTTP status codes as controller responsibility
- Thin controllers being easier to unit test via mocked service

**Answer**

A thin controller validates input, authorizes the caller, calls one application service method, and maps the result to an HTTP response. It contains no business logic, data access, or cross-cutting orchestration. Business rules, transactions, and email sending live in the service or a MediatR handler. Injecting services alone does not make a controller thin if orchestration remains in the action — the test for thinness is whether the action can be fully tested by mocking a single service and asserting the HTTP result mapping. Mapping domain exceptions to HTTP status codes — `404`, `409` — is the controller's job, not the service's.

---

## Gotchas — API Controllers & Action Results (Interview Traps)

---

#### Gotcha 1. `[ApiController]` auto-400 behavior surprises

**Concepts**
- `[ApiController]` automatically returns `ValidationProblemDetails` 400 before action runs
- `ModelState.IsValid` check in action body is redundant and never reached
- Missing attribute means invalid requests reach action with bad data
- Assembly-level `[ApiController]` applying to all controllers

**Answer**

When `[ApiController]` is present, the framework evaluates `ModelState` during model binding and short-circuits to `400 ValidationProblemDetails` before the action method body runs — any manual `if (!ModelState.IsValid) return BadRequest(ModelState)` check inside the action is dead code. The real trap is the inverse: a controller missing `[ApiController]` receives invalid models silently, allowing requests with wrong data types or missing required fields to reach service layer code and produce unexpected behavior or corrupt data.

---

#### Gotcha 2. `IActionResult` vs `ActionResult<T>` for OpenAPI schema generation

**Concepts**
- `IActionResult` — no generic type; Swashbuckle cannot infer 200 response schema
- `ActionResult<T>` — implicit conversion from `T` or `IActionResult`; schema inferred as `T`
- `[ProducesResponseType(typeof(T), 200)]` needed when using `IActionResult`
- `TypedResults` in minimal APIs for compile-time return type safety

**Answer**

Declaring a controller action as `IActionResult` leaves Swashbuckle with no information about the 200 response body schema — the generated OpenAPI document shows `object` or omits the schema entirely, and code-generated clients produce untyped objects. `ActionResult<T>` allows the compiler to use implicit conversion so `return dto` works alongside `return NotFound()`, and Swashbuckle reads `T` to document the success schema automatically. I use `IActionResult` only when the success-path return shape genuinely varies across branches, and annotate every branch with `[ProducesResponseType]`.

---

#### Gotcha 3. Returning 200 with null body vs 204 No Content

**Concepts**
- `Ok(null)` — produces 200 with empty body, confusing REST clients expecting a resource
- `NoContent()` — produces 204 with no body, the correct idiom for void mutations
- `OkResult` (no body) vs `OkObjectResult` (with body) vs `NoContentResult`
- Client expectations — 200 signals a body; 204 signals intentionally empty

**Answer**

`return Ok(null)` produces `200 OK` with an empty body, which tells clients to expect a JSON body that is simply missing — some clients throw a deserialization error on the empty body. `return NoContent()` produces `204 No Content`, the correct HTTP idiom for operations that succeed without returning a resource, such as DELETE and update operations that echo nothing back. I use `NoContent()` for DELETE and PUT-with-no-echo actions, `Ok(dto)` when the updated or created resource is returned, and avoid `Ok(null)` entirely.

---

#### Gotcha 4. `CreatedAtAction` referencing a renamed or misnamed action

**Concepts**
- `CreatedAtAction(nameof(GetById), ...)` resolves at runtime via routing system
- Wrong action name produces incorrect `Location` header or a broken `null` URL
- `nameof` prevents compile errors but not silent mismatches after rename
- Integration test asserting `Location` header follows up with GET

**Answer**

`CreatedAtAction("GetUser", new { id }, dto)` fails silently when the target action was renamed to `GetById` — link generation produces an incorrect or empty `Location` header without throwing. Using `nameof` catches the rename at compile time, but only if the symbol exists; if you type `nameof(GetUser)` when the action is `GetById`, the string compiles fine and only fails at runtime. I always write an integration test that POSTs a resource, reads the `Location` header, and issues a GET to that URL asserting 200 — this is the only reliable way to catch `CreatedAtAction` breakage.

---

#### Gotcha 5. Returning EF entities directly from actions

**Concepts**
- Navigation properties causing lazy-load N+1 during JSON serialization
- Circular references causing `JsonException` at runtime
- Internal DB columns exposed as public API contract
- DTOs as stable public shape decoupled from schema migrations

**Answer**

Returning an EF Core entity directly from a controller serializes every public property including navigation properties, shadow fields, and internal audit columns that were never meant to be public. Lazy-loaded navigations trigger one SQL query per navigation access during serialization — a list of 100 orders with a `Customer` navigation produces 101 queries. Circular entity references (`Order → Customer → Orders`) cause `System.Text.Json` to throw `JsonException`. I always project to a DTO inside the LINQ query or map with AutoMapper before returning from an action, which fixes all three problems at once.

---

#### Gotcha 6. `ControllerBase` vs `Controller` in Web API projects

**Concepts**
- `Controller` inherits `ControllerBase` and adds Razor view helpers (`View()`, `PartialView()`)
- View helpers pull in MVC view-rendering dependencies irrelevant to JSON APIs
- `ControllerBase` — correct base for Web API controllers with no view rendering
- `[ApiController]` requires `ControllerBase`; works with `Controller` but adds unnecessary weight

**Answer**

Inheriting from `Controller` in a Web API project adds the Razor view engine helper methods (`View()`, `PartialView()`, `Json()`) to every action, which adds unnecessary assembly dependencies and makes the view-rendering surface available by accident. `ControllerBase` is the correct base class for JSON APIs — it provides `Ok()`, `NotFound()`, `BadRequest()`, `CreatedAtAction()`, and the full `IActionResult` helper set without view-rendering overhead. Web API project templates use `ControllerBase` by default; switching to `Controller` is usually a mistake from copying MVC tutorial code.

---

#### Gotcha 7. `OkResult` vs `OkObjectResult` — empty vs valued 200

**Concepts**
- `Ok()` — `OkResult`, HTTP 200 with no body
- `Ok(value)` — `OkObjectResult`, HTTP 200 with serialized body
- `return Ok()` instead of `return Ok(dto)` — produces empty body on GET
- Unit test casting `result` to `OkObjectResult` throws when action returned `OkResult`

**Answer**

`Ok()` and `Ok(dto)` both return 200 but `Ok()` produces no response body while `Ok(dto)` serializes the object. A GET action that accidentally calls `return Ok()` rather than `return Ok(dto)` sends an empty 200 response to the client — the HTTP client library may throw a deserialization error or return a default empty object depending on the SDK. In unit tests, casting `result` to `OkObjectResult` throws `InvalidCastException` when the action returned `OkResult`. I always test the full return type when verifying action results.

---

#### Gotcha 8. Multiple possible return types and OpenAPI documentation

**Concepts**
- `[ProducesResponseType]` documenting each possible status code and body type
- `[ProducesDefaultResponseType]` for undocumented error shapes
- `ActionResult<T>` documenting the 200 success type automatically
- Undocumented 400/404/500 responses in OpenAPI spec

**Answer**

An action that can return `200 OrderDto`, `400 ValidationProblemDetails`, and `404 ProblemDetails` must have all three documented for generated clients to handle them correctly. `ActionResult<T>` documents the 200 schema automatically, but 400 and 404 must be declared with `[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]` and `[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]`. Omitting these declarations leaves client SDK generators with no error type information. I use an operation filter to globally inject the common error responses rather than repeating the attributes on every action.

---

#### Gotcha 9. `[ApiController]` binding source inference surprises with complex types

**Concepts**
- Complex types inferred as `[FromBody]` by `[ApiController]`
- Two complex parameters on one action — two body-bound params, only one body allowed
- `[FromServices]` explicitly required for injected service parameters on actions
- Primitive types inferred as `[FromRoute]` or `[FromQuery]`

**Answer**

With `[ApiController]`, complex type parameters are automatically inferred as `[FromBody]` — this is convenient for single-body actions but breaks when an action has two complex parameters, since HTTP only allows one request body. The second parameter also inferred as `[FromBody]` causes a binding conflict or silently binds as `null`. I explicitly annotate each parameter with `[FromBody]`, `[FromQuery]`, `[FromRoute]`, or `[FromServices]` when there is any ambiguity, especially in actions that receive both a route-identified resource and a command body.

---

#### Gotcha 10. Using `[HttpGet]` without `[Route]` on `ControllerBase` — missing attribute routing

**Concepts**
- `[ApiController]` enforces attribute routing — no conventional route fallback
- Controller without `[Route]` class attribute produces no routable endpoints
- Action-level `[HttpGet("path")]` only works when class-level `[Route]` is present or action has absolute path
- Conventional routing `MapControllerRoute` ignored for `[ApiController]` controllers

**Answer**

`[ApiController]` requires attribute routing — controllers with it cannot be reached via conventional `MapControllerRoute` route tables. A controller with `[ApiController]` but without a class-level `[Route]` attribute will simply not be reachable: action-level `[HttpGet("items")]` registers relative to an empty prefix, which may produce unexpected route shapes or 404s. I always place `[Route("api/[controller]")]` (or a literal path) on the controller class and use relative paths on action attributes, keeping the full URL visible at the class + action level.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this controller action under load. Integration tests pass locally; production threads spike and requests time out under concurrent traffic.

```csharp
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var report = _reportService.GenerateAsync(id).Result;
        return Ok(report);
    }
}
```

---

**Answer**

Blocking on `.Result` inside a synchronous action captures the request thread while waiting for async I/O. Under concurrent load this causes thread-pool starvation — each blocked thread holds a pool slot so Kestrel cannot handle new requests, and health checks time out too. Integration tests run at low concurrency and miss this entirely since starvation only manifests under parallel traffic. The fix is to change the action to `async Task<ActionResult<ReportDto>>` and `await _reportService.GenerateAsync(id)`, propagating the async pattern through the service to database and file I/O calls. Adding a parallel integration test or load test to CI for this endpoint catches the regression before production traffic exposes it.

---

#### Q2. (R) Review resource creation. Mobile clients create accounts successfully but cannot find the new user URI for subsequent PATCH calls — OpenAPI documents `201` with a Location header.

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var user = _users.Create(dto);
    return Ok(user);
}
```

`UsersController` uses `[Route("api/[controller]")]`; action name is `Create`.

---

**Answer**

`Ok(user)` returns `200` without a `Location` header, so mobile clients have no standard way to discover the canonical URI of the newly created user for PATCH or GET. The OpenAPI document promises `201` but the action delivers `200`, causing contract drift that breaks partner integration tests in CI. The fix is `return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto)` — this returns `201 Created` with a `Location` header pointing to the GET-by-id action. I also return a response DTO rather than the EF entity to avoid serializing navigation properties, and align Swagger metadata with `[ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]`.

---

#### Q3. (R) Review `CreatedAtAction` usage. After deploy, the Location header points to `GET /api/users/GetUser/5` which returns 404.

```csharp
[HttpPost]
public ActionResult<UserDto> Create([FromBody] CreateUserDto dto)
{
    var created = _users.Create(dto);
    return CreatedAtAction(
        nameof(GetUser),
        new { id = created.Id },
        created);
}

[HttpGet("{id:int}")]
public ActionResult<UserDto> GetById(int id) => Ok(_users.Get(id));
```

---

**Answer**

`CreatedAtAction` resolves the target action by name — `nameof(GetUser)` does not match `GetById`, so link generation targets a non-existent action and produces a wrong route. The fix is `return CreatedAtAction(nameof(GetById), new { id = created.Id }, created)`. Using `nameof` prevents compile-time errors when the string is wrong only if the symbol exists — if `GetUser` was renamed to `GetById`, `nameof(GetUser)` would fail to compile, but if the action was never named `GetUser` at all, the `nameof` expression points at nothing. I add a `WebApplicationFactory` integration test asserting `response.Headers.Location` is set and a follow-up `GET` to that URL returns `200`, since this bug only shows up when the full routing pipeline runs.

---

#### Q4. (P) When should a production Web API action return `ActionResult<T>` vs `IActionResult`, and how does that choice affect OpenAPI schema generation and unit testing?

---

**Answer**

I prefer `ActionResult<T>` when the success path returns a typed body and I still need to return `NotFound`, `BadRequest`, or other error results — Swagger documents `T` as the `200` response schema automatically, and the compiler assists when I `return dto` directly while still allowing `return NotFound()` via implicit conversion. I use bare `IActionResult` when the return shape varies widely — file downloads, redirects, or heterogeneous response types — since there is no single `T` to document. Using `Task<T>` alone on API actions is problematic when I need to return `404` or `400` without throwing; `ActionResult<T>` is the right union type. In unit tests, I assert `result.Result is OkObjectResult` and cast the typed value, rather than casting `IActionResult` to specific subtypes by string comparison. Minimal APIs use `Results<T>` for the same reason — type-safe success body with flexible error paths.

---

#### Q5. (R) Review DELETE and update responses. Cache invalidation middleware keys on status code; QA reports stale list pages after delete.

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    _repo.Delete(id);
    return Ok(new { deleted = true, id });
}

[HttpPut("{id:int}")]
public IActionResult Replace(int id, [FromBody] UpdateDto dto)
{
    _repo.Replace(id, dto);
    return Ok();
}
```

---

**Answer**

DELETE should return `204 NoContent` when there is no body — returning `200` with a JSON wrapper prevents cache invalidation middleware from recognizing delete semantics since it keys on `204` for DELETE operations. The ad-hoc `{ deleted: true }` envelope forces clients to parse an unnecessary payload for an operation that succeeded with no content to report. `Ok()` on PUT with no body is semantically ambiguous — it should be `204 NoContent` when the client does not need the updated entity, or `200` with the full updated DTO if echo-back is needed. I choose one and document it. The immediate fix for the stale list page bug is `return NoContent()` after DELETE, which aligns with the cache middleware's expectation.

---

#### Q6. (R) Review validation behavior. Frontend sends invalid JSON bodies but receives `200`-series responses with null fields processed as defaults.

```csharp
public class ProductsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        var product = _svc.Create(request);
        return Ok(product);
    }
}

public class CreateProductRequest
{
    [Required] public string Sku { get; set; } = "";
    [Range(0.01, 100000)] public decimal Price { get; set; }
}
```

No `[ApiController]` on the class; `[Required]` attributes present on the model.

---

**Answer**

Without `[ApiController]`, the automatic `400` validation response on model state failure is disabled — `[Required]` and `[Range]` attributes are evaluated but the failure does not automatically produce a `400` response. The invalid request reaches the service layer with an empty `Sku`, creating bad rows in the database. The fix is to add `[ApiController]` to the controller class, which activates the automatic `400` behavior so invalid requests never reach the action body. If `[ApiController]` cannot be added, the explicit guard `if (!ModelState.IsValid) return ValidationProblem(ModelState)` as the first line of the action achieves the same result. I also enable `AddProblemDetails()` in `Program.cs` for consistent RFC 7807 shapes across the API.

---

#### Q7. (D) Review this "thin controller" refactor proposal. The interviewer asks whether moving logic to the service layer actually fixed the design problems.

```csharp
[HttpPost("checkout")]
public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutRequest req)
{
    if (req.Items.Count == 0) return BadRequest("No items");
    if (!_inventory.HasStock(req.Items)) return Conflict("Out of stock");
    if (!_payments.Authorize(req.PaymentToken, req.Total)) return PaymentRequired();
    var order = await _orders.PlaceAsync(req);
    await _email.SendConfirmationAsync(order);
    await _analytics.TrackPurchaseAsync(order);
    return CreatedAtAction(nameof(Get), new { id = order.Id }, order.ToDto());
}
```

All dependencies are injected; no `[Authorize]` yet.

---

**Answer**

Injecting services moved data access off the controller but the action is still fat — it orchestrates stock check, payment authorization, order placement, email, and analytics as a sequential workflow, which means this logic cannot be reused from a message consumer or background job without duplicating the controller. A genuinely thin controller calls one application service or command handler and maps the result to HTTP: `CheckoutCommand` handled by `IOrderCheckoutHandler.CheckoutAsync` returning `Result<Order>`, with the controller mapping `Result.Success` to `CreatedAtAction` and `Result.Failure` to the appropriate `4xx`. The missing `[Authorize]` is a separate security gap — payment operations exposed without policy would be a security review failure regardless of layering. HTTP error mapping — `Conflict`, `PaymentRequired` — remains the controller's correct responsibility even in a thin design; it just receives a typed domain result rather than making business decisions itself.

---

#### Q8. (R) Review return types and leaked domain models. Security scan flags internal fields in JSON responses.

```csharp
[HttpGet("{id:int}")]
public IActionResult Get(int id)
{
    var entity = _db.Orders
        .Include(o => o.InternalNotes)
        .Include(o => o.PaymentAudit)
        .FirstOrDefault(o => o.Id == id);

    if (entity is null) return NotFound();
    return Ok(entity);
}
```

Entity types map 1:1 to EF Core tables; no `[JsonIgnore]` or DTO projection.

---

**Answer**

Returning the EF entity directly serializes every public property — including `InternalNotes` and `PaymentAudit` — because the serializer has no knowledge of which fields are internal. This is a data leak: sensitive audit and notes fields reach every API consumer. The entity's schema also changes with every migration, making the API contract unstable. The broad `.Include` calls over-fetch data that clients never requested, increasing response size and query cost. The fix is to project to `OrderDto` with only client-facing fields — `return Ok(entity.ToDto())` — and remove the broad `Include` calls in favor of projecting in the query or mapping in the service. An integration test asserting that the response JSON does not contain `InternalNotes` or `PaymentAudit` property names prevents regression.

---

#### Q9. (M) Compare `CreatedAtAction`, `CreatedAtRoute`, and `Created(uri, value)` for a multi-tenant API where the public URL is `https://api.example.com/tenant/{tenantId}/orders/{id}`. Which helper survives renamed actions and attribute route refactors?

```csharp
[Route("api/{tenantId}/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<OrderDto> Create(string tenantId, [FromBody] CreateOrderDto dto) { /* ... */ }
}
```

---

**Answer**

`CreatedAtAction(nameof(Get), new { tenantId, id = order.Id }, dto)` resolves via the routing system and includes all route values — it participates in `LinkGenerator` so path base, lowercase URL policy, and forwarded host headers are applied automatically. It survives action method renames when I use `nameof` and include `tenantId` in the route values; missing `tenantId` produces a `Location` URL with a dangling query parameter instead of the path segment. `CreatedAtRoute("GetOrderById", values, dto)` is more stable against action renames since the route name is independent of the method name — I use it when the GET action is stable but the method name changes frequently. `Created($"/tenant/{tenantId}/orders/{id}", dto)` hard-codes the path and breaks behind a path base, API management prefix, or lowercase URL policy without modification. For multi-tenant APIs I always pass all parent route parameters — `tenantId` and `id` — or the generated `Location` URL will be wrong and the follow-up GET will return `404`.

---
