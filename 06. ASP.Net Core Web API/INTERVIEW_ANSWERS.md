# ASP.NET Core Web API — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).  
Written for clarity and recall — concepts only, no interview coaching.

> **Scope:** All chapters

---

## Chapter 01. Introduction to REST & Web API

#### Q1. What is REST?

**Answer:** REST (Representational State Transfer) is an architectural style for distributed hypermedia systems that uses standard HTTP methods, stateless request/response exchanges, and resource-oriented URLs. It treats server-side data as resources addressable by URIs and manipulated through a uniform interface.

- Resources are identified by URLs such as `/api/orders/42`, not by action names in the path.
- Each request carries enough context (auth, representation format) because the server holds no client session state between calls.
- HTTP verbs express intent: GET reads, POST creates, PUT replaces, PATCH partial-updates, DELETE removes.
- Representations (JSON, XML) transfer resource state — the same resource can have multiple formats.
- ASP.NET Core 8 Web API implements REST principles through attribute-routed controllers, standard status codes, and content negotiation.

---

#### Q2. What is a Web API?

**Answer:** A Web API is an HTTP-based service that exposes application functionality to clients over the network, typically returning structured data (JSON) rather than HTML pages. In ASP.NET Core 8, Web APIs are built with controller or minimal API endpoints mapped to HTTP routes.

- Clients include browsers, mobile apps, partner systems, and microservices calling over HTTPS.
- APIs use HTTP methods, status codes, and headers as the contract instead of UI-specific responses.
- ASP.NET Core Web API projects use `AddControllers()`, `[ApiController]`, and `MapControllers()` in the middleware pipeline.
- Authentication, validation, and serialization are handled by the framework pipeline before and after action execution.
- Web APIs differ from MVC views in that they return machine-readable payloads, not Razor-rendered HTML.

---

#### Q3. What are the main REST architectural constraints?

**Answer:** REST defines six constraints: client-server separation, statelessness, cacheability, uniform interface, layered system, and optionally code-on-demand. Together they promote scalability, independent evolution of clients and servers, and predictable HTTP semantics.

- **Client-server:** UI concerns stay on the client; the server exposes data and operations through an API boundary.
- **Stateless:** Each request is self-contained — no server-side session state tied to a specific client connection.
- **Cacheable:** Responses declare cacheability so intermediaries can reduce load when appropriate.
- **Uniform interface:** Standard HTTP methods, resource identification via URI, and self-descriptive messages simplify integration.
- **Layered system:** Clients cannot tell whether they talk to the origin server, a gateway, or a cache — proxies and load balancers are transparent.
- **Code-on-demand (optional):** Server can extend client behavior by sending executable code — rarely used in JSON APIs.

---

#### Q4. What is HTTP idempotency, and which methods are idempotent?

**Answer:** An HTTP method is idempotent when repeating the same request multiple times produces the same side-effect state as executing it once. GET, PUT, DELETE, HEAD, and OPTIONS are idempotent; POST is not, because each call may create a new resource.

- **GET/HEAD/OPTIONS:** Safe reads — repeated calls do not change server state.
- **PUT:** Replacing `/api/items/5` with the same body twice converges to the same resource state.
- **DELETE:** Deleting an already-deleted resource may return 404, but the end state (resource absent) is the same.
- **POST:** Each retry can create duplicate records unless the server implements idempotency keys.
- Clients safely retry idempotent requests after network timeouts without fear of multiplying side effects.

---

#### Q5. What is the difference between PUT and POST?

**Answer:** POST creates a new resource at a collection URI and the server typically assigns the identifier; PUT replaces or creates a resource at a known URI supplied by the client. POST is not idempotent; PUT is idempotent on a specific resource path.

- `POST /api/orders` — server generates `id`, returns `201 Created` with a Location header.
- `PUT /api/orders/42` — client targets a known URI; repeated calls replace the same resource.
- POST is used when the client does not know the final resource URL before creation.
- PUT sends the full representation — omitted fields may be cleared depending on server semantics.
- ASP.NET Core actions map these with `[HttpPost]` and `[HttpPut("{id}")]` respectively.

---

#### Q6. What is the difference between PUT and PATCH?

**Answer:** PUT replaces the entire resource representation at a URI; PATCH applies a partial update, changing only the fields included in the request. Both target a known resource URI, but PATCH avoids sending the full object when only a few properties change.

- PUT requires the client to send the complete resource — missing properties may be set to null or default.
- PATCH sends a delta — JSON Merge Patch (`application/merge-patch+json`) or JSON Patch (`application/json-patch+json`) are common formats.
- PUT is idempotent; PATCH can be idempotent if the patch operation is defined that way, but complex patches may not be.
- ASP.NET Core 8 supports PATCH actions with DTOs or dedicated patch document types bound from the body.
- Choosing PATCH reduces bandwidth and concurrency conflicts when updating single fields like `status`.

---

#### Q7. What does it mean for an HTTP method to be "safe"?

**Answer:** A safe HTTP method does not modify server-side state — it only retrieves information. GET and HEAD are safe methods; clients, caches, and crawlers may invoke them without causing side effects.

- Safe methods can be prefetched, cached, and logged without risk of unintended mutations.
- Using GET for delete, charge, or submit operations violates safety and causes duplicate actions on refresh.
- Safe does not mean "harmless" — a GET may return sensitive data, so authorization still applies.
- POST, PUT, PATCH, and DELETE are unsafe because they change server state.
- REST-compliant APIs reserve GET exclusively for read operations.

---

#### Q8. When should an API return HTTP 201 Created vs 200 OK?

**Answer:** Return 201 Created when a POST (or sometimes PUT) successfully creates a new resource; return 200 OK when returning an existing resource or processing a request that does not create a new URI. 201 signals resource birth and typically includes a Location header.

- POST that inserts a record → `201 Created` with the new resource URI in the Location header.
- GET that returns data → `200 OK` with the representation in the body.
- PUT that updates an existing resource → `200 OK` or `204 No Content`, not 201.
- ASP.NET Core uses `CreatedAtAction`, `CreatedAtRoute`, or `Created(uri, value)` to produce 201 responses.
- Returning 200 on create hides the canonical URI from standard HTTP client libraries.

---

#### Q9. When should an API return HTTP 204 No Content?

**Answer:** Return 204 No Content when the request succeeded but there is no response body to send. Common cases include successful DELETE, PUT, or PATCH where the client does not need the updated representation.

- DELETE that removes a resource → `204 NoContent()` in ASP.NET Core.
- PUT/PATCH update where the client already has the data and does not need echo-back.
- 204 must not include a message body — only headers (e.g., ETag) may accompany it.
- Contrasts with 200 OK which includes a body with the resource or confirmation payload.
- Use 200 with a body when the client expects the updated entity in the response.

---

#### Q10. What is the difference between HTTP 400 Bad Request and 404 Not Found?

**Answer:** 400 Bad Request means the request was malformed or failed validation — the server understood the target but rejected the input. 404 Not Found means the requested resource URI or identifier does not exist on the server.

- Missing required fields, invalid JSON, or failed Data Annotations → 400 with `ValidationProblemDetails`.
- `GET /api/orders/99999` when order 99999 does not exist → 404.
- 400 is about request quality; 404 is about resource existence at the given address.
- `[ApiController]` in ASP.NET Core 8 automatically returns 400 for model validation failures.
- Returning 200 with an error message for either case breaks standard client error handling.

---

#### Q11. What is HATEOAS?

**Answer:** HATEOAS (Hypermedia As The Engine Of Application State) means API responses include links that describe available next actions on a resource, enabling clients to discover transitions dynamically. It is the highest level of the Richardson Maturity Model for REST.

- A response might include `_links: { "self": "...", "cancel": "..." }` pointing to related operations.
- Clients follow links instead of hard-coding URL patterns for every workflow step.
- Full HATEOAS adds contract complexity — many production APIs rely on OpenAPI documentation instead.
- ASP.NET Core can generate links via `LinkGenerator`, `CreatedAtAction`, or custom hypermedia envelopes.
- HATEOAS is optional in practice; resource-oriented URLs and correct HTTP verbs are the baseline.

---

#### Q12. What is RPC-style API design vs RESTful resource design?

**Answer:** RPC-style APIs expose operations as named endpoints (often POST to `/api/CreateOrder`), while RESTful design models nouns as resources manipulated by HTTP verbs on standard URIs (`POST /api/orders`). REST aligns with HTTP caching, idempotency, and standard tooling.

- RPC: `POST /api/orders/CancelOrder` — verb in URL, procedure-oriented.
- REST: `DELETE /api/orders/42` or `PATCH /api/orders/42` with `{ "status": "cancelled" }`.
- RPC is simpler for complex transactions but produces non-standard contracts and poor cache behavior.
- REST resources have stable identifiers and predictable CRUD mappings.
- Many APIs blend both — REST for CRUD, RPC-style sub-resources for non-CRUD workflows like `/api/orders/42/payments`.

---

#### Q13. What is the difference between REST and SOAP?

**Answer:** REST is an architectural style using HTTP with lightweight formats like JSON and human-readable URLs; SOAP is a protocol with strict XML envelopes, WSDL contracts, and transport-level features built into the specification. REST is the dominant choice for modern public and mobile APIs.

- REST uses standard HTTP methods and status codes; SOAP typically posts XML to a single endpoint.
- REST payloads are lean JSON; SOAP messages include envelope headers, body, and fault elements in XML.
- SOAP has built-in WS-* standards for transactions, security, and reliability — heavier but common in enterprise ESB integrations.
- REST scales well with browsers, CDNs, and HTTP/2; SOAP tooling is heavier but strongly typed via WSDL.
- ASP.NET Core 8 focuses on REST/JSON Web APIs; SOAP requires separate WCF or third-party middleware.

---

#### Q14. Why should GET requests not perform state-changing operations?

**Answer:** GET is defined as a safe, idempotent read — browsers, proxies, crawlers, and prefetchers may execute GET requests without user intent, causing unintended mutations. State changes belong on POST, PUT, PATCH, or DELETE.

- Refreshing a browser repeats the last GET — charging a payment via GET creates duplicate charges.
- GET URLs appear in server logs, browser history, and referrer headers — exposing sensitive operations.
- Caches may store GET responses — a destructive GET cached incorrectly is catastrophic.
- Search engines crawling GET delete links can remove data unintentionally.
- REST and HTTP specifications explicitly require GET to have no side effects.

---

#### Q15. What is a REST resource vs a REST collection?

**Answer:** A collection is a group of resources exposed at a plural noun URI (e.g., `/api/orders`), while an individual resource is a single item within that collection at a specific URI (e.g., `/api/orders/42`). Collections support listing and creation; individual resources support read, update, and delete.

- `GET /api/orders` — returns a list (collection) of order representations.
- `POST /api/orders` — creates a new member in the collection.
- `GET /api/orders/42` — returns a single resource identified by `42`.
- `PUT /api/orders/42` — replaces that specific resource.
- Nested collections use path hierarchy: `/api/customers/3/orders` for orders belonging to customer 3.

---

#### Q16. What is the purpose of the Location header on a 201 response?

**Answer:** The Location header on a 201 Created response tells the client the canonical URI of the newly created resource. Clients use it to fetch, update, or link to the resource without parsing the response body for an identifier.

- Example: `Location: https://api.example.com/api/orders/42` after creating order 42.
- Enables standard HTTP client libraries to follow the URI automatically.
- ASP.NET Core generates it via `CreatedAtAction`, `CreatedAtRoute`, or `Created(uri, value)`.
- The header value should be an absolute or root-relative URI reachable by the client.
- Omitting Location on 201 breaks REST client expectations and mobile SDK conventions.

---

#### Q17. What is API versioning and why is it needed?

**Answer:** API versioning allows multiple incompatible contract generations to coexist so existing clients keep working while new clients adopt improved endpoints. It is needed because breaking changes to URLs, fields, or behavior would otherwise force all consumers to update simultaneously.

- Breaking changes include renaming fields, changing types, or removing endpoints.
- Versioning strategies include URL path (`/api/v2/orders`), headers (`X-Api-Version`), query strings, or media type negotiation.
- ASP.NET Core uses packages like `Asp.Versioning.Mvc` with `[ApiVersion("2.0")]` attributes.
- Non-breaking additive changes (new optional fields) often do not require a new version.
- Deprecation policies and sunset headers communicate timeline for retiring old versions.

---

#### Q18. What makes an endpoint "RESTful" vs merely "HTTP-based"?

**Answer:** A RESTful endpoint models resources with noun-based URIs, uses HTTP methods according to their defined semantics, returns appropriate status codes, and stays stateless. An HTTP-based endpoint simply uses HTTP transport but may ignore method semantics, use verb-filled URLs, or wrap all responses in 200 OK.

- RESTful: `DELETE /api/items/5` returns 204; HTTP-based: `POST /api/items/delete/5` returns 200 with `{ "success": true }`.
- RESTful responses use standard status codes; HTTP-based APIs embed success/failure flags in the body.
- RESTful APIs are cacheable and idempotent where the HTTP specification expects it.
- Merely returning JSON over HTTP does not make an API RESTful.
- ASP.NET Core 8 provides conventions (`[ApiController]`, attribute routing) that steer toward RESTful design.

---

## Chapter 02. API Controllers & Action Results

#### Q1. What is an API controller in ASP.NET Core?

**Answer:** An API controller is a class decorated with `[ApiController]` and route attributes that handles HTTP requests and returns data responses (JSON, XML, files) instead of views. It inherits from `ControllerBase` and contains action methods mapped to HTTP verbs.

- Registered via `builder.Services.AddControllers()` and `app.MapControllers()` in ASP.NET Core 8.
- Actions return `IActionResult`, `ActionResult<T>`, or concrete result types instead of `View()`.
- Constructor injection provides scoped services like repositories or application services.
- API controllers participate in model binding, validation, content negotiation, and filter pipelines.
- They are discovered by the routing system through attribute route templates on the class and actions.

---

#### Q2. What does the `[ApiController]` attribute do?

**Answer:** `[ApiController]` enables opinionated API behaviors: automatic 400 responses for validation failures, binding source inference, attribute routing requirements, and ProblemDetails-compatible error responses. It signals the framework to apply Web API conventions instead of MVC view conventions.

- Invalid model state automatically produces HTTP 400 with `ValidationProblemDetails` — no manual `ModelState` check needed.
- Complex types from the body are inferred as `[FromBody]`; route values as `[FromRoute]`.
- `[ApiController]` requires attribute routing — conventional `{controller}/{action}` routing is not used.
- Multipart form and `[FromForm]` binding behave differently with source inference enabled.
- All controllers in a project should consistently use or omit `[ApiController]` to avoid mixed error contracts.

---

#### Q3. What is the difference between `ControllerBase` and `Controller`?

**Answer:** `ControllerBase` provides core API features (action results, model binding, HTTP context) without view-related helpers; `Controller` inherits `ControllerBase` and adds view methods like `View()`, `PartialView()`, and `ViewBag`. Web APIs inherit `ControllerBase`; MVC apps with Razor views inherit `Controller`.

- `ControllerBase` includes `Ok()`, `NotFound()`, `BadRequest()`, `CreatedAtAction()`, and related helpers.
- `Controller` adds `View()`, `ViewData`, `ViewBag`, and TempData for HTML rendering.
- API projects should use `ControllerBase` to avoid accidental view dependencies.
- Both support authorization attributes, filters, and model binding identically.
- Minimal APIs bypass controller classes entirely but produce equivalent HTTP responses.

---

#### Q4. What is `IActionResult`?

**Answer:** `IActionResult` is the non-generic interface representing an HTTP response produced by executing a controller action — status code, headers, and optional body. Action methods return `IActionResult` or `Task<IActionResult>` to defer response execution until the result pipeline runs.

- Concrete implementations include `OkObjectResult`, `NotFoundResult`, `CreatedAtActionResult`, and `FileResult`.
- Helper methods like `Ok()`, `NotFound()`, and `BadRequest()` return `IActionResult` instances.
- The result executor selects an output formatter and writes the response after the action returns.
- `IActionResult` allows one action to return different result types based on conditions.
- OpenAPI tooling inspects `[ProducesResponseType]` attributes when the return type is non-generic `IActionResult`.

---

#### Q5. What is `ActionResult<T>` and how does it differ from `IActionResult`?

**Answer:** `ActionResult<T>` is a generic union type that documents the success response type `T` while still allowing error results like `NotFound()` or `BadRequest()`. It improves OpenAPI schema generation and compile-time clarity compared to bare `IActionResult`.

- Success: `return customerDto;` implicitly wraps as 200 OK with typed body.
- Error: `return NotFound();` still compiles — implicit conversion to `ActionResult<T>`.
- Swagger documents `T` as the 200 response schema automatically.
- Prefer `ActionResult<T>` when the happy path returns a known DTO type.
- Use `IActionResult` when responses vary widely (files, redirects, heterogeneous shapes).

---

#### Q6. What is `CreatedAtAction` and when do you use it?

**Answer:** `CreatedAtAction` returns HTTP 201 Created with a Location header generated by resolving a named action and route values through the routing system. Use it after POST (or PUT that creates) when the client needs the canonical URI of the new resource.

- Example: `return CreatedAtAction(nameof(GetById), new { id = order.Id }, orderDto);`
- The Location header points to the GET action that retrieves the created resource.
- Route values must include all template parameters (`id`, `tenantId`, etc.) or the URL 404s.
- Returns 201 with the created entity (or DTO) in the response body.
- Preferred over manual URL strings because it respects path base, lowercase URLs, and route refactors.

---

#### Q7. What is the difference between `CreatedAtAction` and `CreatedAtRoute`?

**Answer:** `CreatedAtAction` generates the Location URL by action method name and route values; `CreatedAtRoute` generates it by a named route defined with `Name = "..."` on a route attribute. Both return 201 Created — the choice depends on whether you anchor links to action names or stable route names.

- `CreatedAtAction(nameof(GetById), new { id = 5 }, dto)` — resolves via action name.
- `CreatedAtRoute("GetOrderById", new { id = 5 }, dto)` — requires `[HttpGet("{id}", Name = "GetOrderById")]`.
- Named routes survive action method renames if the route name stays constant.
- Both participate in `LinkGenerator` and respect `UsePathBase` when configured correctly.
- `Created(string uri, object value)` bypasses routing but breaks when URLs change behind gateways.

---

#### Q8. What does the `Ok()` helper return?

**Answer:** `Ok()` returns HTTP 200 OK — either with no body (`Ok()`) or with a serialized object (`Ok(dto)`). It is the standard success response for GET requests and updates where the client expects the resource in the body.

- `Ok()` produces `OkResult` — status 200, empty body.
- `Ok(dto)` produces `OkObjectResult` — status 200 with JSON/XML serialized body.
- Content negotiation selects the output formatter based on Accept header and `[Produces]`.
- Use 200 for successful reads and updates that return a representation.
- Do not use `Ok()` for resource creation — use `CreatedAtAction` for 201 instead.

---

#### Q9. What does `NoContent()` return and when is it appropriate?

**Answer:** `NoContent()` returns HTTP 204 with an empty body, signaling successful processing without a response payload. It is appropriate for DELETE and for PUT/PATCH when the client does not need the updated entity echoed back.

- Maps to `NoContentResult` in the action result pipeline.
- Must not include a response body — clients expect zero content length.
- Common for DELETE: `return NoContent();` after removing a resource.
- Alternative to `Ok()` when returning a resource after update is unnecessary.
- Cache invalidation middleware and HTTP clients rely on 204 semantics for delete operations.

---

#### Q10. What is the difference between `NotFound()` and `BadRequest()`?

**Answer:** `NotFound()` returns HTTP 404 when the requested resource does not exist at the given identifier; `BadRequest()` returns HTTP 400 when the request is invalid, malformed, or fails validation. They map to different failure categories in the HTTP specification.

- `NotFound()` — `GET /api/orders/999` where order 999 is absent.
- `BadRequest()` — invalid JSON, missing required fields, or business rule violation on input.
- `BadRequest(ModelState)` or `ValidationProblem()` includes field-level error details.
- `[ApiController]` auto-returns 400 for validation failures without explicit `BadRequest()` calls.
- Using the wrong status code breaks client retry logic and monitoring alerts.

---

#### Q11. What HTTP status does `Conflict()` map to?

**Answer:** `Conflict()` returns HTTP 409 Conflict, indicating the request could not be completed due to a conflict with the current state of the resource. Typical cases include duplicate unique keys, version mismatch, or concurrent update failures.

- Example: creating a user with an email that already exists.
- Also used for optimistic concurrency failures when an ETag or row version does not match.
- Distinct from 400 (bad input) and 404 (resource missing).
- ASP.NET Core maps `return Conflict()` to `ConflictResult` or `ConflictObjectResult` with optional body.
- Clients may fetch the current resource state and retry after receiving 409.

---

#### Q12. Why should API actions return DTOs instead of EF entities?

**Answer:** DTOs expose only the fields clients need, decoupling the HTTP contract from the database schema and preventing leaks of internal data, navigation properties, and circular references. EF entities change with migrations; DTOs provide a stable, intentional API surface.

- Entities may serialize `InternalNotes`, audit fields, or lazy-loaded navigation graphs unintentionally.
- Circular references between entities cause `JsonException` at runtime.
- DTOs allow different read and write shapes (`CreateOrderDto` vs `OrderResponseDto`).
- Projection with `Select` in EF Core queries avoids over-fetching and N+1 problems.
- Map entities to DTOs in the service layer or with tools like AutoMapper before returning from actions.

---

#### Q13. What is the Location header used for in API responses?

**Answer:** The Location header specifies the URI of a resource — most commonly on 201 Created responses to point clients to the newly created item. It can also appear on 3xx redirects to indicate where the client should look next.

- Generated automatically by `CreatedAtAction`, `CreatedAtRoute`, and `Created` helpers.
- Clients use it to fetch the resource without parsing the response body for an ID.
- Must reflect the publicly reachable URI, accounting for reverse proxy path bases.
- Integration tests should assert Location header value and verify follow-up GET succeeds.
- An incorrect Location header causes 404 on the client's next request.

---

#### Q14. What is the difference between synchronous and asynchronous controller actions?

**Answer:** Synchronous actions return `IActionResult` directly and block the request thread during I/O; asynchronous actions return `Task<IActionResult>` or `Task<ActionResult<T>>` and release the thread while awaiting I/O operations. ASP.NET Core 8 expects async actions for database, HTTP, and file operations.

- Async: `public async Task<ActionResult<OrderDto>> Get(int id) => Ok(await _service.GetAsync(id));`
- Sync: `public IActionResult Get(int id) => Ok(_service.Get(id));` — acceptable only for in-memory work.
- Async improves scalability under concurrent load by not holding threads during I/O waits.
- Kestrel handles thousands of concurrent connections efficiently with async actions.
- Action signature must use `async`/`await` all the way through the call chain.

---

#### Q15. What problem does blocking on `.Result` cause in API controllers?

**Answer:** Calling `.Result` or `.Wait()` on an incomplete `Task` inside an async-capable request blocks the thread pool thread while the async operation continues on another thread, causing thread-pool starvation and potential deadlocks under load. Always use `await` instead.

- Common anti-pattern: `var data = _service.GetAsync(id).Result;` in a sync action.
- Under concurrency, blocked threads accumulate and Kestrel cannot accept new requests.
- Classic deadlock when the continuation needs the same synchronization context.
- Integration tests with low concurrency miss this — production traffic exposes it.
- Fix: make the action `async Task<ActionResult<T>>` and `await` the service call.

---

#### Q16. What is the difference between returning `Ok(entity)` and `CreatedAtAction` for POST?

**Answer:** `Ok(entity)` on POST returns HTTP 200 without a Location header — treating creation as a generic success. `CreatedAtAction` returns HTTP 201 with a Location header pointing to the new resource URI, following REST create semantics.

- 200 on create hides the canonical URL from REST clients and mobile SDKs.
- 201 explicitly signals resource birth and provides the address for subsequent GET/PATCH/DELETE.
- `CreatedAtAction` includes the created DTO in the body plus the Location header.
- OpenAPI documents should declare 201 for POST create endpoints, not 200.
- Use `Ok()` on POST only for non-create operations like search or command processing.

---

#### Q17. What does `[ProducesResponseType]` do on an API action?

**Answer:** `[ProducesResponseType]` declares possible HTTP status codes and response body types for an action, feeding Swagger/OpenAPI generation and API explorer metadata. It documents the contract beyond what the return type alone conveys.

- Example: `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]`
- Multiple attributes document different outcomes: 200, 404, 400 on the same action.
- Essential when the return type is `IActionResult` and the success type is not inferable.
- OpenAPI clients use this to generate typed response handling code.
- Does not change runtime behavior — it is metadata for documentation and tooling.

---

#### Q18. What is a "thin controller" in Web API design?

**Answer:** A thin controller validates input, authorizes the caller, calls one application service method, and maps the result to an HTTP response — it contains no business logic, data access, or cross-cutting orchestration. Fat controllers embed rules that belong in services, handlers, or domain layers.

- Controller: check auth → validate → `_orderService.CreateAsync(dto)` → `CreatedAtAction`.
- Business rules, transactions, and email sending live in the service or MediatR handler.
- Thin controllers are easier to unit test — mock the service, assert HTTP mapping.
- Mapping domain exceptions to HTTP status codes (404, 409) is the controller's job.
- Injecting services alone does not make a controller thin if orchestration remains in the action.

---

## Chapter 03. Routing & API Conventions

#### Q1. What is attribute routing in ASP.NET Core Web API?

**Answer:** Attribute routing maps URLs to controller actions using `[Route]`, `[HttpGet]`, `[HttpPost]`, and related attributes placed on classes and methods. The route template is co-located with the action it serves, replacing conventional `{controller}/{action}` patterns.

- Class level: `[Route("api/[controller]")]` sets the base prefix.
- Action level: `[HttpGet("{id:int}")]` appends segments and HTTP method constraints.
- Registered via `MapControllers()` — no `MapControllerRoute` needed for pure API projects.
- Attribute routes support route constraints, defaults, and named routes.
- ASP.NET Core 8 Web API projects use attribute routing exclusively for REST endpoints.

---

#### Q2. What does the `[Route("api/[controller]")]` template mean?

**Answer:** This template creates a route prefix `api/` followed by the controller name with the `Controller` suffix removed — `OrdersController` becomes `api/orders`. Action-level route attributes append to this base template.

- `[controller]` is a token replaced at runtime by the framework.
- `OrdersController` with `[HttpGet("{id}")]` matches `GET /api/orders/{id}`.
- Token replacement is convention-based — renaming the class changes the public URL.
- Combine with `RouteOptions.LowercaseUrls = true` to emit lowercase segments.
- For stable public URLs, use a literal: `[Route("api/orders")]` instead of `[controller]`.

---

#### Q3. What is the difference between attribute routing and conventional routing for Web APIs?

**Answer:** Attribute routing defines URLs on each action with attributes; conventional routing uses centralized route templates in `Program.cs` like `{controller=Home}/{action=Index}/{id?}`. Web APIs use attribute routing; conventional routing is primarily for MVC views.

- Conventional: `/Orders/GetOrderById/5` — exposes action names in URLs.
- Attribute: `/api/orders/5` — RESTful resource paths with HTTP verb constraints.
- Conventional routing makes multiple GET actions on one controller collide.
- Attribute routing co-locates the contract with the handler for clarity and OpenAPI discovery.
- `[ApiController]` requires attribute routing — conventional routes are ignored for API controllers.

---

#### Q4. Why is attribute routing preferred for REST APIs?

**Answer:** Attribute routing expresses resource-oriented URLs directly on actions, supports HTTP method constraints, and integrates cleanly with Swagger and `[ApiController]` binding inference. It avoids leaking implementation names and enables precise RESTful path design.

- Templates like `[HttpGet("{id:int}")]` and `[HttpPost]` map cleanly to REST verbs.
- Each action declares its own URL — no collision between multiple GET methods.
- OpenAPI/Swashbuckle reads attribute routes to generate accurate path documentation.
- Route constraints (`{id:int}`) disambiguate overlapping templates.
- Teams can apply consistent prefixes via conventions without sacrificing per-action control.

---

#### Q5. What is the `[controller]` token in a route template?

**Answer:** The `[controller]` token is a route parameter placeholder replaced with the controller class name minus the `Controller` suffix. It keeps route templates DRY but ties public URLs to C# class names.

- `ProductsController` → segment `Products` (or `products` with lowercase URLs).
- Renaming `ProductsController` to `ItemsController` changes the URL from `/api/products` to `/api/items`.
- Override with `[ControllerName("products")]` to decouple class name from URL segment.
- Prefer literal route segments for public APIs where URL stability matters across refactors.
- The token works at the class-level `[Route]` template, not inside action templates alone.

---

#### Q6. What is RESTful route design for resource URLs?

**Answer:** RESTful routes use plural nouns for collections, path parameters for identifiers, and HTTP verbs for operations — avoiding action verbs in the path. Nesting expresses ownership: `/api/customers/3/orders/7`.

- Collections: `GET /api/orders`, `POST /api/orders`.
- Single resource: `GET /api/orders/42`, `PUT /api/orders/42`, `DELETE /api/orders/42`.
- Sub-resources: `POST /api/orders/42/payments` for non-CRUD operations on a parent.
- No verbs in paths: reject `/api/orders/create` or `/api/getOrderById`.
- Query strings handle filtering and pagination: `GET /api/orders?status=shipped&page=2`.

---

#### Q7. What is route ambiguity and how can it occur in API routing?

**Answer:** Route ambiguity occurs when two or more actions match the same HTTP method and URL pattern, leaving the framework unable to select a single endpoint. It arises from overlapping templates, missing constraints, or duplicate HTTP method registrations.

- `{category}` (string) and `{id:int}` both match `/api/products/10` — string wins unpredictably.
- Two `[HttpDelete("{id}")]` actions with different parameter types cause ambiguous matches.
- Literal segments (`featured`) must be declared separately from parameterized segments.
- Fix with route constraints (`{id:int}`), distinct path prefixes, or consolidated actions.
- ASP.NET Core logs ambiguous match warnings at startup or returns 500 at runtime.

---

#### Q8. What are nested resource routes?

**Answer:** Nested resource routes express parent-child relationships in the URL hierarchy, placing the child resource under the parent's path. They indicate scope and ownership without embedding verbs.

- Example: `GET /api/customers/3/orders/7` — order 7 belonging to customer 3.
- Controller route: `[Route("api/customers/{customerId}/orders")]` with `[HttpGet("{orderId:int}")]`.
- `CreatedAtAction` must include all parent route values (`customerId`, `orderId`) in link generation.
- Deep nesting (>2 levels) becomes unwieldy — balance REST purity with URL readability.
- Authorization often checks parent ownership: customer 3 can only access their own orders.

---

#### Q9. What is `RouteOptions.LowercaseUrls`?

**Answer:** `RouteOptions.LowercaseUrls` is a global configuration that generates all URL paths in lowercase during link generation and route matching. Setting it to `true` makes `/api/Orders` resolve as `/api/orders`.

- Configured in `Program.cs`: `builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);`.
- Affects `CreatedAtAction`, `LinkGenerator`, and URL generation throughout the app.
- Case-sensitive clients must use lowercase URLs consistently after enabling.
- Does not change controller class names — only the emitted URL segments.
- Partner documentation and integration tests must align with the lowercase policy.

---

#### Q10. What is `UsePathBase` and how does it affect API URLs?

**Answer:** `UsePathBase` strips a path prefix from incoming requests so the app can be hosted behind a reverse proxy or sub-path without rewriting every route attribute. Link generation must include the same path base for Location headers to be correct.

- Example: gateway exposes `/api/v1/store/orders/5` but the app routes `/orders/5`.
- `app.UsePathBase("/api/v1/store")` must run early in the middleware pipeline.
- `CreatedAtAction` includes PathBase when the request carries it — test clients must set it too.
- Combine with `ForwardedHeaders` middleware for correct public scheme and host in absolute URLs.
- Mismatch between PathBase and gateway config causes 404s and broken Location headers.

---

#### Q11. What do `[HttpGet]`, `[HttpPost]`, etc. specify?

**Answer:** HTTP method attributes (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, `[HttpDelete]`) constrain which HTTP verb matches an action and optionally append a route template segment. They combine with the controller's base `[Route]` to form the full URL pattern.

- `[HttpGet]` on an action with no template matches the controller base route.
- `[HttpGet("{id:int}")]` adds `{id}` segment and restricts to GET requests.
- POST to a GET-only action returns 405 Method Not Allowed.
- Each action typically carries exactly one HTTP method attribute.
- Method attributes are required for API controllers — bare `[Route]` without a verb does not expose the action.

---

#### Q12. What is the difference between route templates on controller vs action?

**Answer:** The controller-level `[Route]` sets the shared prefix for all actions in that class; action-level `[HttpGet("...")]` templates append relative segments and HTTP method constraints. Together they form the complete route pattern.

- Controller: `[Route("api/v1/[controller]")]` → base `/api/v1/orders`.
- Action: `[HttpGet("{id:int}")]` → full route `GET /api/v1/orders/{id}`.
- Action templates starting with `~/` override the controller prefix entirely.
- Action templates starting with `/` are absolute from the application root.
- Keeping version and prefix at controller level avoids repeating them on every action.

---

#### Q13. What is link generation in ASP.NET Core routing?

**Answer:** Link generation creates URLs from route names, action names, and route values using the routing system's `LinkGenerator` service. It powers `CreatedAtAction`, `CreatedAtRoute`, `Url.Action`, and hypermedia links in responses.

- Input: action name + `{ id = 5, tenantId = 3 }` → output: `/api/tenants/3/orders/5`.
- Respects route constraints, lowercase URL options, and path base settings.
- Broken link generation (wrong action name, missing route value) produces 404 Location headers.
- Named routes (`Name = "GetOrder"`) decouple link generation from method renames.
- Prefer link generation over hard-coded URL strings for maintainability behind gateways.

---

#### Q14. Why should API routes use nouns instead of verbs?

**Answer:** Nouns identify resources; HTTP methods express the operation — combining verbs in URLs duplicates method semantics and produces RPC-style endpoints that break caching, idempotency, and standard REST tooling. `/api/orders` with POST creates; `/api/createOrder` does not.

- Verbs in paths: `/api/orders/cancel/42` — non-standard, not cacheable, hard to document.
- Nouns with methods: `DELETE /api/orders/42` or `PATCH /api/orders/42` with status change.
- Gateway policies, rate limits, and monitoring rules apply per HTTP method on resource paths.
- OpenAPI groups operations by resource path, not by verb-named endpoints.
- Exception: sub-resource actions that are not CRUD may use nouns for processes: `/api/orders/42/cancellations`.

---

#### Q15. What is a route constraint (e.g., `{id:int}`)?

**Answer:** A route constraint restricts which values match a route parameter — `{id:int}` accepts only integers, `{slug:alpha}` only letters. Constraints disambiguate overlapping templates and reject invalid parameter types early.

- `{id:int}` — matches `42`, rejects `abc` (returns 404 instead of hitting wrong action).
- `{date:datetime}` — validates date format in the URL segment.
- Custom constraints implement `IRouteConstraint` for domain-specific rules.
- Without constraints, `{category}` (string) competes with `{id:int}` for numeric paths.
- Constraints appear in attribute routes: `[HttpGet("{id:int}")]` on the action.

---

#### Q16. What happens when two actions match the same route?

**Answer:** ASP.NET Core cannot dispatch the request to a single action and throws an `AmbiguousMatchException` at runtime or logs a warning at startup. The request fails with HTTP 500 unless one action is removed or templates are disambiguated.

- Common cause: two actions with identical `[HttpGet("{id}")]` on the same controller.
- Another cause: `{id:int}` and `{id:guid}` both matching certain values (rare).
- Fix by adding constraints, merging into one action, or using different path prefixes.
- Swagger may show duplicate operations before the runtime failure is discovered.
- Code review should reject duplicate HTTP method + template combinations on one controller.

---

#### Q17. What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?

**Answer:** Class-level `[Route]` defines the shared URL prefix for the controller; action-level `[HttpGet("path")]` adds a relative segment and binds the action to the GET verb. `[Route]` alone on an action without an HTTP method attribute does not create an endpoint in API projects.

- Class: `[Route("api/[controller]")]` — all actions inherit this prefix.
- Action: `[HttpGet("{id}")]` — matches GET with `{id}` appended to the prefix.
- `[HttpGet("active")]` → `GET /api/orders/active` (literal segment before parameters).
- Action-only `[Route("api/orders")]` without `[HttpGet]` is insufficient for API controllers.
- HTTP method attributes carry optional route templates; bare `[Route]` on actions is for non-API MVC scenarios.

---

#### Q18. How does `[ApiController]` affect route parameter binding?

**Answer:** `[ApiController]` enables binding source inference — simple types from route templates bind as `[FromRoute]`, complex types from the body bind as `[FromBody]`, and query parameters bind as `[FromQuery]` without explicit attributes. This reduces boilerplate but requires understanding the inference rules.

- `Get(int id)` with `{id}` in the route template → bound from route automatically.
- `Post(OrderDto dto)` → bound from request body automatically.
- `Get([FromQuery] string filter)` — simple types not in the route template default to query string.
- `[FromServices]` must still be explicit for service injection into action parameters.
- Misunderstanding inference causes silent null values — e.g., expecting body binding on a simple type gets query binding instead.

---

## Chapter 04. Content Negotiation & Formatters

#### Q1. What is content negotiation in ASP.NET Core Web API?

**Answer:** Content negotiation is the process by which the server selects a response format (JSON, XML, CSV) based on the client's `Accept` header and the available output formatters registered in the application. It ensures clients receive data in a representation they can process.

- Client sends `Accept: application/json` → server serializes with System.Text.Json.
- If no formatter matches and fallback is disabled, the server returns 406 Not Acceptable.
- `[Produces("application/json")]` on actions restricts or declares supported output types.
- Input content negotiation uses the `Content-Type` header to select input formatters.
- ASP.NET Core 8 Web API defaults to JSON; additional formatters must be explicitly registered.

---

#### Q2. What is the Accept HTTP header used for?

**Answer:** The Accept header tells the server which response media types the client prefers, ordered by quality values (`q=`). The server selects the best matching output formatter during content negotiation.

- Example: `Accept: application/json, application/xml;q=0.9`.
- Server picks the highest-preference type it can produce with a registered formatter.
- If the server cannot satisfy any listed type, it may return 406 or fall back to a default.
- Accept governs response format; it does not describe the request body format.
- API clients should set Accept explicitly; browsers default to `*/*`.

---

#### Q3. What is the Content-Type header used for in API requests?

**Answer:** The Content-Type header declares the media type of the request body so the server selects the correct input formatter for deserialization. It is required on POST, PUT, and PATCH requests that include a body.

- `Content-Type: application/json` → System.Text.Json input formatter deserializes the body.
- `Content-Type: application/xml` → XML input formatter if registered.
- Mismatch between Content-Type and actual body format causes model binding failures or 415 Unsupported Media Type.
- `[Consumes("application/json")]` on an action rejects requests with other Content-Type values.
- Multipart form uploads use `Content-Type: multipart/form-data` for file uploads.

---

#### Q4. What is the default JSON serializer in ASP.NET Core 8?

**Answer:** ASP.NET Core 8 uses `System.Text.Json` as the default JSON serializer for both input and output formatters in Web API projects. It replaces Newtonsoft.Json as the framework default, though Newtonsoft can be added optionally.

- Registered automatically when calling `AddControllers()` in the Web SDK template.
- Configured via `AddJsonOptions()` on `JsonSerializerOptions`.
- Default naming policy is camelCase for property names in JSON.
- System.Text.Json is faster and uses less memory than Newtonsoft.Json for typical API payloads.
- Add `AddNewtonsoftJson()` only when legacy features like `$type` polymorphism or specific converters are required.

---

#### Q5. What is camelCase JSON naming and why is it used in Web APIs?

**Answer:** camelCase names JSON properties with a lowercase first letter (`customerName`) matching JavaScript and front-end conventions, while C# properties remain PascalCase (`CustomerName`). ASP.NET Core 8 applies camelCase by default via `JsonNamingPolicy.CamelCase`.

- Outbound: `CustomerName` serializes as `"customerName"` in JSON responses.
- Inbound: JSON keys must match camelCase unless `PropertyNameCaseInsensitive = true`.
- Consistency with JavaScript clients avoids manual property mapping on the front end.
- OpenAPI/Swagger schemas reflect camelCase names in generated client code.
- PascalCase JSON from legacy clients may fail to bind, causing silent null defaults.

---

#### Q6. What is the difference between input formatters and output formatters?

**Answer:** Input formatters deserialize the request body into action parameters during model binding; output formatters serialize the action result into the response body during result execution. Each set is selected independently based on Content-Type (input) and Accept (output).

- Input: runs before the action — `Content-Type: application/json` → `JsonInputFormatter`.
- Output: runs after the action — `Accept: application/json` → `JsonOutputFormatter`.
- Input formatters populate `[FromBody]` parameters; output formatters write `OkObjectResult` values.
- Custom formatters implement `InputFormatter` or `OutputFormatter` base classes.
- An API can accept JSON and return CSV by registering different formatters for each direction.

---

#### Q7. What HTTP status code is returned when content negotiation fails?

**Answer:** HTTP 406 Not Acceptable is returned when the server cannot produce any representation matching the client's Accept header and no fallback formatter is configured. Without explicit 406 configuration, the framework may silently fall back to the default JSON formatter.

- Client sends `Accept: application/pdf` but only JSON formatters are registered → 406.
- Enable strict behavior via formatter options or ensure `[Produces]` matches registered formatters.
- Returning 200 with JSON when the client requested XML violates HTTP semantics.
- 415 Unsupported Media Type is the input-side equivalent — unsupported Content-Type on the request.
- Integration tests should assert 406 for unsupported Accept headers on strict APIs.

---

#### Q8. What does `[Produces("application/json")]` do?

**Answer:** `[Produces]` declares the content types an action can return, influencing content negotiation and OpenAPI documentation. When only JSON is listed, the formatter pipeline selects the JSON output formatter for that action.

- Metadata attribute — documents intent for Swagger/Swashbuckle schema generation.
- Can list multiple types: `[Produces("application/json", "application/xml")]` if both formatters exist.
- Declaring a type without a registered formatter misdocuments the API in OpenAPI.
- Helps restrict formatters so CSV or XML formatters do not accidentally handle JSON endpoints.
- Does not alone enforce 406 — formatter configuration controls fallback behavior.

---

#### Q9. What does `[Consumes("application/xml")]` do?

**Answer:** `[Consumes]` restricts which Content-Type values an action accepts for the request body, acting as an action constraint during endpoint selection. Requests with a non-matching Content-Type receive 415 Unsupported Media Type.

- `[Consumes("application/xml")]` — only XML body requests match this action.
- Used to isolate legacy XML endpoints while the rest of the API accepts JSON.
- Multiple types allowed: `[Consumes("application/json", "application/xml")]`.
- Requires a registered input formatter for each consumed type.
- Without a matching formatter, model binding fails even if the action is selected.

---

#### Q10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?

**Answer:** System.Text.Json is the built-in, high-performance serializer with camelCase defaults and strict standards compliance; Newtonsoft.Json (Json.NET) is an optional third-party serializer with richer features like reference loop handling, `$type` polymorphism, and broader custom converter support.

- System.Text.Json: faster, lower allocation, default in ASP.NET Core 8 templates.
- Newtonsoft: add via `AddNewtonsoftJson()` — needed for some legacy serialization scenarios.
- Configuration differs: `AddJsonOptions()` vs `AddNewtonsoftJson(options => ...)`.
- Mixing both in one app creates inconsistent naming and behavior across endpoints.
- Choose one serializer per application unless migrating incrementally with clear boundaries.

---

#### Q11. How does model binding relate to input formatters?

**Answer:** Model binding orchestrates populating action parameters from the HTTP request; for body parameters, it delegates to input formatters to deserialize the raw request stream into a CLR object. The input formatter is selected based on the request's Content-Type header.

- Route/query parameters bind directly without formatters.
- `[FromBody]` complex types trigger input formatter selection and deserialization.
- If no input formatter matches Content-Type, binding fails with 415 or empty model.
- Input formatters run before validation — Data Annotations validate the bound object afterward.
- `[ApiController]` infers `[FromBody]` for complex types without explicit attributes.

---

#### Q12. What is an output formatter?

**Answer:** An output formatter is a component that serializes the action result object into the HTTP response body in a specific media type. ASP.NET Core includes built-in JSON, XML, and string formatters, and supports custom formatters for CSV, PDF, or other formats.

- Selected during result execution based on Accept header, `[Produces]`, and `CanWriteType`.
- `JsonOutputFormatter` uses System.Text.Json to write JSON responses.
- Custom formatters extend `TextOutputFormatter` or `OutputFormatter` and register in `AddControllers(options => options.OutputFormatters.Add(...))`.
- `CanWriteType` must accurately declare supported CLR types — returning true for everything causes mismatches.
- Output formatters set the response Content-Type header (e.g., `application/json; charset=utf-8`).

---

#### Q13. What is the difference between JSON and XML responses in Web APIs?

**Answer:** JSON is the default lightweight text format used by modern Web APIs — compact, JavaScript-native, and supported by System.Text.Json out of the box. XML is verbose, schema-heavy, and required by some legacy enterprise integrations — it needs explicit formatter registration in ASP.NET Core 8.

- JSON: `{"customerName":"Acme","creditLimit":5000}` — default in ASP.NET Core 8.
- XML: `<Customer><CustomerName>Acme</CustomerName></Customer>` — requires `AddXmlSerializerFormatters()`.
- JSON dominates mobile, SPA, and microservice communication.
- XML persists in banking, government, and SOAP-adjacent integrations.
- Supporting both requires registering both formatters and testing content negotiation for each.

---

#### Q14. What does `[JsonPropertyName]` do?

**Answer:** `[JsonPropertyName("custom_name")]` overrides the default naming policy for a single property, forcing a specific JSON key during serialization and deserialization. It applies when using System.Text.Json.

- Example: `[JsonPropertyName("customer_name")]` maps C# `CustomerName` to JSON `"customer_name"`.
- Overrides camelCase policy for that property only — creates a mixed naming contract.
- Must be documented in OpenAPI so generated clients use the correct key.
- Useful when integrating with external schemas that mandate specific field names.
- Inconsistent use across DTOs confuses clients and code generators.

---

#### Q15. What is `AddJsonOptions` used for?

**Answer:** `AddJsonOptions` configures `System.Text.Json` serializer settings globally for all JSON input and output formatters in the application. It is chained on `AddControllers()` in `Program.cs`.

- Set naming policy: `options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase`.
- Enable case-insensitive deserialization: `PropertyNameCaseInsensitive = true`.
- Register custom converters: `options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())`.
- Control reference handling, default ignore conditions, and number handling.
- Changes apply application-wide — all controllers share the same JSON configuration.

---

#### Q16. When would you register a custom output formatter?

**Answer:** Register a custom output formatter when the API must produce a media type not supported by built-in formatters — CSV exports, PDF reports, Protocol Buffers, or proprietary binary formats. Implement `OutputFormatter`, register it in MVC options, and constrain endpoints with `[Produces]`.

- Implement `CanWriteType` to match only the intended CLR types.
- Override `WriteResponseBodyAsync` to serialize the object to the response stream.
- Register: `builder.Services.AddControllers(o => o.OutputFormatters.Add(new CsvOutputFormatter()))`.
- Pin endpoints with `[Produces("text/csv")]` to prevent formatter conflicts.
- Test with appropriate Accept headers to verify selection and output correctness.

---

#### Q17. What is the default response format for ASP.NET Core Web API?

**Answer:** The default response format is JSON (`application/json`) serialized by System.Text.Json with camelCase property naming. No additional configuration is needed in the ASP.NET Core 8 Web API project template.

- All `Ok(dto)`, `CreatedAtAction(...)`, and implicit `ActionResult<T>` returns serialize as JSON.
- Content-Type response header is set to `application/json; charset=utf-8`.
- XML, CSV, and other formats require explicit formatter registration and `[Produces]` attributes.
- Clients omitting the Accept header receive JSON by default.
- The default applies to both minimal APIs and controller-based APIs.

---

#### Q18. What is the difference between serialization and model binding?

**Answer:** Serialization converts a CLR object to an outbound byte stream (response body) using an output formatter; model binding is the inbound process of populating action parameters from the request — route values, query strings, headers, and body deserialization via input formatters.

- Serialization: object → JSON string → HTTP response (after action executes).
- Model binding: HTTP request → deserialized object → action parameter (before action executes).
- Body deserialization is one step within model binding, performed by input formatters.
- Validation runs after model binding completes, before the action method body executes.
- Failures differ: binding failure → 400/415; serialization failure → 500 at result execution time.

---

---

## Chapter 05. Model Binding & Validation

#### Q1. What is model binding in ASP.NET Core Web API?

**Answer:** Model binding is the framework mechanism that maps incoming HTTP request data — route values, query strings, headers, form fields, and JSON bodies — to action method parameters and complex types. In ASP.NET Core 8 Web API, binding runs after routing selects an endpoint and before validation and the action execute.

- The appropriate `IModelBinder` is chosen based on parameter type, HTTP verb, and binding source attributes such as `[FromBody]` and `[FromQuery]`.
- Binding failures add entries to `ModelState` with conversion or source errors keyed by property name.
- `[ApiController]` applies opinionated binding conventions — for example, inferring `[FromBody]` for complex types on POST/PUT/PATCH actions.
- Input formatters (typically `System.Text.Json`) deserialize JSON bodies; simple types bind from route and query strings as strings converted to CLR types.

---

#### Q2. What does `[FromBody]` do?

**Answer:** `[FromBody]` tells model binding to deserialize the HTTP request body into the parameter, typically from JSON via `System.Text.Json`. ASP.NET Core 8 allows one `[FromBody]` parameter per action by default because the body is a single stream read once.

- Used on POST, PUT, and PATCH actions for create/update DTOs such as `CreateOrderRequest`.
- The JSON input formatter reads the body stream and maps properties to the target type using the configured naming policy (camelCase by default).
- Malformed JSON or type mismatches fail during formatting and return 400 before the action runs.
- `[ApiController]` infers `[FromBody]` for complex types on actions without an explicit attribute when no other binding source applies.

---

#### Q3. What does `[FromQuery]` do?

**Answer:** `[FromQuery]` binds a parameter from the URL query string — the key-value pairs after `?` in the request URI. Simple types and complex filter objects both bind from query keys matching property names.

- Example: `GET /api/products?page=2&category=books` binds `page` and `category` to action parameters or a filter DTO.
- Complex types use prefix notation — `sort.field=createdAt&sort.descending=true` binds nested properties.
- `[ApiController]` infers `[FromQuery]` for simple types (int, string, bool) on GET actions automatically.
- Query binding is the correct source for search, filter, and pagination parameters on GET endpoints.

---

#### Q4. What does `[FromRoute]` do?

**Answer:** `[FromRoute]` binds a parameter from values captured by the route template in the URI path. Route parameters defined in `[Route]` or `[HttpGet("{id}")]` map to method parameters marked or inferred as route-bound.

- Example: `GET /api/orders/42` with template `{id:int}` binds `id = 42` to an `int id` parameter.
- Route constraints such as `{id:int}` reject invalid values at routing time with 404 before the action executes.
- `[ApiController]` infers `[FromRoute]` for action parameters whose names match route template tokens.
- Explicit `[FromRoute]` is required when the parameter name differs from the route token or ambiguity exists.

---

#### Q5. What is the difference between `[FromBody]` and `[FromQuery]`?

**Answer:** `[FromBody]` reads the request body (typically JSON) and `[FromQuery]` reads URL query string parameters. They represent different parts of the HTTP request and suit different HTTP methods and payload sizes.

- `[FromBody]` is for request payloads on POST, PUT, and PATCH — one body stream per request.
- `[FromQuery]` is for filters, pagination, and simple parameters on GET — visible in the URL and cacheable.
- Using `[FromBody]` on GET is unreliable because clients and proxies often ignore GET bodies.
- Binding the same business field from both sources creates ambiguous contracts — choose one authoritative source per field.

---

#### Q6. How does `[ApiController]` affect automatic model validation?

**Answer:** `[ApiController]` enables automatic model validation that short-circuits to HTTP 400 with `ValidationProblemDetails` when `ModelState` is invalid, without manual `if (!ModelState.IsValid)` checks. It also applies binding source inference for route, query, and body parameters.

- DataAnnotations, `IValidatableObject`, and FluentValidation all run after binding and populate `ModelState` on failure.
- Invalid models never reach the action method body — a filter returns 400 before execution.
- Binding source inference reduces boilerplate `[FromQuery]` and `[FromBody]` attributes on conventional API actions.
- Customize via `ConfigureApiBehaviorOptions` — for example, `InvalidModelStateResponseFactory` for custom error shapes.

---

#### Q7. What HTTP status code does automatic validation failure return?

**Answer:** `[ApiController]` returns **400 Bad Request** when automatic model validation fails. The response body is `ValidationProblemDetails` serialized as `application/problem+json`.

- The `errors` dictionary maps property names to arrays of validation messages for client-side form feedback.
- Malformed JSON also returns 400, but with a different problem type than field-level validation failures.
- Binding conversion failures on route/query parameters may produce 400 or 404 depending on route constraints.
- Consistent 400 responses across endpoints let SPAs and generated SDKs parse errors uniformly.

---

#### Q8. What is `ValidationProblemDetails`?

**Answer:** `ValidationProblemDetails` extends RFC 7807 `ProblemDetails` with an `errors` dictionary for field-level validation failures. ASP.NET Core 8 uses it as the default 400 response body for invalid models on `[ApiController]` actions.

- Standard fields include `type`, `title`, `status` (400), `detail`, and `instance`.
- The `errors` object maps property names (camelCase in JSON) to string arrays of messages — e.g., `"email": ["The Email field is required."]`.
- Content type is `application/problem+json`, aligning Web API error contracts with RFC 7807.
- Clients parse `errors` for per-field UI feedback instead of custom `{ message: "..." }` wrappers.

---

#### Q9. What are data annotations for validation?

**Answer:** Data annotations are declarative attributes on model properties — such as `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, and `[RegularExpression]` — evaluated during model validation after binding succeeds.

- `[Required]` ensures a value was supplied — essential for non-nullable reference types when nullable reference types are enabled.
- `[Range(1, 100)]` and `[StringLength(200)]` enforce numeric bounds and string length limits.
- Annotations are property-scoped — cross-field rules require `IValidatableObject` or FluentValidation.
- Validation runs automatically on `[ApiController]` actions without explicit validation calls in the action method.

---

#### Q10. What is the difference between `[Required]` and optional properties?

**Answer:** `[Required]` marks a property as mandatory — validation fails if the value is null, empty string, or missing after binding. Optional properties accept null or default values without triggering a required-field error.

- With nullable reference types enabled, non-nullable `string Name` is implicitly required; nullable `string? Nickname` is optional.
- Value types like `int` are never null — use `int?` for optional numeric fields or provide a default.
- Optional properties omitted from JSON bind as `null` (reference types) or `default` (value types) — not as validation failures unless constrained.
- `[Required]` validates after binding — a missing JSON property and an explicit `null` both fail required checks on non-nullable types.

---

#### Q11. What is `IValidatableObject`?

**Answer:** `IValidatableObject` is an interface implemented on a model class to perform cross-property validation in a `Validate(ValidationContext)` method. It runs during the same validation pass as DataAnnotations and adds errors to `ModelState`.

- Use when one property's validity depends on another — for example, `EndDate >= StartDate` or `GuestCount <= RoomCapacity`.
- Return `ValidationResult` instances with optional member names to attach errors to specific properties.
- Errors appear in `ValidationProblemDetails.errors` identically to `[Required]` failures.
- Suitable for a few cross-field rules — larger rule sets belong in FluentValidation validators.

---

#### Q12. What is FluentValidation and how does it integrate with Web APIs?

**Answer:** FluentValidation defines validation rules in separate `AbstractValidator<T>` classes using a fluent API instead of attributes on DTOs. Register it with `AddFluentValidationAutoValidation()` to integrate with ASP.NET Core 8 model validation and `ModelState`.

- Validators live in dedicated classes — DTOs stay clean without validation attributes.
- Supports conditional logic, async rules, and reusable rule sets more comfortably than DataAnnotations alone.
- Errors merge into `ModelState` and return as `ValidationProblemDetails` when `[ApiController]` is enabled.
- Do not duplicate the same rule in both DataAnnotations and FluentValidation on the same DTO.

---

#### Q13. Why can't GET requests reliably use `[FromBody]`?

**Answer:** HTTP does not forbid a body on GET, but browsers, HTTP clients, proxies, and caches commonly ignore or strip GET bodies. ASP.NET Core 8 does not bind `[FromBody]` on GET by default, so filters sent as JSON in GET requests fail silently.

- Search and filter parameters on GET should use `[FromQuery]` or a complex type bound from the query string.
- Heavy or sensitive filter payloads belong on `POST /search` with `[FromBody]` when the payload is too large for URLs.
- GET with a body breaks HTTP caching semantics — intermediaries may not forward the body to the origin server.
- Integration tests against localhost may mask the issue that production clients and CDNs drop GET bodies.

---

#### Q14. What is complex type binding from query strings?

**Answer:** Complex types bind from query strings using prefix notation — property names become query keys such as `filter.category=books&filter.minPrice=10` for a nested filter object. ASP.NET Core 8 binds public settable properties case-insensitively for `[FromQuery]` types.

- Nested objects use dot notation — `sort.field=createdAt&sort.descending=true`.
- Some frontends send bracket notation (`sort[field]`) which does not bind by default without custom model binders.
- Deep nesting in query strings is fragile — flatten to `sortField` and `sortDescending` for public APIs.
- `[ApiController]` applies `[FromQuery]` inference for complex types on GET when no other source is specified.

---

#### Q15. What is the difference between model binding and validation?

**Answer:** Model binding maps raw request data to CLR types and populates action parameters. Validation checks whether bound values satisfy business rules and constraints after binding succeeds.

- Binding errors occur when data cannot be converted — invalid JSON syntax, type conversion failure, or wrong binding source.
- Validation errors occur when binding succeeds but DataAnnotations, `IValidatableObject`, or FluentValidation reject the values.
- Both contribute to `ModelState`, but binding failures on unbound properties may prevent validation from running on those properties.
- Route constraints push some binding failures to 404 before `ModelState` is populated — for example, non-integer `{id}` values.

---

#### Q16. What does `[ValidateNever]` do?

**Answer:** `[ValidateNever]` excludes a property from the validation pipeline — validators such as `[Required]` do not run on that property or its children. The property still binds if present in the request.

- Apply to EF Core navigation properties accidentally exposed on API DTOs to prevent validating entire object graphs.
- Prevents over-validation of nested graphs on PATCH endpoints that bind partial updates.
- Useful when a property is populated server-side and should not be validated from inbound client data.
- Use deliberately — excluding sensitive fields from validation can allow invalid data through if binding still occurs.

---

#### Q17. What is PATCH semantics for partial updates?

**Answer:** PATCH applies partial updates — only fields present in the request should change; omitted fields remain unchanged. This requires nullable types (`bool?`, `string?`, `DateOnly?`) or dedicated patch DTOs to distinguish "omit (unchanged)" from "explicit null or false (set value)".

- Non-nullable `bool` cannot represent three states — omitted JSON deserializes as `default(false)`, conflating "unchanged" and "explicit false".
- Service logic applies updates only when patch properties `HasValue` or are explicitly present in the payload.
- `JsonPatchDocument<T>` (RFC 6902 JSON Patch) provides operation-based partial updates as an alternative to nullable DTO fields.
- OpenAPI should mark patch properties as `nullable: true` so generated clients send omission correctly.

---

#### Q18. How does camelCase JSON map to PascalCase C# properties?

**Answer:** ASP.NET Core 8 Web API defaults to `JsonNamingPolicy.CamelCase` in `System.Text.Json` — a C# property `CustomerName` serializes as `"customerName"` and expects the same key on deserialization. Property name matching is case-sensitive by default.

- Outbound responses use camelCase keys; inbound JSON must use camelCase unless configured otherwise.
- PascalCase JSON keys from legacy clients do not bind unless `PropertyNameCaseInsensitive = true` is set in `AddJsonOptions`.
- `[JsonPropertyName("custom_key")]` overrides naming for individual properties — document exceptions in OpenAPI.
- Silent data loss occurs when clients send PascalCase keys — properties bind as default values (null, 0, false) without validation errors unless `[Required]` catches empties.

---

## Chapter 06. Swagger & OpenAPI

#### Q1. What is OpenAPI?

**Answer:** OpenAPI (formerly Swagger Specification) is a machine-readable standard for describing REST HTTP APIs — endpoints, parameters, request bodies, response schemas, authentication, and metadata. OpenAPI 3.x documents are JSON or YAML files consumed by tools for documentation, client generation, and testing.

- Defines paths, operations, components (schemas), security schemes, and tags in a structured format.
- Enables code generation of TypeScript, C#, Java, and other client SDKs from a single source of truth.
- ASP.NET Core 8 can produce OpenAPI documents via Swashbuckle or the built-in `Microsoft.AspNetCore.OpenApi` package.
- The spec describes the contract — it does not execute or validate requests at runtime.

---

#### Q2. What is Swagger in the context of ASP.NET Core?

**Answer:** In ASP.NET Core, "Swagger" commonly refers to the OpenAPI document generation and Swagger UI tooling integrated via Swashbuckle.AspNetCore or the built-in OpenAPI support. It auto-discovers endpoints and produces interactive API documentation.

- `AddSwaggerGen()` configures Swashbuckle to generate an OpenAPI document from ApiExplorer metadata.
- `UseSwagger()` serves the JSON document; `UseSwaggerUI()` serves the interactive browser UI.
- .NET 8 also offers `AddOpenApi()` and `MapOpenApi()` as a lighter built-in alternative to Swashbuckle.
- Swagger UI lets developers explore and test endpoints without writing separate documentation.

---

#### Q3. What is the difference between OpenAPI and Swagger UI?

**Answer:** OpenAPI is the specification format — the `swagger.json` or `openapi.yaml` document describing the API contract. Swagger UI is a browser-based interactive tool that renders that document for exploration and testing.

- The OpenAPI document is machine-readable — consumed by code generators, gateways, and API management platforms.
- Swagger UI is human-facing — displays endpoints, schemas, and a "Try it out" feature for sending requests.
- You can serve the OpenAPI JSON without Swagger UI — for example, only exposing the document to CI pipelines.
- Swagger UI depends on a valid OpenAPI document — an empty or malformed document produces an empty UI.

---

#### Q4. What does `AddEndpointsApiExplorer` do?

**Answer:** `AddEndpointsApiExplorer` registers the endpoint metadata explorer service that discovers minimal API routes and controller actions for OpenAPI generation. It implements `IApiDescriptionGroupCollectionProvider` consumed by Swashbuckle and the built-in OpenAPI generator.

- Required alongside `AddSwaggerGen()` or `AddOpenApi()` for minimal API endpoint discovery.
- Controller-based projects also benefit — it complements `AddMvcCore().AddApiExplorer()` for full ApiExplorer coverage.
- Without it, minimal API endpoints may be absent from the generated OpenAPI document.
- Call it during service registration: `builder.Services.AddEndpointsApiExplorer();`.

---

#### Q5. What does `AddSwaggerGen` do?

**Answer:** `AddSwaggerGen` registers Swashbuckle services that build an OpenAPI document at runtime from ApiExplorer endpoint metadata, action parameters, return types, and serializer schema generators. Configuration callbacks customize documents, schemas, security, and XML comments.

- Registers `ISwaggerProvider` that produces `OpenApiDocument` objects per registered document name (e.g., `"v1"`).
- Schema generation reflects DTO property types, nullability, and data annotation constraints.
- Customization hooks include `CustomSchemaIds`, `IncludeXmlComments`, `DocInclusionPredicate`, and security definitions.
- The document is generated on first request to `/swagger/{docName}/swagger.json` and cached thereafter.

---

#### Q6. What is a Swagger document (`swagger.json`)?

**Answer:** A Swagger document is the serialized OpenAPI specification — typically `swagger.json` — listing all API paths, HTTP methods, parameters, request/response schemas, and security requirements. It is the machine-readable contract exported by Swashbuckle or `MapOpenApi()`.

- Served at `/swagger/v1/swagger.json` by default when using Swashbuckle with document name `"v1"`.
- Consumed by Swagger UI, NSwag, OpenAPI Generator, and API management platforms for SDK generation.
- Multiple documents can coexist for API versioning — separate `v1` and `v2` JSON files.
- The document reflects what ApiExplorer discovers — undocumented endpoints or missing attributes produce incomplete schemas.

---

#### Q7. What is Swashbuckle?

**Answer:** Swashbuckle.AspNetCore is the popular NuGet package that integrates Swagger/OpenAPI document generation and Swagger UI into ASP.NET Core applications. It bridges ApiExplorer metadata to OpenAPI 3.x documents with extensive customization options.

- Provides `AddSwaggerGen`, `UseSwagger`, and `UseSwaggerUI` extension methods.
- Generates schemas from .NET types using System.Text.Json or Newtonsoft.Json schema generators.
- Supports XML comment integration, custom schema filters, security definitions, and multi-document versioning.
- Alternative: .NET 8's built-in `Microsoft.AspNetCore.OpenApi` for document generation without the full Swashbuckle UI stack.

---

#### Q8. How does Swagger discover API endpoints?

**Answer:** Swagger discovers endpoints through the ASP.NET Core ApiExplorer infrastructure — `IApiDescriptionGroupCollectionProvider` collects metadata from controller actions and minimal API routes registered during application startup.

- Controller actions are discovered via `[Route]`, `[HttpGet]`, and related attributes plus parameter and return type metadata.
- Minimal API routes require `AddEndpointsApiExplorer()` and benefit from `.WithOpenApi()` for rich metadata.
- `[ApiExplorerSettings(IgnoreApi = true)]` excludes endpoints from the document.
- Discovery is reflection-based at startup — it does not execute controllers or inspect runtime behavior.

---

#### Q9. What is a schema in OpenAPI?

**Answer:** An OpenAPI schema describes the structure of a data type — properties, types, formats, nullability, required fields, and constraints. Schemas appear in request bodies, response payloads, and parameter definitions within the `components.schemas` section.

- Generated from .NET DTO types — `string`, `int`, `DateTime`, nested objects, arrays, and enums.
- Nullable reference types and `bool?` affect `nullable: true` in OpenAPI 3 schemas when NRT is enabled.
- Schema `$ref` pointers reference shared component schemas to avoid duplication across operations.
- Schema ID collisions occur when two types share the same short name — resolved with `CustomSchemaIds`.

---

#### Q10. What does `[ProducesResponseType]` contribute to OpenAPI?

**Answer:** `[ProducesResponseType]` adds response metadata to ApiExplorer — HTTP status code, response type, and content type — which Swashbuckle maps to OpenAPI response definitions with accurate schemas.

- Example: `[ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]` documents the 200 response schema.
- Multiple attributes document different status codes — 200, 400, 404, 409 — on the same action.
- Without it, Swashbuckle may infer return types incompletely — especially for `IActionResult` without generic type info.
- Improves generated client SDKs by documenting error response shapes alongside success responses.

---

#### Q11. What is the purpose of Swagger UI?

**Answer:** Swagger UI is an interactive browser interface that renders the OpenAPI document — listing endpoints, schemas, and providing a "Try it out" feature to send test requests. It accelerates development, QA, and partner integration without separate API documentation.

- Displays request parameters, body schemas, and response examples per operation.
- Supports authentication flows — Bearer token entry or OAuth2 authorization code with PKCE.
- Useful in Development and Staging — should be restricted or disabled in Production to avoid exposing the full API surface.
- Multiple documents appear as a dropdown when several SwaggerDoc versions are registered.

---

#### Q12. What is a security scheme in OpenAPI?

**Answer:** A security scheme defines how clients authenticate to the API — Bearer JWT, API key, OAuth2 flows, or basic auth. Defined in `components.securitySchemes` and referenced globally or per-operation in the OpenAPI document.

- Bearer scheme: `AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer" })`.
- Global `AddSecurityRequirement` applies authentication to all operations unless overridden.
- Swagger UI renders an "Authorize" button based on registered security definitions.
- Security schemes describe authentication method — they do not store or validate actual credentials.

---

#### Q13. What is schema ID collision in Swagger generation?

**Answer:** Schema ID collision occurs when Swashbuckle generates the same schema identifier for two different .NET types with the same short name — for example, `ProductDto` in two namespaces. One schema overwrites the other, producing wrong properties in the OpenAPI document and broken client generation.

- Default schema IDs use the short type name without namespace.
- Fix with `options.CustomSchemaIds(type => type.FullName?.Replace("+", "."))` for unique IDs.
- Common when internal and public DTOs share names or when versioning introduces parallel types.
- Collisions are silent until client SDK regeneration fails or returns wrong models.

---

#### Q14. What does `IncludeXmlComments` do?

**Answer:** `IncludeXmlComments` configures Swashbuckle to read XML documentation files generated from `///` summary comments and attach them to OpenAPI operation descriptions, parameter docs, and property descriptions.

- Requires `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the `.csproj` to emit the `.xml` file.
- Call `c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "MyApi.xml"))` in `AddSwaggerGen`.
- Enriches the OpenAPI document with human-readable descriptions beyond what reflection provides.
- Multiple XML files can be included for controllers, models, and shared contract assemblies.

---

#### Q15. What is the difference between documenting controllers vs minimal APIs in Swagger?

**Answer:** Controller-based APIs inherit conventions from `[Route]`, `[HttpGet]`, parameter attributes, and `[ProducesResponseType]` — Swashbuckle discovers metadata automatically. Minimal APIs require explicit metadata via `.WithOpenApi()`, `[AsParameters]`, `[FromBody]`, and `.Produces<T>()` because lambda signatures lack convention-based inference.

- Controllers: `[ApiController]` + attribute routing provides rich default metadata with minimal extra configuration.
- Minimal APIs: `app.MapGet(...).WithOpenApi()` adds summaries, tags, and response types; complex parameters need `[AsParameters]` or explicit binding attributes.
- Both require `AddEndpointsApiExplorer()` for minimal API discovery.
- Minimal API lambda return types may appear as untyped or generic schemas without explicit `.Produces<T>(200)` metadata.

---

#### Q16. Why should Swagger UI be restricted in production?

**Answer:** Public Swagger UI exposes the full API surface — every endpoint, parameter, schema, and authentication scheme — to anyone who can reach the URL. This aids reconnaissance and targeted attacks against misconfigured or undocumented endpoints.

- Gate behind environment checks: `if (app.Environment.IsDevelopment()) { app.UseSwaggerUI(); }`.
- Production alternatives: internal VPN docs portal, IP-restricted access via reverse proxy, or auth-protected `/swagger` route.
- Even with `[Authorize]` on endpoints, Swagger documents their existence and parameter shapes.
- OpenAPI JSON can be served to CI/CD pipelines without exposing the interactive UI publicly.

---

#### Q17. What is `DocInclusionPredicate` in Swagger?

**Answer:** `DocInclusionPredicate` is a Swashbuckle filter function `(docName, apiDescription) => bool` that determines which API descriptions appear in which OpenAPI document. Essential for multi-version APIs where v1 and v2 operations must land in separate Swagger documents.

- Example: `(docName, apiDesc) => apiDesc.GroupName == docName` maps ApiExplorer group names to document names.
- Used with `Asp.Versioning.Mvc.ApiExplorer` where each API version produces a distinct group name (e.g., `"v1"`, `"v2"`).
- Without a predicate, all actions appear in every registered document or only in the default document.
- Pair with separate `SwaggerDoc("v1", ...)` and `SwaggerDoc("v2", ...)` registrations and matching Swagger UI endpoints.

---

#### Q18. What is the relationship between DTOs and OpenAPI schemas?

**Answer:** OpenAPI schemas are generated from the .NET types used in action parameters and return types — typically response and request DTOs. The DTO defines the public API contract; the OpenAPI schema is the machine-readable representation of that contract.

- Exposing EF entities generates schemas reflecting database columns — internal fields, navigation properties, and circular references leak into the document.
- Response DTOs produce stable, intentional schemas with only approved fields — map entities to DTOs before returning.
- `[ProducesResponseType(typeof(OrderResponseDto), 200)]` ensures the documented schema matches the actual response shape.
- DTO renames and removals are breaking API changes reflected in schema diffs — version DTOs per API version to keep schemas accurate.

---

## Chapter 07. API Versioning

#### Q1. What is API versioning?

**Answer:** API versioning is the practice of managing multiple concurrent versions of an API contract so existing clients continue working while new features and breaking changes ship in later versions. ASP.NET Core 8 supports versioning via the `Asp.Versioning.Mvc` package with URL, header, query, and media type readers.

- Clients specify a version explicitly or rely on a configured default version.
- Each version can have distinct controllers, actions, DTOs, and OpenAPI documents.
- Versioning decouples client migration timelines from server deployment schedules.
- Without versioning, breaking JSON field changes force all clients to update simultaneously.

---

#### Q2. What is URL path versioning?

**Answer:** URL path versioning embeds the version in the URI path — for example, `/api/v1/products` and `/api/v2/products`. It is the most visible and gateway-friendly strategy, making version identification obvious in logs, proxies, and browser address bars.

- Route template: `[Route("api/v{version:apiVersion}/[controller]")]` with `[ApiVersion("1.0")]` on controllers.
- Easy to configure in reverse proxies, API gateways, and CDN cache rules by path prefix.
- URL churn when versioning — clients must update base URLs for major version changes.
- Most common strategy for public REST APIs with long-lived client integrations.

---

#### Q3. What is header-based API versioning?

**Answer:** Header-based versioning sends the API version in an HTTP request header — for example, `X-Api-Version: 2.0` or `api-version: 2.0`. URLs remain clean and identical across versions while the server selects the version from the header value.

- Configure with `options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version")`.
- Invisible in URLs — harder to test in a browser without tools like curl or Postman.
- CDNs and proxies must forward the version header to the origin server.
- Requires unambiguous controller/action mapping — duplicate routes without `[MapToApiVersion]` cause wrong version selection.

---

#### Q4. What is query string API versioning?

**Answer:** Query string versioning passes the version as a URL parameter — for example, `GET /api/products?api-version=2.0`. It is easy to add to existing APIs without changing route templates but interacts poorly with CDN caching.

- Configure with `options.ApiVersionReader = new QueryStringApiVersionReader("api-version")`.
- Clients can omit the parameter if `AssumeDefaultVersionWhenUnspecified` is enabled.
- CDNs that cache by path only may serve the wrong version body when query strings differ but share the same cache key.
- Easy for clients to forget the parameter — accidental default version usage causes silent behavior changes.

---

#### Q5. What is media type (Accept header) API versioning?

**Answer:** Media type versioning embeds the version in the `Accept` header using a vendor-specific media type — for example, `Accept: application/vnd.myapi.v2+json`. The server content-negotiates the version from the Accept header value.

- Configure with `options.ApiVersionReader = new MediaTypeApiVersionReader()`.
- Pure REST approach — version is part of the representation format, not the URL.
- Complex for clients and poorly supported by browser tools and some HTTP libraries.
- Requires custom media type registration and formatter configuration alongside versioning middleware.

---

#### Q6. What is the difference between breaking and non-breaking API changes?

**Answer:** Breaking changes alter the contract in ways that cause existing clients to fail — removing fields, renaming properties, changing types, or altering HTTP semantics. Non-breaking (additive) changes extend the contract without affecting clients that ignore new elements.

- Breaking: remove `name`, rename to `fullName` only, change `id` from int to string, change response status codes.
- Additive: add optional `fullName` alongside existing `name`, add new optional query parameters, add new endpoints.
- Breaking changes require a new major API version; additive changes are safe within the current version.
- JSON clients typically ignore unknown properties — adding fields is safe; removing or renaming is not.

---

#### Q7. What does `DefaultApiVersion` configure?

**Answer:** `DefaultApiVersion` sets the API version applied when a client does not specify a version and `AssumeDefaultVersionWhenUnspecified` is true. It defines which version unversioned requests route to — typically the oldest supported version during migration.

- Example: `options.DefaultApiVersion = new ApiVersion(1, 0)` routes unspecified requests to v1.
- Should remain on the stable, widely deployed version until a published sunset date passes.
- Setting default to the latest major version silently upgrades clients that omit version info — a common production incident source.
- Works with all version readers — URL, header, query, and media type.

---

#### Q8. What does `AssumeDefaultVersionWhenUnspecified` do?

**Answer:** When `true`, requests that do not include a version identifier are routed to `DefaultApiVersion`. When `false`, unversioned requests do not match versioned routes and typically return 404.

- `true`: `/api/products` (without version segment) maps to v1 if default is 1.0 — convenient for legacy clients.
- `false`: clients must explicitly include the version — safer for public APIs where silent defaults cause accidental breaking upgrades.
- The choice is contractual — document whether omitting the version is supported and which version applies.
- Changing this setting on deploy can shift behavior for all clients that omit version information.

---

#### Q9. What is the `[ApiVersion]` attribute?

**Answer:** `[ApiVersion]` declares which API version(s) a controller or action supports — for example, `[ApiVersion("1.0")]` or `[ApiVersion("2.0")]`. Combined with `[MapToApiVersion]`, it maps specific actions to specific versions on shared controllers.

- A controller can declare multiple versions: `[ApiVersion("1.0")] [ApiVersion("2.0")]`.
- `[MapToApiVersion("2.0")]` on an action restricts it to v2 even when the controller supports both versions.
- Works with `[ApiVersionNeutral]` for endpoints that serve all versions (health checks, metadata).
- The version reader (URL, header, query) selects which version the client requests; `[ApiVersion]` declares what the server offers.

---

#### Q10. What is `ReportApiVersions`?

**Answer:** When `ReportApiVersions` is enabled, ASP.NET Core adds response headers listing supported API versions — typically `api-supported-versions` and `api-deprecated-versions`. Clients discover available versions without consulting external documentation.

- Example response header: `api-supported-versions: 1.0, 2.0`.
- Helps client developers identify which versions are active and which are deprecated.
- Pair with deprecation headers (`Deprecation`, `Sunset`) for migration planning.
- Configure in `AddApiVersioning(options => options.ReportApiVersions = true)`.

---

#### Q11. Why is API versioning needed?

**Answer:** API versioning lets server teams evolve the API — add features, fix design mistakes, and restructure payloads — without forcing all clients to update simultaneously. It provides a migration window where old and new contracts coexist.

- Mobile apps and partner integrations cannot update instantly — they need months to adopt breaking changes.
- Without versioning, any breaking JSON change is a coordinated big-bang release across all consumers.
- Supports deprecation strategies with sunset dates, giving clients time to migrate.
- Enables separate OpenAPI documents and SDK generation per version for accurate client tooling.

---

#### Q12. What are the trade-offs of URL path vs header versioning?

**Answer:** URL path versioning is visible, log-friendly, and gateway-compatible but changes URLs on major upgrades. Header versioning keeps URLs stable but is invisible to caches, harder to test in browsers, and requires proxy header forwarding.

- URL: obvious in access logs, easy CDN cache key separation, simple proxy routing rules — but URL churn for clients.
- Header: clean URLs across versions, same route templates — but hidden contract, CDN cache key issues, poor browser testability.
- URL versioning is preferred for public APIs with diverse clients; header versioning suits internal services with controlled client fleets.
- Query string versioning is a middle ground — easy to add but hostile to CDN caching without explicit cache key configuration.

---

#### Q13. What is a deprecation strategy for old API versions?

**Answer:** A deprecation strategy communicates that an old version will be removed, gives clients a migration deadline, and monitors usage before shutdown. It combines HTTP deprecation headers, OpenAPI `deprecated: true` markers, documentation, and usage metrics.

- Send `Deprecation: true` and `Sunset: Sat, 01 Feb 2027 00:00:00 GMT` headers on deprecated version responses.
- Include `Link: <https://docs.example.com/migration>; rel="successor-version"` pointing to the replacement API.
- Mark operations `deprecated: true` in the v1 OpenAPI document.
- Monitor deprecated endpoint usage — do not remove a version until sunset date passes and usage drops below threshold.

---

#### Q14. What is the Sunset HTTP header?

**Answer:** The `Sunset` HTTP header specifies the date after which an endpoint or API version may be removed — for example, `Sunset: Sat, 01 Feb 2027 00:00:00 GMT`. It gives clients a concrete deadline for migration, aligned with RFC 8594 semantics.

- Pair with `Deprecation: true` to signal the endpoint is deprecated but still functional until the sunset date.
- Clients and API gateways can automate warnings or block requests after the sunset date.
- Include a `Link` header with `rel="successor-version"` pointing to the replacement endpoint or version.
- Log and monitor requests to deprecated endpoints to drive partner outreach before removal.

---

#### Q15. What is an additive vs breaking change in JSON APIs?

**Answer:** Additive changes add new optional elements without altering existing ones — new JSON properties, new endpoints, new optional query parameters. Breaking changes modify or remove existing elements that clients depend on — field renames, type changes, removed endpoints, or altered status codes.

- Additive: add `"fullName"` alongside existing `"name"` — old clients ignore the new field.
- Breaking: rename `"name"` to `"fullName"` only — old clients lose the name field.
- Additive: new `GET /api/v1/products/search` endpoint — existing endpoints unchanged.
- Breaking: change pagination from offset to cursor — clients parsing offset responses break.

---

#### Q16. How does CDN caching interact with query-string versioning?

**Answer:** CDNs cache responses by URL path by default — `GET /api/items?api-version=1.0` and `?api-version=2.0` may share the same cache key if the CDN ignores query strings, returning stale v1 JSON to v2 clients.

- Configure the CDN to include `api-version` in the cache key or disable caching for versioned endpoints.
- URL path versioning avoids this issue — `/api/v1/items` and `/api/v2/items` are distinct cache keys.
- `Cache-Control: public, max-age=300` on versioned responses amplifies cross-version cache pollution.
- Use `Cache-Control: private` or `no-store` for authenticated APIs during migration testing.

---

#### Q17. What is `Asp.Versioning.Mvc`?

**Answer:** `Asp.Versioning.Mvc` is the NuGet package (successor to `Microsoft.AspNetCore.Mvc.Versioning`) that adds API versioning support to ASP.NET Core MVC and Web API controllers. It provides version readers, `[ApiVersion]` attributes, ApiExplorer integration, and Swagger document grouping.

- Register with `builder.Services.AddApiVersioning()` and configure readers, defaults, and reporting.
- Add `.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV")` for Swagger integration.
- Supports URL segment, header, query string, and media type version readers — combinable with `ApiVersionReader.Combine()`.
- Works with controller-based and minimal API projects through the Asp.Versioning.Http package.

---

#### Q18. What is the difference between versioning the URL vs versioning the response schema?

**Answer:** URL versioning changes the endpoint address per major version — `/api/v1/orders` vs `/api/v2/orders` — with potentially different controllers and routes. Schema versioning keeps the same URL but returns different JSON shapes based on the requested version — often via header or Accept negotiation.

- URL versioning: distinct routes, controllers, and OpenAPI documents per version — clearest separation.
- Schema versioning: same route serves multiple response shapes — harder to document and test but avoids URL churn.
- URL versioning is preferred for breaking changes with different endpoint structures.
- Schema versioning suits minor representation changes within a negotiated format — more common with media type versioning.

---

## Chapter 08. CORS

#### Q1. What is CORS?

**Answer:** Cross-Origin Resource Sharing (CORS) is a browser security mechanism that controls whether a web page from one origin (scheme + host + port) can access resources from a different origin via JavaScript. ASP.NET Core implements CORS through middleware and policy configuration.

- CORS is enforced by browsers only — it does not affect server-to-server calls, curl, or Postman.
- The server responds with CORS headers (`Access-Control-Allow-Origin`, etc.) telling the browser whether to expose the response to JavaScript.
- ASP.NET Core configures CORS via `AddCors()` service registration and `UseCors()` middleware with named policies.
- CORS is not a substitute for authentication — it controls browser access, not API authorization.

---

#### Q2. Why do browsers enforce CORS for Web APIs?

**Answer:** Browsers enforce the Same-Origin Policy to prevent malicious websites from reading responses from other origins using the user's credentials. CORS provides a controlled exception — the server explicitly permits specific origins to access its resources via JavaScript.

- Without CORS, any website could call your API from the user's browser and read sensitive response data.
- CORS headers are the server's way of opting in to cross-origin browser access.
- Same-origin requests (SPA and API on the same host/port) do not trigger CORS checks.
- CORS protects users browsing the web — it does not protect the API from direct non-browser access.

---

#### Q3. What is a cross-origin request?

**Answer:** A cross-origin request occurs when the JavaScript origin (scheme, host, and port) of the web page differs from the origin of the API being called. For example, a SPA at `https://app.example.com` calling an API at `https://api.example.com` is cross-origin.

- `https://app.example.com:443` vs `https://api.example.com:443` — different host, cross-origin.
- `http://localhost:3000` vs `http://localhost:5000` — different port, cross-origin.
- `https://example.com` vs `https://example.com` — same origin, no CORS check.
- The browser sends an `Origin` header on cross-origin requests; the server must echo it in `Access-Control-Allow-Origin`.

---

#### Q4. What is a CORS preflight request?

**Answer:** A CORS preflight is an automatic `OPTIONS` request sent by the browser before the actual request when the request is "non-simple" — for example, JSON POST with `Content-Type: application/json` or requests with custom headers like `Authorization`.

- The browser sends `OPTIONS` with `Access-Control-Request-Method` and `Access-Control-Request-Headers`.
- The server must respond with appropriate `Access-Control-Allow-*` headers and a 2xx status without requiring authentication.
- Only after a successful preflight does the browser send the actual GET, POST, PUT, or DELETE request.
- Failed preflights block the actual request — the browser reports a CORS error, not the underlying HTTP status.

---

#### Q5. When does a browser send an OPTIONS preflight?

**Answer:** Browsers send an OPTIONS preflight for non-simple cross-origin requests — those using methods other than GET/HEAD/POST, custom headers beyond the CORS-safelist, or `Content-Type` values other than `application/x-www-form-urlencoded`, `multipart/form-data`, or `text/plain`.

- JSON POST with `Content-Type: application/json` always triggers preflight.
- Requests with `Authorization` header (Bearer JWT) trigger preflight.
- Custom headers like `X-Request-Id` or `X-Api-Key` trigger preflight.
- Simple GET requests without custom headers typically do not preflight.

---

#### Q6. What is the `Access-Control-Allow-Origin` header?

**Answer:** `Access-Control-Allow-Origin` tells the browser which origin is permitted to read the response via JavaScript. The server echoes the requesting origin or a specific allowed origin — never a list of multiple origins in a single header value.

- Example: `Access-Control-Allow-Origin: https://app.example.com`.
- `Access-Control-Allow-Origin: *` allows any origin but cannot be combined with credentials.
- ASP.NET Core's `WithOrigins("https://app.example.com")` sets this header for matching requests.
- The browser blocks JavaScript access to the response if this header is missing or does not match the requesting origin.

---

#### Q7. What is the difference between `AllowAnyOrigin` and `WithOrigins`?

**Answer:** `AllowAnyOrigin()` sets `Access-Control-Allow-Origin: *` for all origins — permissive but incompatible with credentials. `WithOrigins("https://app.example.com")` sets the header to specific allowed origins, required when cookies or credentials are involved.

- `AllowAnyOrigin()` is acceptable for public read-only APIs in development without authentication.
- `WithOrigins()` requires listing each allowed origin explicitly — load from configuration per environment.
- ASP.NET Core does not support wildcard subdomains in `WithOrigins` natively — list each subdomain or implement custom `ICorsPolicyProvider`.
- Production APIs with authenticated users must use explicit origin allowlists.

---

#### Q8. Why can't `AllowAnyOrigin` be used with `AllowCredentials`?

**Answer:** The CORS specification forbids combining `Access-Control-Allow-Origin: *` with `Access-Control-Allow-Credentials: true`. Browsers reject this combination because wildcard origin with credentials would allow any site to access authenticated responses.

- When credentials (cookies, client certificates) are included, the server must echo a specific origin.
- ASP.NET Core throws at startup or the browser blocks the response if both are configured together.
- For Bearer tokens in the `Authorization` header, `AllowCredentials()` may not be needed — but explicit origins are still required if credentials mode is enabled.
- Fix: replace `AllowAnyOrigin()` with `WithOrigins("https://app.example.com").AllowCredentials()`.

---

#### Q9. What does `AllowHeaders` configure?

**Answer:** `AllowHeaders` (or `WithHeaders` / `AllowAnyHeader`) configures which request headers the browser may send on cross-origin requests. The server echoes allowed headers in `Access-Control-Allow-Headers` on preflight responses.

- Must include `Authorization` for JWT Bearer token requests — otherwise preflight fails.
- Must include `Content-Type` for JSON POST requests with `application/json`.
- Custom headers like `X-Request-Id` or `X-Api-Version` must be explicitly allowed or use `AllowAnyHeader()`.
- Missing header permissions cause preflight failure — the browser blocks the actual request before it reaches authentication.

---

#### Q10. What does `WithExposedHeaders` do?

**Answer:** `WithExposedHeaders` configures which response headers JavaScript can read from cross-origin responses via `fetch` or XHR. By default, browsers expose only CORS-safelisted response headers — custom headers require explicit exposure.

- Sets `Access-Control-Expose-Headers` — for example, `Content-Disposition`, `X-Total-Count`, `X-Pagination`.
- Without exposure, JavaScript cannot read pagination totals or download filenames from response headers.
- Example: `.WithExposedHeaders("Content-Disposition", "X-Total-Count")`.
- Alternatively, return metadata in the JSON body to avoid CORS header exposure complexity.

---

#### Q11. What is the correct middleware order for `UseCors` in a Web API?

**Answer:** CORS middleware must run after routing and before authentication and authorization — typically: `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`.

- CORS must execute before auth so preflight OPTIONS requests succeed without authentication.
- CORS headers must be added to error responses (401, 403) — if auth runs before CORS, browsers report CORS errors instead of auth failures.
- Middleware registered after `MapControllers()` does not run for matched endpoints — CORS must precede endpoint mapping.
- Use a named policy: `app.UseCors("DefaultPolicy")` matching the policy registered in `AddCors()`.

---

#### Q12. What is the difference between simple and non-simple CORS requests?

**Answer:** Simple requests skip preflight and proceed directly — GET/HEAD/POST with safelisted headers and Content-Type values (`text/plain`, `application/x-www-form-urlencoded`, `multipart/form-data`). Non-simple requests trigger an OPTIONS preflight before the actual request.

- Simple: `GET /api/products` with no custom headers from a cross-origin SPA — no preflight.
- Non-simple: `POST /api/orders` with `Content-Type: application/json` and `Authorization: Bearer ...` — preflight required.
- Non-simple: PUT, PATCH, DELETE methods always trigger preflight.
- Preflight adds latency — one extra round trip before the actual request executes.

---

#### Q13. Does CORS protect the API server from unauthorized access?

**Answer:** No. CORS is a browser-enforced policy that prevents JavaScript on unauthorized websites from reading API responses. It does not block direct HTTP requests from curl, Postman, server-to-server calls, or malicious scripts running outside a browser context.

- Authentication and authorization middleware protect the API from unauthorized access.
- CORS only controls which browser origins can read responses via JavaScript.
- A public API without auth is accessible to anyone regardless of CORS configuration.
- Treat CORS as a browser UX/security feature, not an API security boundary.

---

#### Q14. What is `Access-Control-Allow-Credentials`?

**Answer:** `Access-Control-Allow-Credentials: true` tells the browser it may include credentials (cookies, HTTP authentication, client certificates) in cross-origin requests and expose the authenticated response to JavaScript. Requires a specific origin in `Access-Control-Allow-Origin`, not a wildcard.

- ASP.NET Core: `.AllowCredentials()` on the CORS policy sets this header.
- The client must also set `credentials: 'include'` in fetch or `withCredentials: true` in XHR.
- Required for cookie-based authentication in cross-origin SPAs.
- Bearer tokens in the `Authorization` header do not require credentials mode unless cookies are also sent.

---

#### Q15. What is the difference between CORS errors and 401 Unauthorized?

**Answer:** A CORS error occurs when the browser blocks JavaScript from reading a response due to missing or incorrect CORS headers — the actual HTTP status may be 200 or 401, but JavaScript cannot see it. A 401 Unauthorized is an authentication failure returned by the server that JavaScript can read if CORS headers are present.

- CORS error in DevTools console: "blocked by CORS policy" — often caused by middleware order or missing CORS on error responses.
- 401 with proper CORS headers: JavaScript can read the status and response body — the client handles re-authentication.
- 401 without CORS headers on a cross-origin request: browser reports a CORS error, masking the real auth failure.
- Fix middleware order first when diagnosing "CORS error on 401" — ensure `UseCors()` runs before `UseAuthentication()`.

---

#### Q16. What does `Access-Control-Allow-Methods` specify?

**Answer:** `Access-Control-Allow-Methods` lists the HTTP methods the browser may use on cross-origin requests. It appears in preflight OPTIONS responses, echoing permitted methods such as GET, POST, PUT, PATCH, and DELETE.

- ASP.NET Core: `.WithMethods("GET", "POST", "PUT", "DELETE")` or `.AllowAnyMethod()`.
- Must include the method used by the actual request — otherwise preflight succeeds but the real request method is blocked.
- Preflight sends `Access-Control-Request-Method: POST`; server responds with `Access-Control-Allow-Methods: POST`.
- Restrict methods in production policies — avoid `AllowAnyMethod()` when only GET and POST are needed.

---

#### Q17. When should CORS be configured at the API vs API gateway?

**Answer:** Configure CORS at the API gateway or reverse proxy for public multi-tenant APIs with centralized origin allowlists across microservices. Configure CORS in the ASP.NET Core app when the app is directly exposed without a gateway, or for development and internal SPAs hitting the app directly.

- Gateway/APIM: central policy, consistent allowlist, preflight caching — one place to manage origins for all backend services.
- ASP.NET Core app: direct public exposure, local development, internal services without an edge gateway.
- Avoid duplicate CORS headers from both gateway and app — browsers reject responses with multiple `Access-Control-Allow-Origin` values.
- Document a single CORS owner in the platform runbook — on-call fixes one layer, not two.

---

#### Q18. What is a CORS policy in ASP.NET Core?

**Answer:** A CORS policy is a named set of rules registered in `AddCors()` defining allowed origins, methods, headers, exposed headers, and credential support. The policy is applied via `UseCors("PolicyName")` middleware or `[EnableCors("PolicyName")]` on controllers and actions.

- Register: `builder.Services.AddCors(options => options.AddPolicy("Default", builder => builder.WithOrigins(...).AllowAnyHeader().AllowAnyMethod()))`.
- Apply globally: `app.UseCors("Default")` in the middleware pipeline.
- Apply per-controller: `[EnableCors("AdminPolicy")]` for different rules on different endpoint groups.
- Load allowed origins from `appsettings.{Environment}.json` via `IOptions` for environment-specific configuration.

---

## Chapter 09. Problem Details & Error Responses

#### Q1. What is RFC 7807 Problem Details?

**Answer:** RFC 7807 defines a standard JSON (or XML) format for HTTP API error responses — a machine-readable object with fields like `type`, `title`, `status`, `detail`, and `instance` so clients can parse errors consistently across endpoints and services.

- It replaces ad hoc shapes like `{ "error": "something went wrong" }` with a predictable contract.
- The `Content-Type` is typically `application/problem+json` (or `application/json` with the same schema).
- Extensions are allowed — ASP.NET Core adds `traceId` and validation uses an `errors` dictionary.
- Problem Details describe the error itself, not the successful resource representation.
- ASP.NET Core 8 maps `ProblemDetails` and `ValidationProblemDetails` classes directly to this standard.

---

#### Q2. What is the `ProblemDetails` class in ASP.NET Core?

**Answer:** `ProblemDetails` is the built-in ASP.NET Core model for RFC 7807 error payloads. Controller helpers like `NotFound()`, `BadRequest()`, and `Results.Problem()` serialize it to JSON with standard fields populated from the HTTP status and your message.

- Properties include `Type`, `Title`, `Status`, `Detail`, and `Instance` (usually the request path).
- `Extensions` is a dictionary for custom fields such as `traceId` without breaking the standard shape.
- Works in MVC controllers (`ControllerBase.Problem()`) and minimal APIs (`Results.Problem()`).
- OpenAPI/Swashbuckle can document `ProblemDetails` as the response schema for 4xx/5xx codes.
- Register `AddProblemDetails()` in .NET 8 to customize defaults and environment-specific behavior globally.

---

#### Q3. What is `ValidationProblemDetails`?

**Answer:** `ValidationProblemDetails` extends `ProblemDetails` with an `Errors` property — an `IDictionary<string, string[]>` mapping field names to one or more validation messages. ASP.NET Core returns it automatically when model validation fails on an `[ApiController]`.

- Inherits all RFC 7807 fields (`title`, `status`, `type`, etc.) plus field-scoped errors.
- Produced by the `ModelStateInvalidFilter` before the action method runs when `ModelState` is invalid.
- Default HTTP status is **400 Bad Request** unless you configure `InvalidModelStateResponseFactory` for 422.
- Keys match model property names (respecting JSON naming policy — camelCase by default).
- Clients iterate `errors["Email"]` to show inline form validation without custom parsing logic.

---

#### Q4. What is the difference between `ProblemDetails` and a custom `{ error: "..." }` object?

**Answer:** `ProblemDetails` follows an industry-standard schema with typed fields, HTTP status alignment, and OpenAPI documentation support. A custom `{ error: "..." }` object forces every client to implement a one-off parser and breaks consistency across endpoints.

- Problem Details include `status`, `title`, `type`, and `instance` — not just a message string.
- Validation errors use a structured `errors` dictionary instead of a flat message array.
- Standard shape works with generated API clients, API gateways, and monitoring tools.
- Custom objects often omit `traceId`/correlation, making support harder in Production.
- Mixing both shapes on one API (some endpoints ProblemDetails, others custom) is a common integration failure.

---

#### Q5. What HTTP status code does `[ApiController]` return for validation failures?

**Answer:** By default, `[ApiController]` returns **400 Bad Request** with a `ValidationProblemDetails` body when model binding or data annotation validation fails. The action method does not execute.

- The `ModelStateInvalidFilter` short-circuits the pipeline when `ModelState.IsValid` is false.
- Response body is JSON with RFC 7807 fields plus an `errors` dictionary keyed by property name.
- Teams can switch to **422 Unprocessable Entity** via `ConfigureApiBehaviorOptions` and `InvalidModelStateResponseFactory`.
- This path is separate from exception middleware — validation is not an unhandled exception.
- Document the chosen status (400 vs 422) consistently in OpenAPI for client generators.

---

#### Q6. What is centralized exception handling for Web APIs?

**Answer:** Centralized exception handling catches unhandled exceptions in one place — middleware or `IExceptionHandler` — maps them to appropriate HTTP status codes, and returns `ProblemDetails` JSON instead of scattering try/catch blocks in every controller.

- Controllers and services throw domain exceptions (`NotFoundException`, `ConflictException`) without HTTP knowledge.
- A single handler converts exception types to 404, 409, 400, or 500 with a uniform JSON body.
- Logging with the full exception object and `TraceIdentifier` happens in the handler, not in controllers.
- Eliminates inconsistent `{ error = ex.Message }` patterns and wrong status codes (e.g., validation as 500).
- Register `AddExceptionHandler<T>()` and `UseExceptionHandler()` early in the pipeline in ASP.NET Core 8.

---

#### Q7. What is `IExceptionHandler` in .NET 8?

**Answer:** `IExceptionHandler` is the .NET 8+ interface for pluggable global exception handling. Implement `TryHandleAsync(HttpContext, Exception, CancellationToken)` — return `true` if your handler wrote the response, `false` to delegate to the next handler.

- Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()`.
- Pair with `app.UseExceptionHandler()` — no custom path required when handlers are registered.
- Multiple handlers can be registered; the first returning `true` wins.
- Replaces much of the custom exception middleware pattern from earlier ASP.NET Core versions.
- Handler writes `ProblemDetails` via `WriteAsJsonAsync` or `IProblemDetailsService`.

---

#### Q8. What should Production error responses exclude?

**Answer:** Production error responses must not expose stack traces, exception types with internal code paths, file paths, connection strings, SQL fragments, or environment-specific configuration. Clients get a safe generic message plus a correlation id for support lookup.

- Log full exception details server-side with `LogError(ex, ...)` including stack and inner exceptions.
- `Detail` should be user-safe — "An unexpected error occurred" for 500, specific but non-sensitive text for 404/409.
- Never return raw `ex.Message` from infrastructure exceptions (database, file system) to external callers.
- Use `IHostEnvironment.IsDevelopment()` in ProblemDetails customization for optional diagnostic extensions locally only.
- Include `traceId` or `requestId` in Production so support can correlate client reports to logs without exposing internals.

---

#### Q9. What is the difference between 400 Bad Request and 404 Not Found for APIs?

**Answer:** **400 Bad Request** means the client sent a syntactically or semantically invalid request — bad JSON, failed validation, or malformed parameters. **404 Not Found** means the request was well-formed but the target resource (or route) does not exist.

- Validation failures and invalid IDs in the wrong format typically return 400 with `ValidationProblemDetails`.
- A valid route with a non-existent entity id (e.g., `GET /api/orders/99999`) returns 404 with `ProblemDetails`.
- Returning 404 for invalid query parameters can mislead clients into thinking the resource type is missing.
- Returning 400 when a resource simply does not exist breaks REST semantics and caching expectations.
- Both should use ProblemDetails JSON on `[ApiController]` APIs for a consistent client experience.

---

#### Q10. When should an API return 409 Conflict?

**Answer:** Return **409 Conflict** when the request is valid but cannot be applied due to a state conflict with the current resource — duplicate unique key, optimistic concurrency failure, or a business rule violation like "order already shipped."

- Duplicate email on registration, conflicting ETag/version on PUT, or "cannot delete active subscription."
- Distinct from 400 (invalid input) and 404 (resource missing) — the resource often exists but the operation conflicts.
- Return `ProblemDetails` with a clear `title` ("Conflict") and safe `detail` explaining the conflict without internal ids.
- Map `DbUpdateConcurrencyException` from EF Core to 409 when using row versioning.
- Document 409 in OpenAPI so clients handle retries or user messaging appropriately.

---

#### Q11. What is the `type` field in ProblemDetails?

**Answer:** The `type` field is a URI reference that identifies the problem category — often a stable URL pointing to documentation about that error type. It helps clients branch logic programmatically without parsing free-text messages.

- Example: `"type": "https://tools.ietf.org/html/rfc7231#section-6.5.1"` for a generic 400, or a company-specific URI like `https://api.example.com/errors/validation`.
- ASP.NET Core sets a default type based on status code when you use built-in helpers.
- Clients should treat it as an identifier, not a URL users must visit.
- Custom exception handlers can set `type` per domain error for machine-readable classification.
- OpenAPI can reference problem types in response documentation for partner integrations.

---

#### Q12. What is the `title` field in ProblemDetails?

**Answer:** The `title` field is a short, human-readable summary of the problem type — independent of the specific occurrence. It should be stable for a given error category and safe to display in UI headers or toast notifications.

- Examples: "Bad Request", "Not Found", "Validation Failed", "Conflict".
- Unlike `detail`, `title` typically does not include entity-specific ids or user input.
- ASP.NET Core populates it from status code defaults or your handler mapping.
- Localization can target `title` for multi-language API consumers.
- Pair with `status` so clients can use either HTTP status or title for display logic.

---

#### Q13. What is the `detail` field in ProblemDetails?

**Answer:** The `detail` field explains this specific occurrence of the problem — what went wrong for this request. It may include safe contextual information such as "Customer with id 42 was not found" but must not leak stack traces or secrets in Production.

- More specific than `title`; varies per request while `title` stays constant for the error class.
- For validation errors, field-level messages live in `errors`; `detail` may summarize ("One or more validation errors occurred").
- Sanitize in Production — generic text for 500, specific but non-sensitive text for 404/409.
- Log the full exception detail server-side even when the response `detail` is generic.
- Avoid echoing raw user input in `detail` to prevent reflected XSS in clients that render error text as HTML.

---

#### Q14. What is the difference between Development and Production error responses?

**Answer:** Development responses may include richer diagnostics — exception type names, stack traces in extension fields, or Developer Exception Page for browser traffic. Production responses return safe, minimal ProblemDetails with correlation ids while full diagnostics go to logs only.

- Both environments should use the same HTTP status code mapping for the same exception type.
- Development: `ProblemDetails.Extensions["stackTrace"]` or similar behind `IsDevelopment()` checks.
- Production: generic 500 `detail`, no file paths, no connection string fragments, no inner exception chains in JSON.
- Use `AddProblemDetails()` customization or `IExceptionHandler` — not `#if DEBUG` scattered in controllers.
- Logging level and content are the same or stricter in Production; only the response body differs.

---

#### Q15. What is `AddProblemDetails()` used for?

**Answer:** `AddProblemDetails()` registers services that configure RFC 7807 Problem Details generation globally in ASP.NET Core 8 — default field values, customization delegates, and integration with exception handling and status code pages.

- Call `builder.Services.AddProblemDetails(options => { ... })` in `Program.cs`.
- Customize `options.CustomizeProblemDetails` to add `traceId`, sanitize `detail`, or add Development-only extensions.
- Works with `IProblemDetailsService` for consistent ProblemDetails creation across middleware and endpoints.
- Complements `AddExceptionHandler<T>()` — the handler can use injected problem details services.
- Ensures minimal APIs and controllers produce the same error shape when using `Results.Problem()`.

---

#### Q16. What is the `errors` dictionary in `ValidationProblemDetails`?

**Answer:** The `errors` property is an `IDictionary<string, string[]>` where each key is a model property or field name and each value is an array of validation error messages for that field. It enables clients to show per-field form errors without parsing a single combined string.

- Keys follow JSON naming policy — camelCase by default (`email`, not `Email`).
- Multiple messages per field are supported (e.g., `[Required]` and `[EmailAddress]` both failing).
- Empty or missing keys mean no error on that field; clients should only highlight keys present in `errors`.
- Global/model-level errors may use an empty string key `""` for errors not tied to one property.
- Stable parsing contract: read `response.errors.fieldName[0]` rather than regex on `detail`.

---

#### Q17. What is the difference between returning `NotFound()` and a custom ProblemDetails for 404?

**Answer:** On an `[ApiController]`, `NotFound()` without arguments returns a ProblemDetails body with status 404 automatically. `NotFound(string message)` returns **text/plain** with the string as body — same status, incompatible JSON contract. Explicit `NotFound(new ProblemDetails { ... })` gives full control over all fields.

- `[ApiController]` convention converts bare `NotFound()` to RFC 7807 JSON.
- Without `[ApiController]`, `NotFound()` behavior differs — often empty body or negotiated content type.
- Custom ProblemDetails lets you set `type`, `detail`, and extensions consistently with your global handler.
- Minimal APIs use `Results.Problem(statusCode: 404, ...)` or `TypedResults.NotFound()` depending on version/conventions.
- Standardize one approach across all controllers to avoid "sometimes JSON, sometimes plain text" support tickets.

---

#### Q18. How do API clients reliably parse validation errors?

**Answer:** Clients should expect HTTP **400** (or team-standard **422**), deserialize the body as `ValidationProblemDetails`, read the top-level `status` and `title`, then iterate the `errors` dictionary mapping property names to message arrays.

- Do not assume a custom `{ error: "..." }` wrapper — check `Content-Type` and ProblemDetails shape.
- Use code-generated clients from OpenAPI where the `ValidationProblemDetails` schema is documented.
- Handle model-level errors under key `""` or a documented global key.
- Fall back gracefully if an endpoint returns non-ProblemDetails 400 from legacy code — but push server teams to unify.
- Include `traceId` from extensions in client error reports for support correlation.

---

## Chapter 10. Authentication & Authorization in APIs

#### Q1. What is JWT Bearer authentication for Web APIs?

**Answer:** JWT Bearer authentication validates JSON Web Tokens sent in the `Authorization: Bearer <token>` header. ASP.NET Core registers the JWT Bearer handler, validates signature, issuer, audience, and lifetime, then builds a `ClaimsPrincipal` on `HttpContext.User`.

- Tokens are self-contained — claims (user id, roles, scopes) travel in the payload without server-side session lookup.
- Configure with `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` and authority/metadata URL or explicit `TokenValidationParameters`.
- Common for mobile apps, SPAs with token endpoints, and microservice-to-service calls through an identity provider.
- Stateless — API scales horizontally without shared session store, but revocation requires short lifetimes or token blocklists.
- Pair with `UseAuthentication()` before `UseAuthorization()` in the middleware pipeline.

---

#### Q2. What is the difference between authentication and authorization in APIs?

**Answer:** **Authentication** establishes who the caller is — validating credentials or tokens and populating `HttpContext.User` with claims. **Authorization** decides whether that authenticated identity is permitted to perform the requested action on the requested resource.

- Authentication runs first via `UseAuthentication()` and authentication handlers (JWT, API key, cookies).
- Authorization runs via `UseAuthorization()` and evaluates `[Authorize]`, policies, and roles against the principal.
- A request can be authenticated but forbidden (403) — valid token, insufficient permission.
- A request can be unauthenticated (401) — missing, expired, or invalid token before authorization is evaluated.
- Web APIs typically use Bearer tokens for authentication and policies/scopes for authorization.

---

#### Q3. What is an API key and when is it used?

**Answer:** An API key is a static secret issued to a client (partner integration, batch job, internal service) that identifies and authenticates the caller. In ASP.NET Core it is usually sent in a header like `X-Api-Key` and validated by a custom `AuthenticationHandler`.

- Used for server-to-server integrations where OAuth browser flows are impractical.
- Simpler than JWT but no built-in expiry/claims unless you embed metadata in your key store.
- Must be transmitted in headers, never query strings, to avoid logging and referrer leakage.
- Rotate keys on compromise; store hashed keys server-side like passwords when possible.
- Often combined with JWT for user-facing mobile apps while batch endpoints use API keys only.

---

#### Q4. What is the difference between Bearer tokens and API keys?

**Answer:** Bearer tokens (typically JWTs) are time-limited, signed credentials carrying claims about the user and client, validated cryptographically against an authority. API keys are opaque static secrets looked up in a store — simpler but without standard claims, expiry, or refresh flows unless you build them.

- JWTs include issuer, audience, expiration, and scopes; validation uses signing keys from metadata.
- API keys require database or cache lookup to map key → tenant/permissions.
- Bearer tokens suit user-delegated OAuth flows; API keys suit long-lived machine integrations.
- Both use the `Authorization` header format differently: `Bearer <jwt>` vs custom `ApiKey` scheme or `X-Api-Key`.
- ASP.NET Core treats them as separate authentication schemes selected by `[Authorize(AuthenticationSchemes = "...")]`.

---

#### Q5. What does `[Authorize]` do on an API controller?

**Answer:** `[Authorize]` on a controller or action requires an authenticated user before the action executes. If `HttpContext.User` is not authenticated, the pipeline returns **401 Unauthorized** (or challenges with `WWW-Authenticate` for Bearer).

- Applies to all actions on the class unless overridden with `[AllowAnonymous]` on specific actions.
- Can specify `Roles = "Admin"` or `Policy = "CanManageOrders"` for finer checks beyond mere authentication.
- Requires `UseAuthentication()` and `UseAuthorization()` in the pipeline — authorization alone is insufficient.
- Without a fallback policy, controllers missing `[Authorize]` remain publicly accessible.
- Works on minimal API endpoints via `.RequireAuthorization()` with the same underlying middleware.

---

#### Q6. What does `[AllowAnonymous]` do?

**Answer:** `[AllowAnonymous]` bypasses authorization for the decorated endpoint even when a class-level `[Authorize]` or a global fallback policy requires authentication. Use it for login, registration, health checks, and webhooks that authenticate via other means.

- Does not disable authentication middleware — it only skips the authorization requirement for that endpoint.
- Webhooks often use `[AllowAnonymous]` plus HMAC signature verification inside the action.
- Must be applied explicitly; it does not inherit in reverse — class `[Authorize]` + action `[AllowAnonymous]` opens that action only.
- Overuse creates accidental public data exposure — audit endpoints without auth regularly.
- Pair with network controls (IP allowlist, API gateway rules) for sensitive anonymous endpoints when possible.

---

#### Q7. What is the difference between role-based and policy-based authorization in APIs?

**Answer:** **Role-based** authorization checks if the user is in a named role via `[Authorize(Roles = "Admin")]`. **Policy-based** authorization uses named policies registered in `AddAuthorization` that can combine roles, claims, scopes, and custom requirements via `IAuthorizationHandler`.

- Roles map directly to role claims in the JWT — simple for coarse admin/user splits.
- Policies compose rules: `RequireRole("Admin").RequireClaim("scope", "orders.write")`.
- Policies scale better for multi-tenant, resource-level, and OAuth scope enforcement.
- Custom policies use `IAuthorizationHandler` for logic that does not fit attributes alone.
- Production APIs favor policies as the central extension point; roles are one input to policies.

---

#### Q8. What are OAuth2 scopes vs role claims?

**Answer:** **Roles** describe who the user is (group membership — Admin, Clerk). **Scopes** describe what the client application is permitted to do on behalf of the user (delegated permissions — `orders.read`, `orders.write`). They are different claims and require separate enforcement.

- Scopes appear in JWT as `scope` (space-delimited) or `scp` (Azure AD) claims.
- Roles appear as `role` or `ClaimTypes.Role` claims.
- A user may have role `OrderClerk` but the client app may lack `orders.write` scope for POST.
- Machine-to-machine clients often have scopes only, no roles — role-only APIs reject valid integration tokens.
- Map scopes to ASP.NET policies with `RequireClaim("scope", "orders.read")` or custom scope-parsing handlers.

---

#### Q9. What is `JwtBearerDefaults.AuthenticationScheme`?

**Answer:** `JwtBearerDefaults.AuthenticationScheme` is the string constant `"Bearer"` — the default scheme name for JWT Bearer authentication in ASP.NET Core. Pass it to `AddAuthentication(...)` as the default scheme and reference it in `[Authorize(AuthenticationSchemes = ...)]`.

- Registers the JWT Bearer handler that reads and validates the `Authorization: Bearer` header.
- When set as default, `[Authorize]` without explicit schemes uses JWT validation automatically.
- Multiple schemes require explicit scheme names on endpoints that should not use the default.
- Challenge responses include `WWW-Authenticate: Bearer` for 401 responses.
- Configuration lives in `AddJwtBearer(options => { options.Authority = ...; })` or manual `TokenValidationParameters`.

---

#### Q10. What HTTP header carries JWT tokens?

**Answer:** JWT access tokens are sent in the **`Authorization`** header with the scheme **`Bearer`**: `Authorization: Bearer eyJhbGciOiJIUzI1NiIs...`. ASP.NET Core JWT Bearer middleware reads this header exclusively — not cookies or query strings for standard API auth.

- The token is the credential — treat it like a password in transit (HTTPS only in Production).
- Never log the full Authorization header — tokens remain valid until expiry and can be replayed.
- Some legacy systems use query or cookie tokens; ASP.NET Core JWT Bearer handler does not read those by default.
- Refresh tokens use separate endpoints and storage — not sent on every API call in the Authorization header.
- API keys may use `X-Api-Key` or a custom Authorization scheme instead of Bearer.

---

#### Q11. What is the difference between 401 Unauthorized and 403 Forbidden?

**Answer:** **401 Unauthorized** means authentication failed or is missing — no valid identity was established. **403 Forbidden** means the caller is authenticated but not permitted to perform this action on this resource.

- 401: missing/expired/invalid token, wrong signing key, failed API key lookup.
- 403: valid token but insufficient role, scope, or resource-based authorization failure.
- 401 often includes `WWW-Authenticate: Bearer` challenge header; 403 typically does not.
- Returning 401 for authorization failures misleads clients into refreshing tokens unnecessarily.
- Returning 403 for invalid tokens misleads clients into thinking permissions are the issue, not credentials.

---

#### Q12. What is `TokenValidationParameters`?

**Answer:** `TokenValidationParameters` configures how the JWT Bearer handler validates incoming tokens — issuer, audience, signing keys, clock skew, and which standard checks to enforce (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, etc.).

- Set explicitly in `AddJwtBearer` or derived automatically from `Authority` OpenID Connect metadata.
- `ValidIssuer` and `ValidAudience` must match token `iss` and `aud` claims or validation fails with 401.
- `ValidateAudience = false` accepts any audience — dangerous in multi-API environments.
- Clock skew allows small time drift between token issuer and API server for `nbf`/`exp` validation.
- Misconfiguration is a common cause of "token works in jwt.io but API returns 401."

---

#### Q13. What is a custom `AuthenticationHandler` for API keys?

**Answer:** A custom `AuthenticationHandler<TOptions>` reads the API key from a request header, validates it against a store, and returns `AuthenticateResult.Success` with a `ClaimsPrincipal` (tenant id, client name) or `AuthenticateResult.Fail` for 401.

- Register with `AddScheme<ApiKeyOptions, ApiKeyHandler>("ApiKey", ...)` alongside JWT Bearer.
- Implement `HandleAuthenticateAsync` — parse header, lookup key, build claims identity.
- Return `AuthenticateResult.NoResult()` when the header is absent so other schemes can try if configured.
- Use `[Authorize(AuthenticationSchemes = "ApiKey")]` on integration endpoints.
- Never read API keys from query strings — header transport only.

---

#### Q14. What is `[Authorize(AuthenticationSchemes = "...")]`?

**Answer:** This attribute specifies which registered authentication schemes must run for the endpoint — e.g., `"Bearer"` for JWT-only or `"ApiKey"` for batch integrations. When omitted, the default scheme from `AddAuthentication(defaultScheme)` applies.

- Multiple schemes: `[Authorize(AuthenticationSchemes = "Bearer,ApiKey")]` — handler selection depends on configuration.
- Prevents JWT handler from challenging API-key-only routes with confusing Bearer errors.
- Each scheme registers via `AddJwtBearer`, `AddScheme`, etc. with a unique name.
- Authentication schemes are separate from authorization policies — both may be required.
- Hybrid APIs (mobile JWT + partner API key) rely on explicit scheme attributes per controller or route group.

---

#### Q15. What is resource-based authorization in APIs?

**Answer:** Resource-based authorization evaluates permissions against the specific resource being accessed — e.g., "does this user own order 123?" — using `IAuthorizationService.AuthorizeAsync(user, order, "EditPolicy")` or an `IAuthorizationHandler` with the resource instance.

- Role checks alone cannot express row-level security (same role, different tenant data).
- Handler compares route id to `tenant_id` or `sub` claim from the JWT.
- Runs after authentication; often invoked inside the action or via a filter after loading the entity.
- `AuthorizationHandler<OperationAuthorizationRequirement, Order>` receives the `Order` resource for evaluation.
- Essential for multi-tenant Web APIs where two users share a role but must not see each other's data.

---

#### Q16. What is multi-tenant authorization for Web APIs?

**Answer:** Multi-tenant authorization ensures each request operates only within the caller's tenant — validating tenant id from JWT claims or API keys against the tenant associated with the requested resource, often via policies and resource handlers rather than roles alone.

- Embed `tenant_id` claim in JWT or map API key to tenant in the authentication handler.
- Policy `BelongsToTenant` compares route/body tenant to claim — Admin role may bypass for support tools.
- Data queries must filter by tenant at the repository/DbContext level — authorization is not enough alone.
- Cross-tenant data leakage is a critical finding — test with tokens from tenant A accessing tenant B ids.
- Global query filters in EF Core (`HasQueryFilter`) complement authorization handlers for defense in depth.

---

#### Q17. How do mobile apps typically authenticate to REST APIs?

**Answer:** Mobile apps commonly use OAuth 2.0 / OpenID Connect — redirect or embedded web view for user login, receive access and refresh tokens, then send the access token as `Authorization: Bearer` on API calls. PKCE is required for public mobile clients without client secrets.

- Access tokens are short-lived JWTs; refresh tokens renew access without re-login.
- Store tokens in secure platform storage (iOS Keychain, Android Keystore) — never plain SharedPreferences.
- Certificate pinning optional for high-security apps to mitigate MITM on token endpoints.
- Biometric unlock gates access to stored tokens locally — not a substitute for server authentication.
- ASP.NET Core API validates JWT via `AddJwtBearer` with authority pointing to Azure AD, Auth0, IdentityServer, etc.

---

#### Q18. What is the difference between cookie auth and Bearer token auth for APIs?

**Answer:** **Cookie authentication** stores the session id in an HttpOnly cookie sent automatically by browsers — suited for same-site server-rendered apps. **Bearer token authentication** sends a token in the Authorization header — suited for SPAs, mobile apps, and cross-origin API clients that do not rely on automatic cookie submission.

- Cookies require CSRF protection for state-changing browser requests; Bearer APIs typically do not use cookies.
- Bearer tokens work cleanly with CORS and non-browser clients; cookies complicate cross-origin SPA setups.
- Cookie auth enables server-side session revocation; JWT Bearer is stateless unless paired with introspection or short lifetimes.
- SPAs often use Bearer tokens from OAuth token endpoint; MVC apps use cookie auth with anti-forgery tokens.
- ASP.NET Core Web APIs default to Bearer/JWT for machine and mobile clients; cookies remain for Blazor Server or hybrid BFF patterns.

---

## Chapter 11. File Upload & Streaming Responses

#### Q1. What is `IFormFile` in ASP.NET Core Web API?

**Answer:** `IFormFile` represents an uploaded file sent as part of a `multipart/form-data` request. It exposes the file name, content type, length, and `OpenReadStream()` for reading bytes without loading the entire file into memory upfront when configured correctly.

- Bind as an action parameter on POST/PUT endpoints accepting multipart form data.
- Use `CopyToAsync` to stream to disk, blob storage, or virus scanner — avoid reading all bytes for large files.
- Validate extension, content type, and size before persisting — never trust client-provided `FileName` for paths.
- Available on `[ApiController]` actions with `[FromForm]` or implicit form binding for file parameters.
- For JSON-only APIs, file upload requires switching to multipart — `IFormFile` does not bind from raw JSON bodies.

---

#### Q2. What is `multipart/form-data`?

**Answer:** `multipart/form-data` is an HTTP content type for requests containing a mix of files and form fields, each sent as a separate part with its own headers. Browsers and HTTP clients use it for file uploads; ASP.NET Core model binding maps parts to `IFormFile` and form properties.

- `Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...` delimits parts in the body.
- Each part can have a name matching the parameter (`file`, `title`, `metadata`).
- Required for binary file upload — JSON cannot efficiently embed large binary payloads.
- ASP.NET Core parses multipart via form readers with configurable size and memory thresholds.
- OpenAPI documents multipart endpoints with `requestBody.content.multipart/form-data` schema.

---

#### Q3. What does `[FromForm]` do for file uploads?

**Answer:** `[FromForm]` tells model binding to read the parameter from form fields in a multipart or URL-encoded request body rather than from route, query, or JSON body. Apply it to `IFormFile` and companion metadata properties in mixed upload actions.

- Without correct binding source, complex upload actions may fail to bind file or metadata.
- Combine `IFormFile file` and `[FromForm] DocumentMetadata metadata` in one multipart request.
- JSON nested in a single multipart part is not auto-deserialized into POCOs — flatten fields or deserialize manually.
- `[ApiController]` infers `[FromForm]` for simple types in some cases but explicit attributes clarify intent.
- Not used for raw binary POST bodies — those require manual stream reading or custom formatters.

---

#### Q4. What is the `[RequestSizeLimit]` attribute?

**Answer:** `[RequestSizeLimit(bytes)]` raises or lowers the maximum allowed request body size for a specific action or endpoint, overriding the global Kestrel default (~30 MB). It must align with Kestrel, `FormOptions`, and reverse proxy limits or uploads still fail with 413.

- Apply to the action handling large uploads: `[RequestSizeLimit(1_073_741_824)]` for 1 GB.
- Works with `IHttpMaxRequestBodySizeFeature` for per-request overrides in minimal APIs.
- Does not alone fix multipart limits — pair with `[RequestFormLimits(MultipartBodyLengthLimit = ...)]`.
- Setting a limit is also a security control — reject unexpectedly large bodies early.
- Global default remains conservative; opt-in large limits only on import endpoints.

---

#### Q5. What is the difference between buffering and streaming a file download?

**Answer:** **Buffering** reads the entire file into memory (e.g., `byte[]` or `ReadAllBytesAsync`) before sending the response — simple but causes OOM under concurrent large downloads. **Streaming** sends bytes as they are read from disk or blob storage via `FileStreamResult` — constant memory regardless of file size.

- `return File(bytes, contentType)` buffers; `return File(stream, contentType)` streams.
- Streaming improves time-to-first-byte and supports large files (GB+) safely.
- Streaming enables range requests when `enableRangeProcessing: true` for seek/resume.
- Always dispose streams — `FileStreamResult` handles disposal after response completes.
- Cloud downloads should pipe blob `OpenReadAsync` directly to the response body.

---

#### Q6. What is the `Content-Disposition` header?

**Answer:** `Content-Disposition` tells the client how to handle the response body — typically as an attachment to download or inline to display in the browser. For file downloads it includes `filename` or `filename*` (UTF-8 encoded name).

- `Content-Disposition: attachment; filename="report.pdf"` prompts save dialog.
- `Content-Disposition: inline; filename="report.pdf"` opens in browser if content type supports it.
- Set via `ControllerBase.File(..., fileDownloadName)` or `ContentDispositionHeaderValue` in minimal APIs.
- Important for APIs returning binary — JSON responses do not use Content-Disposition.
- RFC 5987 `filename*` supports non-ASCII filenames.

---

#### Q7. What is the difference between attachment and inline Content-Disposition?

**Answer:** **`attachment`** instructs the client to save the file locally or open with an external application. **`inline`** instructs the client to display the content within the browser window when the content type is renderable (PDF, images).

- Use attachment for exports, installers, and sensitive documents users should not preview in-browser.
- Use inline for PDFs or images meant to be viewed directly in a tab or embedded viewer.
- Same file bytes — only the disposition and client behavior differ.
- Mobile clients may treat both similarly but attachment is safer default for unknown file types.
- ASP.NET Core `fileDownloadName` parameter on `File()` results typically sets attachment disposition.

---

#### Q8. What HTTP status does 413 Payload Too Large indicate?

**Answer:** **413 Payload Too Large** means the server refused to process the request because the body exceeds configured size limits — Kestrel `MaxRequestBodySize`, `FormOptions.MultipartBodyLengthLimit`, `[RequestSizeLimit]`, or reverse proxy body limits.

- Occurs before model binding completes — action code never runs.
- Distinct from 400 validation failure — the body was not accepted at all due to size.
- Fix by aligning limits at Kestrel, form options, attribute, and nginx/IIS `client_max_body_size`.
- Return ProblemDetails from custom middleware if you intercept size violations with a friendly message.
- Clients should chunk large uploads or use presigned direct-to-blob upload URLs when 413 persists.

---

#### Q9. What are `FormOptions` in ASP.NET Core?

**Answer:** `FormOptions` configures how ASP.NET Core parses form and multipart requests — limits on body length, number of headers, individual multipart section size, and memory buffering threshold before spilling to disk temp files.

- Configure via `services.Configure<FormOptions>(options => { ... })` or `[RequestFormLimits(...)]` per action.
- `MultipartBodyLengthLimit` defaults to 128 MB — can block uploads below Kestrel limit.
- `ValueLengthLimit` and `KeyLengthLimit` cap individual form field sizes.
- `MultipartHeadersCountLimit` prevents header explosion attacks in multipart requests.
- Must align with `[RequestSizeLimit]` and Kestrel for consistent upload behavior.

---

#### Q10. What role do Kestrel limits play in request body size?

**Answer:** Kestrel enforces `Limits.MaxRequestBodySize` (default ~30 MB) at the server level before ASP.NET Core middleware and model binding see the body. No action-level attribute can allow larger uploads unless Kestrel (or `IHttpMaxRequestBodySizeFeature`) permits it first.

- Configure globally: `builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = ...)`.
- Set to `null` to disable limit (not recommended publicly without other guards).
- Per-endpoint override via `IHttpMaxRequestBodySizeFeature.DisableMaxRequestBodySize` or max size feature on HttpContext.
- First gate in the stack — proxy limits (nginx, Azure Front Door) are equally critical.
- 413 from Kestrel never reaches `[RequestSizeLimit]` logic — configure both layers explicitly.

---

#### Q11. What is `IAsyncEnumerable` streaming for API responses?

**Answer:** Returning `IAsyncEnumerable<T>` from a controller or minimal API lets ASP.NET Core serialize items incrementally as they are produced — chunked JSON array output instead of materializing millions of records in a `List<T>` first.

- EF Core: return query as `AsAsyncEnumerable()` — do not call `ToListAsync()` before returning.
- Improves time-to-first-byte and bounds memory for large read-only exports (logs, events).
- Use `[EnumeratorCancellation]` on `CancellationToken` parameter to cancel enumeration when client disconnects.
- System.Text.Json in ASP.NET Core 8 supports async enumeration for JSON responses.
- Reverse proxies may buffer responses — disable buffering on streaming endpoints at the gateway.

---

#### Q12. What is the difference between `File()` and `PhysicalFileResult`?

**Answer:** Both stream file content to the client. `PhysicalFileResult` (returned by `PhysicalFile(path, contentType, fileDownloadName)`) reads from a file path on disk. `File(stream, ...)` or `File(bytes, ...)` accepts an open stream or byte array — use stream overload for streaming, bytes for small files only.

- `PhysicalFile` is convenient when the file already exists on server filesystem.
- `FileStreamResult` from `File(stream, ...)` works for any readable stream including blob SDK streams.
- Both support `enableRangeProcessing` for HTTP Range requests.
- `VirtualFileResult` serves from embedded resources or IFileProvider content roots.
- Avoid `File(byte[])` for large files — use stream-based overloads exclusively in Production.

---

#### Q13. What is a streaming response in Web APIs?

**Answer:** A streaming response sends the HTTP body incrementally as data is generated or read, rather than buffering the full payload before the first byte. Examples include `FileStreamResult`, `IAsyncEnumerable<T>` JSON, NDJSON line streams, and SSE.

- Reduces memory pressure and latency for large downloads and exports.
- Client receives chunked transfer encoding when Content-Length is unknown.
- CancellationToken propagates to stop production when the client disconnects.
- Proxies and load balancers must not buffer entire responses for streaming to work end-to-end.
- Distinct from upload streaming — both directions benefit from avoiding full-body buffering.

---

#### Q14. What is `[DisableFormValueModelBinding]`?

**Answer:** `[DisableFormValueModelBinding]` is a filter that disables automatic form model binding for an action so the developer can manually parse multipart data with `MultipartReader` for true streaming uploads without buffering the entire body in memory first.

- When applied, `IFormFile` parameters will **not** bind — they remain null.
- Used in advanced streaming upload samples — not compatible with standard `IFormFile` actions on the same endpoint.
- Choose one pattern: standard `IFormFile` binding **or** manual multipart parsing, not both mixed incorrectly.
- Pair with reading `Request.Body` directly and section-by-section processing for virus scan pipelines.
- Removing the attribute restores normal form binding if you switch back to standard upload handling.

---

#### Q15. What is the difference between uploading via JSON vs multipart?

**Answer:** **JSON** (`application/json`) suits metadata and small base64-encoded payloads but is inefficient and impractical for large binary files. **Multipart** (`multipart/form-data`) sends binary files as native binary parts alongside form fields — the standard for file uploads in HTTP APIs.

- JSON base64 inflates size ~33% and requires full body parsing in memory.
- Multipart streams file parts with boundaries; ASP.NET binds to `IFormFile`.
- OpenAPI: JSON endpoints use `application/json` schema; upload endpoints document `multipart/form-data`.
- Mixed APIs often use multipart for create-with-file and JSON for metadata-only updates.
- Presigned URL upload to blob storage is a third pattern — client uploads directly to storage, API receives JSON notification only.

---

#### Q16. What is range request support for large files?

**Answer:** HTTP Range requests let clients request byte subsets of a resource (`Range: bytes=0-1023`) for resume, seek, and partial downloads. Enable with `enableRangeProcessing: true` on `FileResult` — ASP.NET Core responds with **206 Partial Content** and `Content-Range` header.

- Essential for video, PDF viewers, and download managers that resume interrupted transfers.
- Server must support seeking on the underlying stream or file.
- `Accept-Ranges: bytes` header advertises capability to clients.
- Without range support, clients re-download entire multi-GB files after connection drops.
- `PhysicalFile(path, contentType, name, enableRangeProcessing: true)` is the typical Web API pattern.

---

#### Q17. What is `MemoryBufferThreshold` in FormOptions?

**Answer:** `MemoryBufferThreshold` (default 64 KB) controls how much of each multipart section is buffered in memory before ASP.NET Core spills overflow to a temporary disk file. Lower values reduce RAM use; setting it to `int.MaxValue` forces full in-memory buffering.

- Large uploads should spill to disk temp — do not raise threshold to max unless you accept OOM risk.
- Works per multipart section — file parts and form fields each have independent buffering behavior.
- Tuning affects performance on high-concurrency upload servers.
- Related to `MultipartBodyLengthLimit` — threshold is per-section memory, limit is total multipart size.
- Streaming upload tutorials manipulate this setting — misconfiguration breaks expected memory profile.

---

#### Q18. How does a reverse proxy affect large file uploads?

**Answer:** Reverse proxies (nginx, IIS ARR, Azure Application Gateway, Cloudflare) impose their own body size limits, timeouts, and buffering behavior that can reject or truncate uploads before Kestrel sees them — causing 413, 502, or silent timeouts in Production only.

- nginx: `client_max_body_size` must meet or exceed API `[RequestSizeLimit]`.
- IIS: `maxAllowedContentLength` in web.config.
- Proxy read/send timeouts must exceed worst-case upload duration for large files on slow networks.
- Response buffering on proxy can break streaming downloads and `IAsyncEnumerable` JSON exports.
- Align limits and timeouts across proxy, Kestrel, and FormOptions; test large uploads through Production-like path, not just direct Kestrel.

---

## Chapter 12. Health Checks

#### Q1. What are health checks in ASP.NET Core Web API?

**Answer:** Health checks are endpoints that report whether the application and its dependencies are functioning. ASP.NET Core registers checks with `AddHealthChecks`, maps URLs with `MapHealthChecks`, and returns `Healthy`, `Degraded`, or `Unhealthy` status for orchestrators and load balancers.

- Built-in and third-party checks cover SQL, Redis, URLs, disk space, and custom logic.
- Used by Kubernetes, Azure App Service, and load balancers to route or restart traffic.
- Separate from business API endpoints — lightweight, fast, and minimal response bodies in Production.
- Register checks in DI; map multiple endpoints with different predicates for live vs ready probes.
- Custom `IHealthCheck` implementations encapsulate domain-specific readiness logic.

---

#### Q2. What is the difference between liveness and readiness probes?

**Answer:** **Liveness** asks "should this process be restarted?" — checks only that the app process is responsive, not external dependencies. **Readiness** asks "should this instance receive traffic?" — includes dependency checks like database and cache availability.

- Liveness failure → orchestrator **restarts** the pod/container.
- Readiness failure → instance removed from load balancer **without restart**.
- Putting SQL checks on liveness causes restart loops during database maintenance.
- Readiness should fail when dependencies are down so clients are not sent to broken instances.
- Map separate URLs (`/health/live`, `/health/ready`) with tag-filtered predicates in ASP.NET Core 8.

---

#### Q3. What is `AddHealthChecks` used for?

**Answer:** `AddHealthChecks()` registers the health check service in DI and returns `IHealthChecksBuilder` to add individual checks — built-in `AddDbContextCheck`, `AddRedis`, `AddUrlGroup`, or custom `IHealthCheck` implementations with optional tags and timeouts.

- Call `builder.Services.AddHealthChecks().AddCheck(...).AddDbContextCheck<TContext>(...)` in `Program.cs`.
- Tags classify checks for filtering on different mapped endpoints.
- Timeout per check prevents one slow dependency from hanging the entire health response.
- Checks run when a health endpoint is hit — not continuously in background unless using HealthChecksUI polling.
- Degraded status allows optional dependencies to warn without marking fully Unhealthy.

---

#### Q4. What is `MapHealthChecks`?

**Answer:** `MapHealthChecks(path, options)` maps a URL endpoint that executes registered health checks and writes the aggregated result as HTTP response — typically 200 for Healthy/Degraded and 503 for Unhealthy.

- Example: `app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = ... })`.
- `HealthCheckOptions` configures which checks run, response writer format, and result caching.
- Can chain `.RequireAuthorization()` for detailed internal health routes.
- Multiple mappings share one registration but filter different check subsets via `Predicate`.
- Place after routing setup; health endpoints bypass most business middleware when mapped simply.

---

#### Q5. What is `HealthCheckOptions.Predicate`?

**Answer:** `Predicate` is a filter function `Func<HealthCheckRegistration, bool>` that selects which registered checks run for a specific mapped endpoint. Use it to run only `live`-tagged checks on liveness and `ready`-tagged checks on readiness.

- `Predicate = c => c.Tags.Contains("ready")` runs readiness checks only.
- `Predicate = _ => false` runs no checks — endpoint returns Healthy if process responds (liveness pattern).
- Null predicate runs **all** registered checks — convenient for dev, wrong for split K8s probes.
- Enables one registration set to power `/health/live`, `/health/ready`, and `/health/db` without duplication.
- Combine with tags assigned at `AddCheck(..., tags: new[] { "ready" })` registration time.

---

#### Q6. What are health check tags?

**Answer:** Tags are string labels attached to health check registrations at setup time. They classify checks (e.g., `"live"`, `"ready"`, `"db"`) so `MapHealthChecks` predicates include or exclude them per endpoint.

- One check can have multiple tags — runs on any endpoint whose predicate matches any tag.
- `AddCheck("self", () => Healthy(), tags: new[] { "live" })` for process-only liveness.
- `AddDbContextCheck<T>(tags: new[] { "ready", "db" })` for dependency checks.
- Untagged checks run on default `/health` when predicate is null (all checks).
- Tags are the primary mechanism for Kubernetes live/ready endpoint separation in ASP.NET Core 8.

---

#### Q7. Why should readiness checks include dependencies like SQL?

**Answer:** Readiness determines whether the instance should receive traffic. If SQL is unreachable, the API cannot serve requests correctly — marking the instance not ready removes it from the load balancer pool without killing the process, giving the dependency time to recover.

- Prevents clients from hitting instances that will return 500 on every database call.
- Readiness failure is temporary — instance re-enters pool when checks pass again.
- Include critical dependencies only — optional third-party APIs may use Degraded instead of Unhealthy.
- Align check timeout with orchestrator probe timeout to avoid false flapping.
- Do not include the same heavy checks on liveness — restart does not fix external SQL outage.

---

#### Q8. Why should liveness checks avoid external dependencies?

**Answer:** Liveness failure triggers a process restart. If liveness includes SQL or Redis, a brief dependency outage kills and restarts pods that cannot fix the external problem — causing restart storms, cascading failures, and unnecessary downtime.

- Liveness should answer: "Is the ASP.NET Core process hung or deadlocked?"
- A simple `() => HealthCheckResult.Healthy()` self-check or HTTP response suffices.
- External dependency failures belong on readiness — traffic drain, not restart.
- Restarting during DB maintenance removes all replicas simultaneously if liveness includes DB.
- K8s best practice: `/health/live` with no external deps; `/health/ready` with SQL/Redis/message bus.

---

#### Q9. What is `AddDbContextCheck`?

**Answer:** `AddDbContextCheck<TContext>()` registers a health check that verifies EF Core can connect to the database — typically executing a lightweight query or connection test against the configured DbContext.

- Package: `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`.
- Tag with `"ready"` — not `"live"`.
- Configure timeout: `.AddDbContextCheck<OrdersDbContext>(name: "sql", timeout: TimeSpan.FromSeconds(3))`.
- Uses connection pooling — ensure check query is cheap (`CanConnect` / `SELECT 1`), not full migrations scan.
- False failures under load often indicate probe timeout mismatch, not actual DB outage.

---

#### Q10. What happens if liveness and readiness use the same failing check?

**Answer:** When a dependency like SQL fails, **both** probes fail simultaneously — Kubernetes restarts the pod (liveness) **and** removes it from service endpoints (readiness). Restarts do not fix SQL outages, so pods enter a crash loop while traffic is also drained — the worst combined outcome from one misconfiguration.

- Brief SQL blip causes unnecessary pod kills across the fleet.
- During DB recovery, restarting pods adds startup load and delays readiness recovery.
- Fix by separating endpoints: liveness = self only, readiness = SQL/Redis.
- Same URL for both probes duplicates the failure mode — use distinct paths.
- Monitor restart counts separately from readiness failures to detect this misconfiguration.

---

#### Q11. What is HealthChecksUI?

**Answer:** HealthChecksUI is a NuGet package that provides a web dashboard polling health endpoints and displaying history, status, and detailed dependency data. It uses verbose response writers and is intended for operators — not public Production exposure.

- Register with `AddHealthChecksUI()` and map `MapHealthChecksUI()`.
- Polls configured health URLs on an interval (e.g., every 2 seconds).
- Stores history in memory or SQL — useful for ops teams on internal networks.
- Public exposure leaks dependency names, failure messages, and infrastructure topology.
- Restrict to VPN, admin auth, or disable entirely in Production public APIs.

---

#### Q12. Should health endpoints be publicly accessible?

**Answer:** Minimal liveness/readiness endpoints are often unauthenticated so orchestrators can probe them, but responses must contain **minimal information** — status only, no connection strings or exception details. Detailed health and HealthChecksUI should require authentication and internal network access.

- Public `/health/ready` with `{ "status": "Healthy" }` is acceptable.
- Public verbose JSON with SQL errors, Redis keys, and migration state is an information disclosure risk.
- Gate `/health/db` and UI behind `[Authorize]` or IP allowlists.
- Rate-limit health endpoints at the gateway to prevent abuse-driven load.
- Custom `ResponseWriter` strips the `data` dictionary for external-facing routes.

---

#### Q13. What is a `ResponseWriter` in health checks?

**Answer:** `ResponseWriter` is a delegate on `HealthCheckOptions` that controls the HTTP response body format when health checks complete. Replace the default verbose JSON with minimal output for Production public endpoints.

- Signature: `Func<HttpContext, HealthReport, Task>`.
- Default writer includes per-check duration, status, and exception messages.
- Custom writer: `await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() })`.
- HealthChecksUI uses `UIResponseWriter.WriteHealthCheckUIResponse` — verbose, internal only.
- Keep public writers stable — orchestrators parse simple status, not full HealthReport schema.

---

#### Q14. How do Kubernetes probes use health check endpoints?

**Answer:** Kubernetes configures `livenessProbe`, `readinessProbe`, and optionally `startupProbe` as HTTP GET requests to mapped ASP.NET Core health URLs. Failed liveness restarts the container; failed readiness removes the pod from Service endpoints.

- `httpGet.path: /health/live` for liveness; `/health/ready` for readiness.
- Configure `initialDelaySeconds`, `periodSeconds`, `timeoutSeconds`, and `failureThreshold` to match check latency.
- `timeoutSeconds` must exceed worst-case dependency check duration to prevent false failures.
- Startup probe allows slow EF migration boot before liveness/readiness take over.
- ASP.NET Core maps matching endpoints with tag predicates aligned to probe purpose.

---

#### Q15. What is `HealthStatus` (Healthy, Degraded, Unhealthy)?

**Answer:** `HealthStatus` is the enum result of each health check — **Healthy** (all good), **Degraded** (partial impairment, still operational), or **Unhealthy** (failure). The aggregate report status reflects the worst individual check unless configured otherwise.

- HTTP mapping: Healthy/Degraded typically return 200; Unhealthy returns 503.
- Use Degraded for optional dependencies (cache miss fallback) vs Unhealthy for critical SQL failure on readiness.
- Custom checks return `HealthCheckResult.Degraded("Cache slow")` with descriptive messages for ops.
- Orchestrators may treat Degraded differently — document team convention for load balancer behavior.
- All checks contribute to aggregate — one Unhealthy check marks the endpoint Unhealthy.

---

#### Q16. What is the difference between `/health/live` and `/health/ready`?

**Answer:** `/health/live` runs liveness checks — process self-check only, no external dependencies — telling Kubernetes whether to restart the container. `/health/ready` runs readiness checks including SQL, Redis, and other dependencies — telling the load balancer whether to send traffic.

- Different `MapHealthChecks` calls with different `Predicate` filters on tags.
- Live fails → pod restart. Ready fails → pod stays running but removed from service pool.
- Live should respond in milliseconds; ready may take 1–3 seconds for DB ping.
- Never point both K8s probes at the same URL with dependency checks included.
- Document both URLs in deployment runbooks and align probe timeouts per endpoint latency.

---

#### Q17. What timeout considerations apply to health checks under load?

**Answer:** Under traffic spikes, dependency checks (especially DB) slow down. If health probe timeout is shorter than check latency, readiness flaps — pods leave and re-enter the pool repeatedly. Health checks themselves also add query load during spikes, worsening the problem.

- Set K8s `timeoutSeconds` ≥ P99 dependency latency (often 3–5s for readiness, 1–2s for live).
- Configure per-check timeout in `AddDbContextCheck(..., timeout: TimeSpan.FromSeconds(3))`.
- Increase `periodSeconds` and `failureThreshold` to absorb brief blips without immediate removal.
- Avoid expensive queries in health checks — use connection test, not full table scan.
- Consider caching last successful readiness for a few seconds via custom wrapper to reduce DB hammering.

---

#### Q18. What information should external health endpoints expose?

**Answer:** External-facing health endpoints should expose only aggregate status (`Healthy` / `Unhealthy`), optional non-sensitive version or build id, and perhaps timestamp — never connection strings, stack traces, internal hostnames, migration errors, or per-dependency exception messages.

- Internal ops endpoints may expose dependency names and durations behind authentication.
- Custom minimal `ResponseWriter` for public routes; verbose writer for internal routes only.
- `traceId` is unnecessary on health endpoints — keep payloads tiny for fast probes.
- HealthChecksUI and `UIResponseWriter` output are never appropriate for public internet exposure.
- Treat verbose health like debug endpoints — same security review and access controls apply.

---

## Chapter 13. Integration with EF Core

#### Q1. What is the typical DbContext lifetime in a Web API request?

**Answer:** In ASP.NET Core 8 Web APIs, `DbContext` is registered as **scoped** by default via `AddDbContext`, so one instance is created per HTTP request and disposed when the request completes. That aligns the unit of work with a single API call and keeps change tracking isolated between concurrent clients.

- `AddDbContext<AppDbContext>()` registers the context in the scoped DI container — the same scope as controllers and request-scoped services.
- A scoped context must not be captured by singleton services; doing so causes `ObjectDisposedException` or cross-request data leaks under concurrency.
- Long-running background work started from a request should not hold the request's `DbContext` after the response — use `IDbContextFactory` or a new scope instead.
- `SaveChangesAsync` runs against the tracked entities accumulated during that single request scope before disposal.

---

#### Q2. Why should API controllers avoid returning EF entities directly?

**Answer:** EF entities carry navigation properties, change-tracker state, and database-internal fields that were never meant to be a public HTTP contract. Serializing them leaks schema details, risks circular reference errors, and couples clients to your persistence model.

- Navigation properties can trigger lazy-loading N+1 queries during JSON serialization if proxies are enabled.
- Entities expose columns and relationships clients should not see (internal flags, audit fields, soft-delete markers).
- DTOs give you a stable API surface that can evolve independently of table or entity refactors.
- `[ApiController]` actions should map entities to DTOs in the service layer or via projection before returning `Ok(dto)`.

---

#### Q3. What is the N+1 query problem in API endpoints?

**Answer:** N+1 occurs when one query loads a parent collection and then each item triggers an additional query for a related navigation — for example, 1 query for 100 orders plus 100 queries for each order's customer. List endpoints become slow and exhaust the database connection pool under load.

- Common cause: returning entities with lazy-loaded navigations or accessing navigations after materialization without `Include`.
- Another cause: field-by-field resolver or loop that calls `context.Orders.Where(o => o.CustomerId == id)` per row.
- Fix with a single translated query: `.Select(o => new OrderDto { CustomerName = o.Customer.Name })` or explicit `.Include()` with split queries.
- Validate with EF logging or APM — list endpoints should target one (or a fixed small number of) SQL round trips.

---

#### Q4. What is `AsNoTracking` and when should read-only API actions use it?

**Answer:** `AsNoTracking()` tells EF Core not to snapshot entities in the change tracker, reducing memory and CPU for queries whose results are only serialized to the client. Read-only GET endpoints should use it by default because they never call `SaveChanges`.

- Tracked entities remain in memory for the entire scoped `DbContext` lifetime — expensive on large list responses.
- `AsNoTrackingWithIdentityResolution()` deduplicates repeated references in graphs when needed without full tracking.
- Command endpoints (POST/PUT/DELETE) that update entities typically omit `AsNoTracking` so changes are detected on `SaveChangesAsync`.
- Hot read paths can combine `AsNoTracking()` with projection (`Select`) to fetch only columns the DTO requires.

---

#### Q5. What is the difference between `Include` and projection (`Select`) in API queries?

**Answer:** `Include` eagerly loads related entities into the change tracker as full entity graphs, while `Select` projects directly into DTOs in SQL, returning only the columns and shapes the API needs. Projection is usually preferred for read-only API responses.

- `Include(o => o.Lines)` generates JOIN or split queries and materializes complete `OrderLine` entities even if the client only needs a count or title.
- `.Select(o => new OrderDto(...))` translates to SQL that returns exactly the DTO fields — less data over the wire and less memory in the app.
- `Include` is appropriate when the service layer must modify related entities before save.
- `AsSplitQuery()` with multiple `Include`s avoids cartesian explosion on collection navigations but still loads full entities.

---

#### Q6. What is `SaveChangesAsync` in the context of API POST/PUT actions?

**Answer:** `SaveChangesAsync` persists all tracked insert, update, and delete operations accumulated in the current `DbContext` to the database in one transactional unit. API command actions call it after validating input and applying changes to entities or after `Add`/`Update`/`Remove`.

- Returns the number of affected rows; use the returned entity's generated keys (identity columns) after insert for `CreatedAtAction` responses.
- Runs inside an implicit transaction — all changes succeed or all roll back on failure.
- Should be awaited in async controller actions to avoid blocking thread-pool threads under load.
- For multi-step business flows (checkout, transfer), wrap multiple `SaveChanges` calls or operations in an explicit `BeginTransactionAsync` boundary.

---

#### Q7. What is `DbUpdateConcurrencyException` in Web APIs?

**Answer:** EF Core throws `DbUpdateConcurrencyException` when an update or delete affects zero rows because another request changed or deleted the same row first — typically when a concurrency token (`[Timestamp]`/`rowversion` or configured token) no longer matches. Web APIs should catch this and return **409 Conflict**, not 500.

- Optimistic concurrency assumes conflicts are rare; the client must retry with fresh data.
- Without a concurrency token, last-write-wins silently overwrites prior updates.
- Map to `Conflict()` or a `ProblemDetails` response with a clear message for the client to refresh and retry.
- Common in PUT/PATCH endpoints on resources edited concurrently by multiple users or tabs.

---

#### Q8. What is the repository pattern for Web APIs?

**Answer:** The repository pattern wraps data access behind interfaces such as `IOrderRepository`, hiding EF queries from controllers and services. It centralizes query logic, simplifies unit testing with fakes, and keeps HTTP layers thin — though many teams use `DbContext` directly in application services instead of a generic repository.

- Controllers depend on `IOrderService` or `IOrderRepository`, not `AppDbContext` directly.
- Generic `IRepository<T>` abstractions often leak `IQueryable` and re-expose EF — prefer specific, use-case-driven methods.
- Repositories are registered scoped, same lifetime as `DbContext`.
- EF Core already implements repository and unit-of-work patterns; add explicit repositories when testing or team conventions require a clear persistence boundary.

---

#### Q9. What is `IQueryable` and why is returning it from repositories risky?

**Answer:** `IQueryable<T>` represents a composable, deferred database query that executes only when enumerated. Returning it from repositories lets callers append filters, sorting, and paging — but also leaks EF-specific behavior, makes SQL shape unpredictable, and can cause queries to run outside the intended scope or after the `DbContext` is disposed.

- Callers may accidentally trigger client-side evaluation or multiple enumerations (double database hits).
- Exposing `IQueryable` from a repository ties upper layers to LINQ and EF translation rules.
- Prefer returning `Task<List<T>>`, `Task<T?>`, or paginated result types with explicit parameters.
- If composition is needed, keep it inside the repository or service with well-named methods.

---

#### Q10. What is the difference between scoped DbContext and `IDbContextFactory`?

**Answer:** Scoped `DbContext` from `AddDbContext` is injected once per HTTP request and shares the request's DI scope. `IDbContextFactory<TContext>` from `AddDbContextFactory` creates new context instances on demand — useful for parallel work, background tasks, or Blazor where a single scope does not map to one logical operation.

- Factory-created contexts must be disposed (`await using var context = await factory.CreateDbContextAsync()`).
- Do not inject scoped `DbContext` into singleton services; use the factory to create short-lived contexts instead.
- `AddDbContextPool` reuses context instances for performance in scoped request scenarios but still behaves as scoped per request.
- Web APIs use scoped `DbContext` for typical CRUD; factories appear in hosted services, GraphQL DataLoaders, or multi-threaded batch jobs.

---

#### Q11. What is a transaction boundary in an API checkout flow?

**Answer:** A transaction boundary defines the atomic unit of work — either all persistence steps succeed (deduct inventory, create order, record payment) or none are committed. In EF Core, use `await context.Database.BeginTransactionAsync()` or a single `SaveChangesAsync` after all related changes when they fit one context.

- Partial commits (inventory reduced but order missing) indicate a missing or incorrectly scoped transaction.
- Distributed transactions across microservices use outbox patterns or sagas, not one EF transaction spanning databases.
- Keep transactions short — hold locks only for necessary database work, not external HTTP calls to payment gateways.
- `ExecutionStrategy` with retry (SQL transient failures) wraps transactions when using `SqlServerRetryingExecutionStrategy`.

---

#### Q12. What is pagination with Skip and Take?

**Answer:** Offset pagination uses `Skip((page - 1) * pageSize).Take(pageSize)` to return a fixed window of rows for list endpoints. Clients pass `page` and `pageSize` query parameters; the API returns the slice plus optional total count metadata.

- EF Core translates `Skip`/`Take` to `OFFSET`/`FETCH` in SQL Server.
- Always cap `pageSize` (e.g., max 100) to prevent unbounded queries.
- Include stable sort order (`OrderBy`) — without it, pages can return duplicate or missing rows between requests.
- Return pagination metadata in the response body or `Link` headers (`rel="next"`, `rel="prev"`).

---

#### Q13. What causes unstable pagination in concurrent APIs?

**Answer:** Offset pagination is unstable when rows are inserted or deleted while a client walks pages — new rows shift positions, causing duplicates or skipped records between page 2 and page 3. High-write tables under concurrent load expose this frequently.

- `Skip(1000).Take(50)` becomes expensive on large offsets because the database still scans skipped rows.
- Keyset (cursor) pagination uses `WHERE Id > @lastSeenId ORDER BY Id TAKE 50` for stable, efficient paging on indexed columns.
- Timestamp-based cursors work when ids are not sequential but require tie-breaker columns.
- Document pagination strategy in the API contract so clients know whether totals and offsets are approximate.

---

#### Q14. What is DTO projection with EF Core Select?

**Answer:** DTO projection maps database rows directly to response types inside the LINQ query: `.Select(p => new ProductDto(p.Id, p.Name, p.Price))`. EF Core translates the expression to SQL that selects only required columns, avoiding entity materialization and extra mapping steps.

- Projection queries are naturally `AsNoTracking` — no entities enter the change tracker.
- Navigations can be flattened in one query: `CustomerName = o.Customer.Name` when translatable.
- Use records or constructors in DTOs for concise projection expressions.
- Client-side methods in `Select` break translation — keep projections to translatable property access and simple operators.

---

#### Q15. What happens when DbContext is injected into a Singleton service?

**Answer:** Injecting a scoped `DbContext` into a singleton creates a captive dependency — the singleton lives for the application lifetime but holds a disposed or shared context across requests. This causes `ObjectDisposedException`, stale data, and thread-safety violations under concurrent API traffic.

- ASP.NET Core DI validates scopes at startup in Development when `ValidateScopes` is enabled, surfacing the misconfiguration early.
- Singleton caches must not store entities tracked by a context — they become detached stale graphs.
- Fix by making the service scoped, or inject `IDbContextFactory<TContext>` and create a context per operation.
- Background singleton services should create a new DI scope (`IServiceScopeFactory.CreateScope()`) per work item.

---

#### Q16. What is lazy loading and why is it problematic for APIs?

**Answer:** Lazy loading automatically queries related entities when navigation properties are accessed on tracked proxies. In Web APIs, serializing an entity graph or touching navigations after the initial query triggers unexpected extra SQL (N+1) during response generation.

- Enabled via `UseLazyLoadingProxies()` — often disabled in API projects in favor of explicit loading or projection.
- Serializers walking object graphs can fire dozens of lazy loads per request without obvious code in the controller.
- Explicit `Include` or projection makes query cost visible and measurable in one place.
- If proxies are enabled, returning entities directly from actions is especially dangerous.

---

#### Q17. What is the difference between `FindAsync` and `FirstOrDefaultAsync` in APIs?

**Answer:** `FindAsync(key)` uses the context's local cache first, then queries by primary key — efficient for composite or single keys already tracked. `FirstOrDefaultAsync(predicate)` always translates to a SQL query with a `WHERE` clause and is required for non-key lookups or filters.

- `FindAsync` only works with primary key values, not arbitrary predicates.
- `FirstOrDefaultAsync(o => o.Id == id)` hits the database even if the entity is already tracked (unless EF's identity resolution applies in specific cases).
- For GET-by-id endpoints, either works when querying by PK; `FindAsync` can skip a round trip if the entity is cached in the context.
- Use `FirstOrDefaultAsync` with `AsNoTracking()` for read-only detail endpoints when the entity is not already tracked.

---

#### Q18. What is `AddDbContextFactory` used for in Web APIs?

**Answer:** `AddDbContextFactory<AppDbContext>()` registers a factory that creates fresh `DbContext` instances outside the normal request scope. Web APIs use it for parallel operations within one request, background queue processors, or services that must not share a scoped context across threads.

- Register with `builder.Services.AddDbContextFactory<AppDbContext>(options => ...)` alongside or instead of scoped `AddDbContext` depending on needs.
- Each `CreateDbContext()` / `CreateDbContextAsync()` returns a context the caller must dispose.
- Common in GraphQL DataLoaders, report generators, and `IHostedService` workers that process jobs after the HTTP response.
- Factory options can mirror pooled configuration but instances are not shared across concurrent callers.

---

## Chapter 14. API Testing & Integration Tests

#### Q1. What is `WebApplicationFactory` in ASP.NET Core?

**Answer:** `WebApplicationFactory<TEntryPoint>` bootstraps the real application assembly in an in-memory test server, running the full middleware pipeline, routing, DI, and configuration without listening on a network port. Integration tests use it to send real HTTP requests against the app as deployed.

- Reference the API project's `Program` class (or expose it via `public partial class Program` for top-level statements).
- Subclass the factory to override `ConfigureWebHost` and replace services (database, auth, external HTTP clients).
- Tests live in a separate xUnit/NUnit project referencing `Microsoft.AspNetCore.Mvc.Testing`.
- Exercises Kestrel's `TestServer` host — closer to production behavior than mocking controllers in isolation.

---

#### Q2. What is the difference between unit tests and integration tests for APIs?

**Answer:** Unit tests isolate a class (service, validator) with mocked dependencies and no HTTP pipeline. Integration tests boot the application (or a slice of it) and verify behavior through HTTP requests, real middleware, serialization, and often a real or containerized database.

- Unit tests are fast and pinpoint logic failures; they do not catch routing, binding, or middleware misconfiguration.
- Integration tests catch issues like wrong status codes, auth pipeline gaps, and JSON contract mismatches.
- The test pyramid favors many unit tests, fewer integration tests, and minimal end-to-end tests against live external systems.
- Karat-style reviews often fail candidates who label live HTTP calls to sandbox APIs as "unit tests."

---

#### Q3. What is an in-memory test server for Web APIs?

**Answer:** The in-memory test server (`TestServer`) hosts the ASP.NET Core app inside the test process, accepting `HttpClient` requests that flow through the real middleware pipeline without opening a TCP port. `WebApplicationFactory` configures this server automatically.

- `factory.CreateClient()` returns an `HttpClient` whose `BaseAddress` points at the test server.
- Requests are in-process — no firewall or port conflicts in CI.
- Behavior matches production pipeline semantics (model binding, filters, auth) unlike direct controller instantiation.
- Not identical to production Kestrel (HTTP/2 edge cases, TLS) — supplement with deploy-environment smoke tests when needed.

---

#### Q4. What does `CreateClient()` on WebApplicationFactory return?

**Answer:** `CreateClient()` returns a preconfigured `HttpClient` wired to the factory's in-memory test server, with the application's base address and a handler that dispatches requests through the hosted pipeline. Tests call `GetAsync`, `PostAsJsonAsync`, etc., on this client.

- Optional `WebApplicationFactoryClientOptions` set base address, redirect handling, and cookie handling.
- The client shares the factory's service provider — service overrides in `ConfigureWebHost` apply to all requests from that client.
- Create one client per test or reuse from a fixture depending on isolation needs.
- Must dispose the factory (and client if created with `CreateDefaultClient` patterns that require it) to release host resources.

---

#### Q5. What is the test pyramid for API development?

**Answer:** The test pyramid recommends many fast unit tests at the base, a moderate layer of integration tests, and few slow end-to-end tests at the top. For Web APIs, unit tests cover services and validators; integration tests cover HTTP contracts; E2E tests cover critical flows against deployed environments.

- Unit: business rules, mapping, validation logic with mocked `DbContext` or repositories.
- Integration: `WebApplicationFactory` + test database for POST/GET status codes, ProblemDetails shape, auth.
- E2E: post-deploy smoke against staging with real dependencies — keep out of default CI when flaky or costly.
- Inverted pyramids (many Selenium/live-API tests, few unit tests) slow feedback and hide root causes.

---

#### Q6. What is Mock `HttpMessageHandler` used for?

**Answer:** A mock `HttpMessageHandler` intercepts `HttpClient` outbound calls in tests, returning canned responses without hitting the network. Register it via `new HttpClient(mockHandler)` or `IHttpClientFactory` test configuration to isolate services that call external APIs.

- Subclass `HttpMessageHandler` and override `SendAsync` to assert request URL, headers, and body, then return `HttpResponseMessage` with test JSON.
- Libraries like Moq can mock `Protected()` `SendAsync` on the handler base class.
- Tests verify your service sends the correct payload to payment, identity, or notification APIs without sandbox charges.
- Distinguish this from integration tests — the API under test is real; only downstream HTTP is faked.

---

#### Q7. Why should integration tests not use the production database?

**Answer:** Integration tests mutate data, seed fixtures, and may run in parallel — pointing at production or shared dev databases risks destroying real data, causing unique constraint collisions, and creating non-deterministic failures. Tests need isolated, disposable storage.

- Override `DbContext` registration in `ConfigureWebHost` with Testcontainers SQL, local SQLite, or a dedicated CI database.
- Never embed production connection strings in test projects or pipeline variables consumed by default `dotnet test`.
- Tests that leave debris break subsequent runs and other developers' local environments.
- Production-like data belongs in staging with controlled E2E suites, not automated integration test defaults.

---

#### Q8. What is Testcontainers for API testing?

**Answer:** Testcontainers spins up real Docker containers (SQL Server, PostgreSQL, Redis) during test runs, giving integration tests authentic database behavior without a shared permanent instance. The container is created in fixture setup and destroyed after tests complete.

- NuGet packages like `Testcontainers.MsSql` integrate with xUnit `IAsyncLifetime` fixtures.
- Tests hit real SQL semantics (constraints, transactions, raw SQL) that EF InMemory does not emulate.
- CI agents must support Docker — GitHub Actions and Azure DevOps pipelines commonly do.
- Slower than InMemory but far more reliable for testing EF migrations, concurrency, and SQL-specific queries.

---

#### Q9. What is the difference between EF InMemory and real SQL for API tests?

**Answer:** EF Core InMemory provider stores data in a process-local dictionary — fast and simple but ignores SQL semantics like foreign keys, unique constraints, transactions, and raw SQL. Real SQL (Testcontainers or local instance) validates what production actually enforces.

- InMemory suits tests focused purely on HTTP routing and serialization with trivial persistence.
- Migration application, concurrency tokens, and `FromSqlRaw` require a relational provider.
- InMemory provider name changed in EF Core 8 — use `UseInMemoryDatabase` knowing limitations are documented by Microsoft.
- Many teams use InMemory for fast controller tests and SQL containers for repository or full-stack integration tests.

---

#### Q10. What is `ConfigureWebHost` in WebApplicationFactory?

**Answer:** Override `ConfigureWebHost(IWebHostBuilder builder)` in a custom `WebApplicationFactory` subclass to change the test host configuration — swap connection strings, register test auth handlers, replace `HttpClient` handlers, or set `Environment` to "Testing".

- Call `builder.ConfigureTestServices(services => { ... })` to replace or decorate DI registrations after the app's `Program.cs` runs.
- Use `builder.UseEnvironment("Testing")` to trigger test-specific `appsettings.Testing.json` overrides.
- Typical pattern: remove `DbContext` SQL registration, add InMemory or Testcontainers connection.
- Runs before the test server starts — all requests through `CreateClient()` see the overridden services.

---

#### Q11. How do you test authenticated API endpoints?

**Answer:** Integration tests must satisfy the same authentication middleware the app uses — attach a valid JWT in `Authorization: Bearer`, register a test authentication handler that auto-succeeds, or use `ConfigureTestServices` to replace `AddAuthentication` with a scheme that injects claims.

- `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testToken)` when using real JWT validation with a test signing key.
- `services.AddAuthentication("Test").AddScheme<..., TestAuthHandler>("Test", ...)` bypasses token crypto in focused pipeline tests.
- Set `[Authorize]` policies by adding required claims in the test handler's `ClaimsIdentity`.
- Testing anonymous vs authenticated vs forbidden (403) paths requires separate requests with different identities.

---

#### Q12. What is `PostAsJsonAsync` in integration tests?

**Answer:** `PostAsJsonAsync` is an `HttpClient` extension (from `System.Net.Http.Json`) that serializes a CLR object to JSON, sets `Content-Type: application/json`, and POSTs to the given URL. Integration tests use it to exercise model binding and validation on API endpoints.

- Pair with `ReadFromJsonAsync<T>()` to deserialize response DTOs and assert properties.
- Uses `System.Text.Json` defaults — camelCase property names match ASP.NET Core 8 API conventions.
- Assert `response.StatusCode`, `response.Headers.Location`, and ProblemDetails on validation failures.
- Available on `HttpClient` returned from `WebApplicationFactory.CreateClient()`.

---

#### Q13. What causes flaky parallel integration tests?

**Answer:** Parallel tests that share one database file, one Testcontainers instance without isolation, or mutable static seed data race on writes and locks — producing intermittent duplicate key errors, "database is locked," or order-dependent pass/fail. Fix isolation or disable parallelism for shared-state collections.

- xUnit runs test classes in parallel by default — `[Collection("Database")]` serializes tests sharing a fixture.
- Static `WebApplicationFactory` fields with shared InMemory database name collide across classes.
- Use unique database names per test (`Guid.NewGuid()` suffix) or one container per collection fixture.
- Flaky retries mask design problems — prefer deterministic isolation over `[Retry]` attributes.

---

#### Q14. What is the difference between mocking a service vs mocking HttpClient?

**Answer:** Mocking a service (e.g., `IPaymentService`) replaces an internal DI dependency — the HTTP pipeline, routing, and controllers still run. Mocking `HttpMessageHandler` replaces outbound HTTP from a typed client while the real service code executes and calls the fake handler.

- Service mock: test controller or filter behavior when payment always succeeds without running `PaymentService` logic.
- Handler mock: test `PaymentService` builds correct HTTP requests and parses responses; pipeline includes real service registration.
- Choose the boundary under test — mock at the edge closest to what you want to verify.
- Over-mocking services in integration tests reduces the test to a unit test with extra steps.

---

#### Q15. What is a test fixture for API integration tests?

**Answer:** A test fixture (xUnit `IClassFixture<T>` or NUnit `OneTimeSetUp`) creates expensive shared resources once per test class or collection — `WebApplicationFactory`, Testcontainers database, seeded data — and disposes them after tests finish. It amortizes startup cost while controlling isolation boundaries.

- `public class OrderTests : IClassFixture<CustomWebApplicationFactory>` receives the factory via constructor injection.
- `IAsyncLifetime` fixtures start Docker containers asynchronously before any test runs.
- Collection fixtures share one factory across multiple test classes that must not run in parallel.
- Fixture scope defines how much state tests share — broader scope means stronger isolation requirements.

---

#### Q16. What is seed data in API integration tests?

**Answer:** Seed data pre-populates the test database with known entities before assertions run — users, products, orders — so tests start from a predictable state. Apply seeds in fixture setup after migrations, or insert via `DbContext` in each test's arrange phase.

- Enables `GET /api/orders/1` to return a known row without depending on another test's POST.
- Reset strategy: recreate database per test, truncate tables, or use transactions rolled back after each test.
- Avoid hard-coded ids that assume identity seed values — capture ids from seed inserts or use well-known GUIDs.
- Seed only what the test needs to keep arrange sections readable and fast.

---

#### Q17. What is the difference between testing controllers directly vs testing via HTTP?

**Answer:** Direct controller testing instantiates the controller with mocked dependencies and calls action methods — skipping routing, model binding, filters, and middleware. HTTP testing via `WebApplicationFactory` sends real requests through the full pipeline, catching binding source mistakes and auth gaps.

- Direct: fast, good for branching logic inside actions when pipeline is tested elsewhere.
- HTTP: validates `[ApiController]` automatic 400 responses, `[FromBody]` binding, route templates, and content negotiation.
- Direct tests require manual setup of `ControllerContext`, `ModelState`, and `HttpContext`.
- ASP.NET Core 8 interview and production guidance favors HTTP integration tests for contract verification.

---

#### Q18. Why must HttpClient instances be disposed properly in tests?

**Answer:** `HttpClient` and `WebApplicationFactory` hold sockets, `TestServer` hosts, and connection pool resources. Undisposed clients and factories leak handles in long CI runs, causing port exhaustion, socket starvation, and slow or failing subsequent test assemblies.

- Dispose `WebApplicationFactory` in fixture teardown (`Dispose()` or `IAsyncDisposable`).
- `CreateClient()` clients are typically short-lived per test; factory manages the underlying handler pool when reused.
- Do not create a new `HttpClient` per test without disposal when using custom handlers outside the factory pattern.
- Testcontainers and factory disposal order: stop container, dispose factory, release clients.

---

## Chapter 15. GraphQL with HotChocolate

#### Q1. What is GraphQL?

**Answer:** GraphQL is a query language and runtime for APIs where clients request exactly the fields they need in a single POST to a `/graphql` endpoint. A typed schema defines queries (reads), mutations (writes), and subscriptions (real-time pushes) with server-side resolvers backing each field.

- Clients send a document like `{ order(id: 1) { id total lines { sku quantity } } }` — shape drives the response.
- One endpoint replaces many REST resources for aggregated mobile or SPA clients.
- Strong typing enables tooling: introspection, schema stitching, and client code generation.
- Hot Chocolate is the common GraphQL server for ASP.NET Core 8.

---

#### Q2. What is the difference between GraphQL and REST?

**Answer:** REST exposes many resource-oriented URLs with fixed response shapes per endpoint; GraphQL exposes one endpoint where the client selects nested fields in one request. REST uses HTTP verbs and status codes per resource; GraphQL typically POSTs to `/graphql` and returns 200 with errors in the body for partial failures.

- REST over-fetches when endpoints return more fields than the client needs; GraphQL requests only listed fields.
- REST under-fetches when a screen needs data from multiple endpoints; GraphQL nests related data in one round trip.
- REST caching uses HTTP semantics (GET, ETags); GraphQL POST requests require application-level caching strategies.
- GraphQL shifts complexity to the server (resolvers, N+1, query cost limits); REST keeps endpoints simpler and cache-friendly.

---

#### Q3. What is a GraphQL schema?

**Answer:** The schema is the contract defining all types, fields, arguments, and root operations (`Query`, `Mutation`, `Subscription`) clients may request. Hot Chocolate builds the schema from C# types, attributes, and fluent configuration at startup.

- Each type field maps to a resolver method or property that fetches data.
- Schema is introspectable — tools query `__schema` and `__type` unless introspection is disabled in production.
- Breaking changes (removing fields) require versioning or deprecation policies like REST.
- `AddGraphQLServer()` registers types and generates the executable schema.

---

#### Q4. What is a GraphQL query vs mutation?

**Answer:** Queries are read operations that fetch data without side effects; mutations are write operations that create, update, or delete data. By convention, queries may run in parallel; mutations run serially in order to avoid race conditions on related writes.

- Query root: `query { products { id name } }`.
- Mutation root: `mutation { createOrder(input: { ... }) { id status } }`.
- Both are POSTed to the GraphQL endpoint with a JSON body `{ "query": "..." }`.
- Side-effect-free reads belong on `Query`, not `Mutation` — aligns with HTTP safe/idempotent mental model.

---

#### Q5. What is a resolver in GraphQL?

**Answer:** A resolver is the function that returns the value for a single field in the schema — given the parent object, field arguments, and request context. In Hot Chocolate, resolver methods on types or `[GraphQLName]` methods on query classes execute per field selection.

- Root query resolvers load entry points; nested field resolvers load related data (author → books).
- Resolvers receive `[Parent]`, `[Argument]`, and injected services (`[Service] AppDbContext`).
- Naive per-row database access in nested resolvers causes N+1 query explosions.
- Resolvers run within a request scope — align DI lifetimes with scoped `DbContext`.

---

#### Q6. What is the N+1 problem in GraphQL?

**Answer:** GraphQL N+1 occurs when a list field resolver runs a separate database query for each parent item — loading 50 authors triggers 50 additional queries for each author's books. Without batching, GraphQL list queries perform worse than a well-designed REST join endpoint.

- Root query returns N parents; each child field resolver queries independently.
- Symptom: 1 query for parents plus N queries for children in APM traces.
- Fix with DataLoader batching or eager loading at the root when the selection set is known.
- Same conceptual problem as EF lazy loading in REST, but easier to trigger because clients control field depth.

---

#### Q7. What is DataLoader in Hot Chocolate?

**Answer:** DataLoader batches and caches loads within a single GraphQL request — collecting keys requested by field resolvers and issuing one query (`WHERE Id IN (...)`) instead of many. Hot Chocolate provides `BatchDataLoader`, `GroupedDataLoader`, and `CacheDataLoader` base classes.

- Register loaders in DI (scoped) and call `LoadAsync(key)` from field resolvers.
- Request-scoped cache prevents duplicate loads when the same key appears in multiple branches.
- Essential for production GraphQL APIs exposing nested relational data from EF Core.
- `.AddDataLoader<T>()` integrates loaders with the Hot Chocolate execution engine.

---

#### Q8. What is over-fetching in REST vs GraphQL?

**Answer:** Over-fetching happens when an API response includes more data than the client needs — REST list endpoints returning full entity graphs with unused columns and navigations. GraphQL lets clients specify fields, reducing payload size when clients request minimal selections.

- REST: `GET /api/users/1` always returns the same DTO shape regardless of whether the UI needs only `name`.
- GraphQL: client requests `{ user { name } }` and omits email, address, and permissions.
- REST can mitigate with sparse field parameters or separate lightweight endpoints — adds API surface area.
- GraphQL does not eliminate over-fetching on the server if resolvers still load full entities before projecting.

---

#### Q9. What is under-fetching in REST?

**Answer:** Under-fetching occurs when a client needs data from multiple REST endpoints and must chain requests — for example, orders, then customers, then products for a dashboard. Each round trip adds latency and complicates mobile apps on slow networks.

- A screen needing 5 resources may require 5+ HTTP calls with REST.
- BFF (Backend for Frontend) aggregates REST calls server-side as an alternative to GraphQL.
- GraphQL nested queries fetch related data in one request when resolvers are efficient (DataLoader-backed).
- REST `_embed` or `include` query parameters (sparse fieldsets) partially address under-fetching.

---

#### Q10. What is Hot Chocolate?

**Answer:** Hot Chocolate is a high-performance GraphQL server for .NET that integrates with ASP.NET Core 8 through `AddGraphQLServer()` and `MapGraphQL()`. It provides schema-first and code-first modeling, DataLoader, authorization, filtering/sorting, and Banana Cake Pop IDE.

- Successor ecosystem leader on .NET after GraphQL.NET; actively maintained with LTS-friendly releases.
- Supports global object identification (Relay), subscriptions via WebSockets, and Apollo Federation.
- Executes queries with middleware pipeline: parsing, validation, cost analysis, and resolver execution.
- NuGet: `HotChocolate.AspNetCore` for web hosting integration.

---

#### Q11. What is GraphQL introspection?

**Answer:** Introspection lets clients query the schema itself — listing types, fields, arguments, and descriptions via special meta-fields like `__schema` and `__type`. Tools (Banana Cake Pop, GraphiQL, codegen) depend on it; attackers use it to discover hidden admin fields in production.

- Example: `{ __schema { types { name fields { name } } } }` reveals the full API surface.
- Disable or restrict introspection for anonymous users in production environments.
- Hiding Banana Cake Pop UI does not disable introspection — clients can still POST introspection queries.
- Pair introspection restrictions with field-level authorization on sensitive resolvers.

---

#### Q12. What is query depth limiting?

**Answer:** Query depth limiting caps how many nested levels a GraphQL query may traverse — blocking `{ a { b { c { d { ... } } } } }` attacks that exponentially expand resolver work. Hot Chocolate provides `AddMaxExecutionDepth(n)` to enforce limits at execution time.

- Deep recursive schemas (comments on comments, org hierarchies) are abuse vectors without limits.
- Depth limits complement complexity/cost analysis — depth alone does not catch wide fan-out at one level.
- Tune limits against legitimate client queries; mobile apps rarely need depth above 10–15.
- Exceeded depth returns a GraphQL error before resolvers exhaust CPU or database connections.

---

#### Q13. What is query complexity in GraphQL?

**Answer:** Query complexity assigns a cost score to each field and rejects queries exceeding a budget — penalizing wide lists and expensive resolvers. Hot Chocolate supports cost analysis middleware to prevent clients from requesting `users { friends { friends { friends } } }` at scale.

- Each field contributes weight; list fields multiply cost by expected or actual child count.
- Protects against queries that are shallow but wide (1000 items × 50 fields).
- Combine with rate limiting and authentication for public GraphQL endpoints.
- Complexity rules should reflect real database cost, not arbitrary constants.

---

#### Q14. How does authorization work on GraphQL fields?

**Answer:** Hot Chocolate integrates ASP.NET Core authorization — apply `[Authorize]` on query types, mutation classes, or individual field resolvers. Policies and roles evaluate per field, so public `Query` types can expose both anonymous catalog fields and admin-only fields with different auth requirements.

- Field-level auth hides sensitive data without separate GraphQL schemas per role.
- Unauthorized fields return GraphQL errors in the `errors` array; HTTP status may remain 200.
- Use `[Authorize(Roles = "Admin")]` or named policies matching REST API auth configuration.
- Introspection may still reveal field names — security through obscurity is insufficient without auth on resolvers.

---

#### Q15. What is `AddGraphQLServer`?

**Answer:** `AddGraphQLServer()` registers Hot Chocolate's GraphQL executor, schema builder, and supporting services in DI. Chain configuration methods to register query types, mutations, DataLoaders, filtering, and instrumentation before building the host.

- Called in `Program.cs`: `builder.Services.AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>();`
- Returns `IRequestExecutorBuilder` for fluent registration of types, directives, and middleware.
- Integrates with ASP.NET Core logging, DataLoader scopes, and optional Apollo tracing.
- Required counterpart to `MapGraphQL()` endpoint mapping.

---

#### Q16. What is `MapGraphQL`?

**Answer:** `MapGraphQL()` adds endpoint routing for the GraphQL HTTP transport — typically POST `/graphql` — and optionally WebSocket endpoints for subscriptions. It connects incoming requests to Hot Chocolate's executor pipeline.

- `app.MapGraphQL()` after `app.Build()` exposes the schema to HTTP clients.
- `MapGraphQL("/api/graphql")` customizes the path.
- Enable Banana Cake Pop in Development with `.WithOptions(new GraphQLServerOptions { Tool = { Enable = true } })`.
- Place after auth middleware when endpoints require authenticated access.

---

#### Q17. What is Banana Cake Pop?

**Answer:** Banana Cake Pop is Hot Chocolate's built-in GraphQL IDE — a browser UI for exploring the schema, writing queries, and viewing responses. It replaces GraphQL Playground in modern Hot Chocolate versions and ships embedded with the server in Development.

- Accessible at `/graphql` when tooling is enabled — similar role to Swagger UI for REST.
- Disable or restrict in Production to avoid exposing schema details and ad-hoc query execution.
- Supports exporting schema SDL and testing mutations against local or staging servers.
- Not a substitute for securing introspection and query cost limits on public endpoints.

---

#### Q18. When would you choose GraphQL over REST for an API?

**Answer:** Choose GraphQL when diverse clients (mobile, web, third-party) need flexible, nested data shapes from one endpoint and your team can invest in DataLoader batching, query limits, and resolver DI. Prefer REST when caching, simple CRUD, file uploads, and standard HTTP semantics matter more than client-driven field selection.

- Good fit: product catalog + user + cart screens with different field needs; rapid frontend iteration without new REST endpoints per screen.
- Poor fit: public APIs needing CDN caching, binary uploads, strict rate limiting by route, or teams without GraphQL operational experience.
- Hybrid architectures expose REST at the edge and GraphQL internally behind a BFF.
- ASP.NET Core 8 supports both in one host — GraphQL does not replace REST universally.

---

## Chapter 16. gRPC Web APIs

#### Q1. What is gRPC?

**Answer:** gRPC is a high-performance RPC framework using HTTP/2 and Protocol Buffers for contract-first, strongly typed service-to-service communication. Clients and servers generate stubs from `.proto` files, calling methods like local functions with binary serialization instead of JSON.

- Built on HTTP/2 multiplexing, header compression, and bidirectional streaming support.
- First-class in .NET via `Grpc.AspNetCore` with Kestrel as the server.
- Ideal for low-latency internal microservice calls on .NET, Go, Java, and other supported languages.
- Uses `.proto` contracts versioned independently of REST URL paths.

---

#### Q2. What is the difference between gRPC and REST?

**Answer:** REST models resources with HTTP verbs, JSON payloads, and standard status codes on many URLs; gRPC models RPC methods on a service with Protobuf messages over HTTP/2 on typically one base path per service. REST is human-readable and browser-friendly; gRPC is binary, contract-strict, and optimized for service meshes.

- REST: `GET /api/orders/1` returns JSON; clients infer shape from documentation or OpenAPI.
- gRPC: `GetOrder(OrderRequest)` returns typed `OrderReply` — compiler-checked on both sides.
- REST leverages HTTP caching and CDN; gRPC requires grpc-web and different caching strategies for browsers.
- gRPC supports streaming (server, client, bidirectional); REST traditionally uses chunked HTTP or SSE/WebSockets separately.

---

#### Q3. What are Protocol Buffers?

**Answer:** Protocol Buffers (protobuf) are Google's language-neutral serialization format defined in `.proto` files. Messages declare typed fields with numbered tags; the compiler generates C# classes and serialization code that produces compact binary payloads on the wire.

- Smaller and faster to serialize/deserialize than JSON for structured data.
- Field numbers identify wire data — names are not sent on the wire.
- `proto3` is the current syntax for new gRPC services in .NET.
- Backward compatibility depends on field number rules, not C# property names.

---

#### Q4. What is a `.proto` file?

**Answer:** A `.proto` file defines gRPC services, RPC methods, request/response messages, and enums in a language-neutral contract. The .NET build integrates `Grpc.Tools` to generate C# server base classes and client stubs from the proto at compile time.

- Declares `service OrderService { rpc GetOrder(OrderRequest) returns (OrderReply); }`.
- Messages specify fields as `type name = number;` — numbers are permanent wire identifiers.
- Shared `.proto` files enable polyglot clients (Java mobile app, .NET backend) from one contract.
- Place protos in the project with `<Protobuf Include="Protos\order.proto" GrpcServices="Server" />` in the `.csproj`.

---

#### Q5. What is gRPC-Web?

**Answer:** gRPC-Web is a protocol variant that lets browser clients call gRPC services over HTTP/1.1 or HTTP/2 with JSON-like framing adapters, because browsers cannot use native gRPC's HTTP/2 trailing headers directly. ASP.NET Core enables it with `AddGrpc().EnableGrpcWeb()` and `UseGrpcWeb()` middleware.

- Required for SPA/browser consumers without a native gRPC stack.
- Often deployed behind Envoy or nginx that translates grpc-web to native gRPC.
- Needs explicit CORS configuration for cross-origin browser apps.
- Unary calls are most common; streaming support in browsers is more limited than server-to-server gRPC.

---

#### Q6. Why can't browsers use native gRPC directly?

**Answer:** Native gRPC relies on HTTP/2 features — trailing headers for status, binary framing, and full duplex streaming — that browser `fetch` and `XMLHttpRequest` APIs do not expose completely. Browsers lack a first-class gRPC client without grpc-web translation or a proxy.

- gRPC status and metadata arrive in HTTP/2 trailers inaccessible to standard browser HTTP APIs.
- Corporate proxies and HTTP/1.1-only paths break native gRPC from client-side JavaScript.
- grpc-web wraps calls in forms browsers can send; a gateway converts to native gRPC server-side.
- Mobile and server .NET clients use `Grpc.Net.Client` with full HTTP/2 support — no grpc-web needed.

---

#### Q7. What is a unary gRPC call?

**Answer:** A unary call is the simplest gRPC pattern — one client request message and one server response message, analogous to a REST request/response. Most CRUD-style RPC methods are unary: `GetOrder`, `CreateOrder`, `CancelOrder`.

- Client awaits `var reply = await client.GetOrderAsync(request);`.
- Maps naturally to single database lookups or command operations.
- Uses one HTTP/2 stream opened and closed for the call.
- Deadlines and cancellation tokens apply to the entire round trip.

---

#### Q8. What is server streaming in gRPC?

**Answer:** Server streaming RPCs send one client request and a stream of multiple server response messages — useful for large result sets, live updates, or file chunks without loading everything into memory. The client reads messages asynchronously from `AsyncServerStreamingCall`.

- Defined in `.proto` as `rpc ListOrders(OrderFilter) returns (stream OrderReply);`.
- Server calls `await responseStream.WriteAsync(order)` repeatedly until complete.
- More efficient than paginated REST when the client consumes data incrementally.
- Client must handle backpressure and cancellation mid-stream via `CancellationToken`.

---

#### Q9. What is `RpcException`?

**Answer:** `RpcException` is the gRPC-specific exception type carrying a `StatusCode` (similar to gRPC status codes) and detail message. Servers throw it to signal client errors; clients catch it to distinguish `NotFound`, `InvalidArgument`, and `DeadlineExceeded` from transport failures.

- Throw: `throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));`.
- Clients inspect `ex.StatusCode` and `ex.Trailers` for structured error metadata.
- Map business validation failures to `InvalidArgument` rather than generic `Internal` for clearer client handling.
- Do not leak stack traces or internal details in production status messages.

---

#### Q10. What are gRPC status codes?

**Answer:** gRPC status codes are standardized result indicators parallel to HTTP status codes but carried in HTTP/2 trailers — `OK`, `Cancelled`, `InvalidArgument`, `NotFound`, `AlreadyExists`, `PermissionDenied`, `Unauthenticated`, `ResourceExhausted`, `FailedPrecondition`, `Aborted`, `OutOfRange`, `Unimplemented`, `Internal`, `Unavailable`, `DeadlineExceeded`, and others.

- `OK` (0) means success; non-zero codes indicate failure at the RPC layer.
- Map domain errors consistently — e.g., duplicate create → `AlreadyExists`, auth failure → `Unauthenticated` vs `PermissionDenied`.
- Clients retry idempotent calls on `Unavailable` and `DeadlineExceeded` with backoff.
- Status details can include `google.rpc.Status` protobuf extensions for structured error info.

---

#### Q11. What is a deadline in gRPC?

**Answer:** A deadline specifies the absolute time by which an RPC must complete — propagated from client to server so work stops when the budget expires. Clients set `CallOptions(deadline: DateTime.UtcNow.AddSeconds(5))`; servers observe `ServerCallContext.Deadline`.

- Prevents hung calls from tying up threads and database connections indefinitely.
- When exceeded, the call ends with status `DeadlineExceeded`.
- Server code must pass `context.CancellationToken` to EF and downstream calls to honor the deadline.
- Chain deadlines through microservice calls — each hop should use the remaining budget, not a fresh full timeout.

---

#### Q12. How does cancellation work in gRPC?

**Answer:** gRPC cancellation propagates a `CancellationToken` from client to server over HTTP/2 when the client cancels or the deadline passes. Server methods should pass `ServerCallContext.CancellationToken` to long-running EF queries, HTTP calls, and loops so work stops promptly.

- Client: `cts.Cancel()` or dispose the call disposes the underlying stream.
- Server: `await context.CancellationToken.ThrowIfCancellationRequested()` or pass token to `FirstOrDefaultAsync(..., token)`.
- Ignoring cancellation wastes SQL and thread pool resources after the client already disconnected.
- Map cooperative cancellation to `StatusCode.Cancelled` when appropriate.

---

#### Q13. What is backward compatibility in Protocol Buffers?

**Answer:** Protobuf wire compatibility requires never changing the wire type or number of existing fields and never reusing field numbers for different semantics. New fields are added with new numbers; old clients ignore unknown fields; new clients use default values for missing old fields.

- Renaming a field in `.proto` is safe — only tag numbers matter on the wire.
- Changing field 2 from `double` to `string` breaks old clients — use a new field number instead.
- Mark deprecated fields with `deprecated = true` and reserve removed numbers with `reserved`.
- Mobile apps lag server deploys — treat `.proto` changes as multi-version contracts.

---

#### Q14. What is `ServerCallContext`?

**Answer:** `ServerCallContext` is passed to every gRPC service method, providing request metadata (headers), response trailers, peer identity, deadline, and `CancellationToken`. It is the gRPC equivalent of `HttpContext` for RPC handlers.

- Read client metadata: `context.RequestHeaders.GetValue("correlation-id")`.
- Write trailers: `context.ResponseTrailers.Add("processed-by", "orders-service")`.
- Access `context.User` after authentication middleware maps credentials.
- Use `context.CancellationToken` for all async I/O in the service method.

---

#### Q15. What is the difference between gRPC and JSON HTTP APIs?

**Answer:** JSON HTTP APIs (ASP.NET Core controllers or Minimal APIs) serialize text payloads negotiated via `Content-Type`, discovered through OpenAPI, and consumed universally including browsers. gRPC uses binary Protobuf over HTTP/2 with generated stubs — faster and stricter but less visible without specialized tools.

- JSON: human-readable, easy debugging with curl, broad client support, Swagger documentation.
- gRPC: smaller payloads, strongly typed contracts, built-in streaming, better performance for internal calls.
- JSON APIs suit public partners and SPAs; gRPC suits service-to-service on shared `.proto` contracts.
- ASP.NET Core 8 hosts both in one application — expose REST at the edge, gRPC internally.

---

#### Q16. What is `AddGrpc` used for?

**Answer:** `AddGrpc()` registers gRPC server services, interceptors, and Kestrel configuration needed to host gRPC endpoints in ASP.NET Core 8. Chain `.AddServiceOptions<T>()` for interceptors, compression, and message size limits.

- Called in `Program.cs`: `builder.Services.AddGrpc();`.
- Add `.EnableGrpcWeb()` when browser clients use grpc-web.
- Register interceptors for logging, auth, and exception translation globally or per service.
- Works alongside `AddControllers()` — REST and gRPC share the same DI container and auth configuration.

---

#### Q17. What is `MapGrpcService`?

**Answer:** `MapGrpcService<TService>()` maps a concrete gRPC service implementation to its RPC methods on the Kestrel endpoint routing table. The generated base class from `.proto` defines overrides the application implements.

- `app.MapGrpcService<OrderServiceImpl>();` after `app.Build()`.
- Combine with `RequireHost`, TLS, and authorization policies on the endpoint.
- For grpc-web: `app.MapGrpcService<OrderServiceImpl>().EnableGrpcWeb().RequireCors("spa");`.
- Each service class inherits from the generated `OrderService.OrderServiceBase`.

---

#### Q18. When should you choose gRPC over REST?

**Answer:** Choose gRPC for internal microservice communication where both ends share generated contracts, need low latency, high throughput, streaming, or strict typing — especially .NET-to-.NET or polyglot backends behind a service mesh. Choose REST for public APIs, browser clients, partner integrations, and scenarios requiring HTTP caching and human-readable debugging.

- gRPC: order processing between inventory, payment, and fulfillment services on HTTP/2 with deadlines.
- REST: mobile app and third-party partner APIs documented with OpenAPI and consumed via standard HTTP.
- Hybrid: gRPC inside the cluster; REST or BFF at the API gateway for external consumers.
- Evaluate operational cost — grpc-web, proxies, and protobuf governance add complexity REST avoids at the edge.

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
