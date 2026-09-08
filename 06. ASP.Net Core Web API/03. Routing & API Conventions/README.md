# Routing & API Conventions — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is attribute routing in ASP.NET Core Web API?](#q1-what-is-attribute-routing-in-aspnet-core-web-api)
2. [Q2. What does the `[Route("api/[controller]")]` template mean?](#q2-what-does-the-routeapicontroller-template-mean)
3. [Q3. What is the difference between attribute routing and conventional routing for Web APIs?](#q3-what-is-the-difference-between-attribute-routing-and-conventional-routing-for-web-apis)
4. [Q4. Why is attribute routing preferred for REST APIs?](#q4-why-is-attribute-routing-preferred-for-rest-apis)
5. [Q5. What is the `[controller]` token in a route template?](#q5-what-is-the-controller-token-in-a-route-template)
6. [Q6. What is RESTful route design for resource URLs?](#q6-what-is-restful-route-design-for-resource-urls)
7. [Q7. What is route ambiguity and how can it occur in API routing?](#q7-what-is-route-ambiguity-and-how-can-it-occur-in-api-routing)
8. [Q8. What are nested resource routes?](#q8-what-are-nested-resource-routes)
9. [Q9. What is `RouteOptions.LowercaseUrls`?](#q9-what-is-routeoptionslowercaseurls)
10. [Q10. What is `UsePathBase` and how does it affect API URLs?](#q10-what-is-usepathbase-and-how-does-it-affect-api-urls)
11. [Q11. What do `[HttpGet]`, `[HttpPost]`, etc. specify?](#q11-what-do-httpget-httppost-etc-specify)
12. [Q12. What is the difference between route templates on controller vs action?](#q12-what-is-the-difference-between-route-templates-on-controller-vs-action)
13. [Q13. What is link generation in ASP.NET Core routing?](#q13-what-is-link-generation-in-aspnet-core-routing)
14. [Q14. Why should API routes use nouns instead of verbs?](#q14-why-should-api-routes-use-nouns-instead-of-verbs)
15. [Q15. What is a route constraint (e.g., `{id:int}`)?](#q15-what-is-a-route-constraint-eg-idint)
16. [Q16. What happens when two actions match the same route?](#q16-what-happens-when-two-actions-match-the-same-route)
17. [Q17. What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?](#q17-what-is-the-difference-between-route-at-class-level-vs-httpgetpath-on-action)
18. [Q18. How does `[ApiController]` affect route parameter binding?](#q18-how-does-apicontroller-affect-route-parameter-binding)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is attribute routing in ASP.NET Core Web API?

**Concepts**
- Route template co-located with the action via `[Route]`, `[HttpGet]`, `[HttpPost]`, etc.
- Class-level base prefix combined with action-level segment
- `MapControllers()` — no `MapControllerRoute` needed for pure API projects
- Route constraints, defaults, and named routes supported
- `[ApiController]` requiring attribute routing exclusively

**Answer**

Attribute routing maps URLs to controller actions using `[Route]`, `[HttpGet]`, `[HttpPost]`, and related attributes placed on classes and methods. The route template is co-located with the action it serves, replacing conventional `{controller}/{action}` patterns. The class provides the base prefix and the action appends segments and HTTP method constraints. I register it via `MapControllers()` with no `MapControllerRoute` needed for pure API projects. ASP.NET Core Web API projects use attribute routing exclusively for REST endpoints because `[ApiController]` requires it.

---

## Q2. What does the `[Route("api/[controller]")]` template mean?

**Concepts**
- `[controller]` token replaced by class name minus `Controller` suffix
- `OrdersController` → segment `orders` (combined with `LowercaseUrls`)
- Renaming the class changing the public URL
- `[ControllerName("...")]` to decouple class name from URL segment
- Literal route segment preferred for stable public APIs

**Answer**

This template creates a route prefix `api/` followed by the controller name with the `Controller` suffix removed — `OrdersController` becomes `api/orders`. The `[controller]` token is replaced at runtime by the framework, so renaming `OrdersController` to `ItemsController` changes the public URL from `/api/orders` to `/api/items`, which is a breaking change for all clients. For stable public URLs I use a literal: `[Route("api/orders")]` rather than `[controller]`, or I use `[ControllerName("orders")]` to decouple the URL segment from the class name.

---

## Q3. What is the difference between attribute routing and conventional routing for Web APIs?

**Concepts**
- Attribute routing defining URL per action with HTTP verb constraints
- Conventional routing using centralized templates in `Program.cs`
- Conventional routing exposing action names in URLs and colliding GET actions
- `[ApiController]` requiring attribute routing; conventional routing ignored

**Answer**

Attribute routing defines URLs on each action with attributes, co-locating the contract with the handler. Conventional routing uses centralized route templates in `Program.cs` like `{controller=Home}/{action=Index}/{id?}`, which works for MVC views. Conventional routing exposes action names in URLs — `/Orders/GetOrderById/5` — and makes multiple GET actions on one controller collide since there is no HTTP verb constraint. `[ApiController]` requires attribute routing and ignores conventional routes, so Web APIs use attribute routing exclusively. This gives each action its own precise URL with HTTP method constraints and integrates cleanly with Swagger.

---

## Q4. Why is attribute routing preferred for REST APIs?

**Concepts**
- Resource-oriented URLs declared directly on actions
- HTTP method constraints preventing action collisions
- OpenAPI/Swashbuckle reading attributes for accurate path documentation
- Route constraints disambiguating overlapping templates

**Answer**

Attribute routing expresses resource-oriented URLs directly on actions, supports HTTP method constraints so multiple actions can share a controller without colliding, and integrates cleanly with Swagger and `[ApiController]` binding inference. Templates like `[HttpGet("{id:int}")]` and `[HttpPost]` map cleanly to REST verbs, and each action declares its own URL so there is no collision between multiple GET methods. OpenAPI/Swashbuckle reads attribute routes to generate accurate path documentation. Route constraints like `{id:int}` disambiguate overlapping templates, and teams can apply consistent prefixes via conventions without sacrificing per-action control.

---

## Q5. What is the `[controller]` token in a route template?

**Concepts**
- Placeholder replaced by class name minus `Controller` suffix
- Class rename causing public URL change — breaking contract
- `[ControllerName("...")]` decoupling class name from URL segment
- Literal route segments for public APIs where URL stability matters

**Answer**

The `[controller]` token is replaced with the controller class name minus the `Controller` suffix at route registration time. `ProductsController` produces segment `Products` (or `products` with `LowercaseUrls`). The problem is that renaming the class to `ItemsController` changes the URL from `/api/products` to `/api/items` — a breaking change for all clients. I override this with `[ControllerName("products")]` to decouple the class name from the URL segment, or I use a literal route string `[Route("api/products")]` for public APIs where URL stability across refactors matters.

---

## Q6. What is RESTful route design for resource URLs?

**Concepts**
- Plural nouns for collections; path parameters for identifiers
- HTTP verbs expressing the operation, not path segments
- Query strings for filtering and pagination
- Sub-resources for non-CRUD operations on a parent

**Answer**

RESTful routes use plural nouns for collections, path parameters for identifiers, and HTTP verbs for operations — the path names the resource, the verb names the operation. `GET /api/orders` lists, `POST /api/orders` creates, `GET /api/orders/42` retrieves, `PUT /api/orders/42` replaces, `DELETE /api/orders/42` removes. I reject verbs in paths — `/api/orders/create` or `/api/getOrderById` — because they duplicate method semantics and produce non-standard contracts. Query strings handle filtering and pagination: `GET /api/orders?status=shipped&page=2`. For non-CRUD operations I use noun sub-resources: `POST /api/orders/42/payments` rather than `/api/orders/42/pay`.

---

## Q7. What is route ambiguity and how can it occur in API routing?

**Concepts**
- `AmbiguousMatchException` when two actions match the same HTTP method and URL
- String `{category}` competing with `{id:int}` for numeric values
- Duplicate `[HttpDelete("{id}")]` actions on one controller
- Route constraints, distinct prefixes, or consolidated actions as fixes

**Answer**

Route ambiguity occurs when two or more actions match the same HTTP method and URL pattern, leaving the framework unable to select a single endpoint. A string `{category}` segment matches numeric values like `10`, competing with `{id:int}` — which one wins depends on routing evaluation order and can be inconsistent across deployments. Two `[HttpDelete("{id}")]` actions differing only by parameter type cause ambiguous matches. ASP.NET Core logs ambiguous match warnings at startup or throws `AmbiguousMatchException` at runtime, resulting in HTTP 500. The fix is to add type constraints (`{id:int}`), use distinct path prefixes, or consolidate competing actions into one with internal branching.

---

## Q8. What are nested resource routes?

**Concepts**
- Parent-child URL hierarchy expressing ownership
- Controller route: `[Route("api/customers/{customerId}/orders")]`
- `CreatedAtAction` requiring all parent route values for link generation
- Deep nesting (>2 levels) becoming unwieldy
- Authorization checking parent ownership

**Answer**

Nested resource routes express parent-child relationships in the URL hierarchy — `GET /api/customers/3/orders/7` means order 7 belonging to customer 3. I declare the controller route as `[Route("api/customers/{customerId}/orders")]` with `[HttpGet("{orderId:int}")]` on the action. `CreatedAtAction` must include all parent route values — `customerId` and `orderId` — or link generation produces a wrong `Location` URL with dangling query parameters instead of path segments. Deep nesting beyond two levels becomes unwieldy in URLs and authorization — I balance REST purity with URL readability and typically check parent ownership in authorization policies.

---

## Q9. What is `RouteOptions.LowercaseUrls`?

**Concepts**
- Global setting producing lowercase URL segments from link generation
- Affects `CreatedAtAction`, `LinkGenerator`, and `Url` helpers throughout the app
- Does not change controller class names — only emitted URL segments
- Partner documentation and integration tests must align with the policy

**Answer**

`RouteOptions.LowercaseUrls` is a global configuration that generates all URL paths in lowercase during link generation and route matching. I configure it via `builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true)`. It affects `CreatedAtAction`, `LinkGenerator`, and URL generation throughout the app so `/api/Orders` becomes `/api/orders`. It does not change controller class names — only the emitted URL segments. Partner documentation and integration tests must align with the lowercase policy, because case-sensitive clients that expect `/api/Orders` will receive `404` after enabling it.

---

## Q10. What is `UsePathBase` and how does it affect API URLs?

**Concepts**
- Stripping a path prefix from incoming requests for subpath hosting
- Link generation including path base when request carries it
- `ForwardedHeaders` middleware for correct public scheme and host
- Mismatch between path base and gateway config causing 404s and broken Location headers

**Answer**

`UsePathBase` strips a path prefix from incoming requests so the app can be hosted behind a reverse proxy or sub-path without rewriting every route attribute. For example, a gateway exposes `/api/v1/store/orders/5` but the app routes `/orders/5` — `app.UsePathBase("/api/v1/store")` applied early in the middleware pipeline makes this transparent. `CreatedAtAction` includes the path base when the request carries it, but test clients must also set the path base or `Location` headers will be wrong in integration tests. I combine this with `ForwardedHeaders` middleware for correct public scheme and host in absolute URLs.

---

## Q11. What do `[HttpGet]`, `[HttpPost]`, etc. specify?

**Concepts**
- HTTP verb constraint restricting which method matches the action
- Optional route template appended to the controller base
- `405 Method Not Allowed` for wrong verb on a matched route
- API controllers requiring one HTTP method attribute per action

**Answer**

HTTP method attributes constrain which HTTP verb matches an action and optionally append a route template segment. `[HttpGet]` on an action with no template matches the controller base route. `[HttpGet("{id:int}")]` adds `{id}` and restricts to GET requests — a POST to the same URL returns `405 Method Not Allowed`. I give each action exactly one HTTP method attribute; a bare `[Route]` without a verb attribute does not expose the action in API controllers. Together with the controller's `[Route]` prefix, these form the complete route pattern.

---

## Q12. What is the difference between route templates on controller vs action?

**Concepts**
- Controller-level `[Route]` as shared prefix for all actions
- Action-level `[HttpGet("...")]` appending relative segments and HTTP method constraint
- `~/` or leading `/` on action template overriding the controller prefix
- Version and prefix at controller level avoiding repetition across actions

**Answer**

The controller-level `[Route]` sets the shared prefix for all actions in that class — `[Route("api/v1/[controller]")]` gives every action a base of `/api/v1/orders`. Action-level `[HttpGet("...")]` templates append relative segments and bind the action to a specific HTTP verb, forming the complete pattern — `[HttpGet("{id:int}")]` produces `GET /api/v1/orders/{id}`. Action templates starting with `~/` or `/` override the controller prefix entirely and use an absolute path from the application root. I keep version and prefix at controller level to avoid repeating them on every action.

---

## Q13. What is link generation in ASP.NET Core routing?

**Concepts**
- `LinkGenerator` service creating URLs from action names and route values
- Powering `CreatedAtAction`, `CreatedAtRoute`, `Url.Action`, and hypermedia links
- Respecting route constraints, lowercase URL options, and path base settings
- Named routes decoupling link generation from method renames
- Hard-coded URL strings breaking behind gateways and path base changes

**Answer**

Link generation creates URLs from route names, action names, and route values using the routing system's `LinkGenerator` service. It powers `CreatedAtAction`, `CreatedAtRoute`, `Url.Action`, and hypermedia links in responses. Given an action name and route values like `{ id = 5, tenantId = 3 }`, it produces the complete path `/api/tenants/3/orders/5` respecting route constraints, lowercase URL options, and path base settings. Broken link generation — wrong action name, missing route value — produces incorrect `Location` headers that return `404` on follow-up. Named routes via `Name = "GetOrder"` decouple link generation from method renames.

---

## Q14. Why should API routes use nouns instead of verbs?

**Concepts**
- Nouns identifying resources; HTTP methods expressing the operation
- Verb-in-URL duplicating method semantics — RPC not REST
- Gateway policies, rate limits, and monitoring applying per HTTP method on resource paths
- OpenAPI grouping operations by resource path

**Answer**

Nouns identify resources and HTTP methods express the operation — combining verbs in URLs duplicates method semantics and produces RPC-style endpoints that break caching, idempotency, and standard tooling. `DELETE /api/orders/42` is cacheable-safe and idempotent by HTTP semantics; `/api/orders/cancel/42` with POST carries no such guarantee and cannot be reasoned about by gateways or monitors. OpenAPI groups operations by resource path so verb-named paths produce clutter. For non-CRUD operations I use noun sub-resources — `/api/orders/42/cancellations` — rather than embedding the verb in the collection path.

---

## Q15. What is a route constraint (e.g., `{id:int}`)?

**Concepts**
- Restricting route parameter values to specific types or patterns
- `{id:int}` matching only integers, returning `404` for non-integers
- Disambiguating overlapping templates where string and int compete
- Custom `IRouteConstraint` for domain-specific rules

**Answer**

A route constraint restricts which values match a route parameter — `{id:int}` accepts only integers and returns `404` for non-numeric values rather than hitting the wrong action. Without constraints, a string `{category}` segment competes with `{id:int}` for numeric paths and wins unpredictably. `{slug:alpha}` restricts to letters, `{date:datetime}` validates date format. I implement `IRouteConstraint` for domain-specific rules when built-in constraints are insufficient. Constraints appear in attribute routes: `[HttpGet("{id:int}")]` on the action, and they are also validated at startup when `ValidateOnBuild` is configured.

---

## Q16. What happens when two actions match the same route?

**Concepts**
- `AmbiguousMatchException` thrown at runtime; HTTP 500
- Common cause: identical `[HttpGet("{id}")]` on same controller
- Swagger showing duplicate operations before runtime failure
- Fix: add constraints, merge into one action, or use different path prefixes

**Answer**

ASP.NET Core cannot dispatch the request to a single action and throws `AmbiguousMatchException` at runtime, resulting in HTTP 500. Two actions with identical `[HttpGet("{id}")]` on the same controller are the most common cause. Swagger may reveal the problem earlier by showing duplicate operations. The fix is to add type constraints — `{id:int}` vs `{id:guid}` — merge the actions into one with internal branching, or use different path prefixes. Code review should reject duplicate HTTP method and template combinations on one controller before they reach production.

---

## Q17. What is the difference between `[Route]` at class level vs `[HttpGet("path")]` on action?

**Concepts**
- Class-level `[Route]` as shared URL prefix for all actions
- Action-level `[HttpGet("path")]` adding segment and binding HTTP verb
- Bare `[Route]` on action without HTTP method attribute not creating an endpoint
- HTTP method attributes carrying optional route templates

**Answer**

Class-level `[Route]` defines the shared URL prefix for the controller — all actions inherit it. Action-level `[HttpGet("path")]` adds a relative segment and binds the action to the GET verb — `[HttpGet("active")]` produces `GET /api/orders/active`. A bare `[Route("api/orders")]` on an action without an HTTP method attribute does not create an endpoint in API projects because there is no verb constraint. HTTP method attributes carry optional route templates; I use them for per-action segments while keeping the shared prefix at controller level.

---

## Q18. How does `[ApiController]` affect route parameter binding?

**Concepts**
- Binding source inference: route parameters → `[FromRoute]`, complex types → `[FromBody]`, simple types not in route → `[FromQuery]`
- `[FromServices]` still requiring explicit attribute
- Misunderstanding inference causing silent null values
- Inference reducing boilerplate while requiring careful understanding

**Answer**

`[ApiController]` enables binding source inference — simple types from route templates bind as `[FromRoute]`, complex types from the body bind as `[FromBody]`, and simple query parameters bind as `[FromQuery]` without explicit attributes. `Get(int id)` with `{id}` in the route template is bound from route automatically; `Post(OrderDto dto)` is bound from the request body automatically. `[FromServices]` must still be explicit for service injection into action parameters. Misunderstanding inference causes silent `null` values — for example, a simple type not in the route template defaults to query string binding, not body binding, which surprises developers who expected `[FromBody]` behavior.

---

## Gotchas — Routing & API Conventions (Interview Traps)

---

#### Gotcha 1. `[controller]` token changes when class is renamed

**Concepts**
- `[Route("api/[controller]")]` derives the URL segment from the class name at startup
- Renaming `OrdersController` to `PurchasesController` changes public URL to `/api/purchases`
- Partner integrations and OpenAPI clients silently break
- Pin with literal route or `[ControllerName("orders")]` to decouple

**Answer**

`[Route("api/[controller]")]` inserts the controller class name (minus the "Controller" suffix) into the URL at application startup, so renaming the class to `PurchasesController` changes every public endpoint URL from `/api/orders` to `/api/purchases` with zero compile-time warning. Every consumer that hard-coded or code-generated client URLs from the previous contract receives 404. I pin the URL with a literal route — `[Route("api/orders")]` — or apply `[ControllerName("orders")]` to decouple the route segment from the class name, and always run integration tests that assert endpoint URLs before any rename reaches production.

---

#### Gotcha 2. Ambiguous route templates — string parameter swallowing integer routes

**Concepts**
- `{id:int}` constraint — matches only numeric values
- `{category}` — matches any string including numeric strings like `"10"`
- Two competing templates for same path shape causes intermittent wrong action hit
- Route resolution order — literal segments beat parameterized; constrained beats unconstrained

**Answer**

`[HttpGet("{id:int}")]` and `[HttpGet("{category}")]` on the same controller both match `GET /api/products/10` because `"10"` is a valid string. Route resolution does not guarantee which action wins when both templates match the same path shape, so the wrong action executes intermittently depending on registration order. The fix is to use distinct path levels: `GET /api/products/{id:int}` for by-id and `GET /api/products/categories/{category}` for by-category so the templates never compete.

---

#### Gotcha 3. Action-level absolute route overriding class-level prefix

**Concepts**
- Template starting with `/` or `~/` is absolute — ignores class-level `[Route]` prefix
- Template without leading slash is relative — appended to class prefix
- Absolute route on action unintentionally bypasses versioned prefix
- Duplicate prefix bug: class `api/[controller]` + action `api/orders/{id}` produces doubled prefix

**Answer**

An action-level route template that begins with `/` or `~/` registers as an absolute path, completely ignoring the class-level `[Route]` prefix. This is useful for legacy alias routes but is a common bug when a developer copies a full path from documentation into the attribute: the route registers correctly but duplicates or contradicts the class prefix, producing URLs that differ from what the controller's other actions expose. Conversely, appending a redundant `api/orders/...` as a relative template doubles the `api` prefix. I always use relative templates on actions — path segments only, no leading slash — and rely on the class-level `[Route]` for the common prefix.

---

#### Gotcha 4. Route constraint syntax errors silently preventing endpoint registration

**Concepts**
- Route constraint syntax: `{id:int}`, `{id:guid}`, `{id:minlength(3)}`
- Typo in constraint — `{id:integer}` — causes endpoint to not register, returning 404
- No startup error thrown for unknown constraint names
- `int` vs `long` vs `guid` vs `regex(...)` available constraints

**Answer**

A typo in a route constraint — `{id:integer}` instead of `{id:int}`, or `{name:minLen(2)}` instead of `{name:minlength(2)}` — causes the endpoint to silently fail to register, returning 404 for all requests to that path. ASP.NET Core does not throw at startup for unknown constraint names by default. I validate route constraints in integration tests that assert every expected endpoint is reachable, and I use the exact constraint names from the documentation: `int`, `long`, `guid`, `bool`, `datetime`, `decimal`, `double`, `float`, `minlength(n)`, `maxlength(n)`, `length(n)`, `min(n)`, `max(n)`, `range(n,m)`, `alpha`, `regex(expr)`.

---

#### Gotcha 5. Conventional routing used in a `[ApiController]` project

**Concepts**
- `[ApiController]` requires attribute routing — ignores `MapControllerRoute`
- Conventional routing designed for MVC actions with view names in URLs
- `MapControllers()` vs `MapControllerRoute(...)` — different endpoint registration
- Attribute routing expresses the full contract co-located with the action

**Answer**

Conventional routing via `MapControllerRoute` is designed for MVC view controllers where the action name appears in the URL and a single route template covers many controller/action combinations. For `[ApiController]` Web API controllers, conventional routing is ignored — the attribute routes on the controller class and action methods are the only registration. Attempting to reach an `[ApiController]` endpoint via a conventional route template returns 404 because the framework requires attribute routing when `[ApiController]` is present. I use `app.MapControllers()` (no template argument) for Web API projects and `MapControllerRoute` only for MVC view areas.

---

#### Gotcha 6. `CreatedAtAction` with missing parent route values in nested resources

**Concepts**
- Nested resource route: `api/customers/{customerId}/orders/{orderId}`
- All route parameters needed in the anonymous object — missing one appends as query string
- `CreatedAtAction(nameof(Get), new { orderId = order.Id }, dto)` — missing `customerId`
- Correct: `new { customerId, orderId = order.Id }`

**Answer**

`CreatedAtAction` uses the routing system to generate the `Location` header URL. For a nested route like `api/customers/{customerId}/orders/{orderId}`, both `customerId` and `orderId` must be supplied in the route values anonymous object. Omitting `customerId` causes link generation to fall back to appending it as a query string — the `Location` header becomes `api/customers/{customerId}/orders?orderId=7` instead of `api/customers/3/orders/7`. I include every route parameter from the target action's template in the `CreatedAtAction` call, and assert the `Location` header with a follow-up GET in integration tests.

---

#### Gotcha 7. Route ordering — literal vs parameterized segments at same depth

**Concepts**
- Literal segments have higher priority than parameterized segments in ASP.NET Core routing
- `GET /api/products/featured` matches the literal `featured` action first
- Two unconstrained parameterized templates at same depth still compete
- Ordering guarantees: literals beat parameters; constrained parameters beat unconstrained

**Answer**

ASP.NET Core's routing engine gives literal segments higher priority than parameterized ones, so `[HttpGet("featured")]` correctly takes priority over `[HttpGet("{id:int}")]` for `GET /api/products/featured`. However, `[HttpGet("{id:int}")]` and `[HttpGet("{category}")]` compete for the same request pattern because both are parameterized at the same depth — a numeric string like `"10"` satisfies both. Constraints narrow the match set but cannot disambiguate two unconstrained string parameters at the same position. The reliable fix is to give each variant a distinct URL shape.

---

#### Gotcha 8. Lowercase URL policy breaking link generation

**Concepts**
- `RouteOptions.LowercaseUrls = true` — lowercases path segments in generated URLs
- `CreatedAtAction` and `LinkGenerator` apply the policy to generated `Location` URLs
- Path base not included unless `UsePathBase` is registered before routing
- Reverse proxy adding path prefix that `LinkGenerator` does not know about

**Answer**

`LowercaseUrls = true` in `RouteOptions` applies to URL generation only, not to routing match — so `GET /api/Orders/5` still resolves to the action even though the generated `Location` header says `/api/orders/5`. The problem is that link generation does not automatically include path bases added by `UsePathBase` unless that middleware runs before the request reaches routing. Integration tests that create a `WebApplicationFactory` without configuring the path base produce generated URLs without the prefix, masking the production bug where a gateway adds `/api/v1` as the base path.

---

#### Gotcha 9. `MapControllers()` vs `MapDefaultControllerRoute()` — order matters

**Concepts**
- `MapControllers()` — registers only attribute-routed `[ApiController]` endpoints
- `MapDefaultControllerRoute()` — adds a conventional `{controller=Home}/{action=Index}` template
- Endpoint registration order determining fallback for ambiguous paths
- Adding `MapDefaultControllerRoute()` to a pure API project causing unexpected shadows

**Answer**

`MapControllers()` registers endpoints for `[ApiController]`-decorated controllers using their attribute route templates. Calling `MapDefaultControllerRoute()` in addition registers a conventional catch-all template that can shadow or conflict with attribute routes if registered in the wrong order. For a pure Web API project, only `MapControllers()` should be called; adding `MapDefaultControllerRoute()` is a leftover from an MVC template and can cause unexpected 404s or wrong-action matches when a URL happens to match both the conventional template and an attribute route.

---

#### Gotcha 10. `[HttpGet]` and `[Route]` on the same action producing double route registration

**Concepts**
- `[Route("items")]` on action + `[HttpGet]` — two separate routes registered
- One registers without HTTP method constraint; one registers GET only
- POST to the non-HTTP-method-constrained route — action reached unexpectedly
- Use only `[HttpGet("items")]` for combined method + route

**Answer**

Placing both `[Route("items")]` and `[HttpGet]` on the same action registers two routes: one that matches any HTTP method and one that matches only GET. A POST request to that URL reaches the action through the method-unconstrained `[Route]` registration even though the developer intended GET-only. I use the combined `[HttpGet("path")]`, `[HttpPost("path")]`, etc. attributes which declare both the HTTP method and the route template in one attribute, and avoid placing a bare `[Route]` attribute on actions in API controllers.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this controller after a rename from `CustomerController` to `ClientsController`. Partner calls to `POST /api/customers` return 404; Swagger still lists `/api/Clients`.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    [HttpPost]
    public ActionResult<ClientDto> Create([FromBody] CreateClientDto dto) { /* ... */ }
}
```

Partners were onboarded with lowercase `/api/customers` from the old controller name. No `[Route("api/customers")]` override exists.

---

**Answer**

`[Route("api/[controller]")]` derives the URL segment from the class name — renaming the class to `ClientsController` changed the public URL to `/api/clients` (or `/api/Clients` without lowercase URLs) without any backward-compatibility shim. All partners onboarded to `/api/customers` now receive `404`. The fix is to pin the route with a literal: `[Route("api/customers")]` on the controller class, or combine `[Route("api/[controller]")]` with `[ControllerName("customers")]` to decouple the URL segment from the class name. If the rename to `clients` is intentional and should eventually be the public URL, I keep `/api/customers` as a deprecated alias via a thin wrapper controller calling the same service layer, with an OpenAPI sunset date published in the contract. Contract tests asserting partner URLs must run before any rename reaches production.

---

#### Q2. (R) Review ambiguous attribute routes. `GET /api/products/10` intermittently hits the wrong action depending on build order.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok(_repo.Get(id));

    [HttpGet("{category}")]
    public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

    [HttpGet("featured")]
    public IActionResult Featured() => Ok(_repo.Featured());
}
```

Request `GET /api/products/featured` works; `GET /api/products/10` sometimes binds `category = "10"`.

---

**Answer**

`{category}` is a string parameter that matches any value, including numeric strings like `"10"`, so it competes with `{id:int}` for requests like `GET /api/products/10`. When routing evaluation order favors the string template, the numeric value binds as `category = "10"` and the wrong action executes. The behavior is inconsistent across deployments because it depends on route ordering that is not guaranteed. The literal `featured` does not have this problem because a literal segment takes priority over a parameterized one. The correct fix is to use distinct path levels — `GET /api/products/{id:int}` for by-id and `GET /api/products/categories/{category}` for by-category — so the two templates never compete for the same path shape. Alternatively, constrain the category path explicitly: `[HttpGet("category/{category}")]`.

---

#### Q3. (R) Review RESTful route design for a code review. Which templates would you reject before merge?

```csharp
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost("GenerateInvoice")]
    public IActionResult Generate([FromBody] GenerateDto dto) => Ok();

    [HttpGet("GetInvoice/{invoiceId}")]
    public IActionResult GetInvoice(int invoiceId) => Ok();

    [HttpPost("{invoiceId}/SendEmail")]
    public IActionResult SendEmail(int invoiceId) => Ok();
}
```

---

**Answer**

All three templates embed verb phrases in the URL, which I reject before merge. `GenerateInvoice` duplicates what `[HttpPost]` already expresses — the correct path is `POST /api/invoices` and the verb in the URL adds nothing. `GetInvoice` is a GET action so the word "Get" is redundant — the correct path is `GET /api/invoices/{invoiceId}`. `SendEmail` is a side-effecting operation that does not map cleanly to a REST resource — I would name it `POST /api/invoices/{invoiceId}/email` or `POST /api/invoices/{invoiceId}/notifications` so it uses a noun sub-resource. The `[Route("api/[controller]")]` prefix without missing `[ApiController]` is also a concern since validation auto-responses would be absent. The review comment: remove all verb segments from route templates and use noun-based paths with HTTP method semantics.

---

#### Q4. (M) After enabling lowercase URLs (`RouteOptions.LowercaseUrls = true`), `CreatedAtAction` generates `Location: /api/orders/5` but the gateway publicly exposes `/api/v1/store/orders/5`. What routing and link-generation pieces are missing?

```csharp
// Program.cs excerpt
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
var app = builder.Build();
app.UsePathBase("/api/v1/store");
app.MapControllers();
```

---

**Answer**

`UsePathBase("/api/v1/store")` is correctly placed before `MapControllers()`, so incoming requests strip the prefix and routing works. The problem is that `CreatedAtAction` generates `Location: /api/orders/5` without the path base because `LinkGenerator` includes path base only when the current request carries it — integration tests that create a `TestServer` without setting `RequestPathBase` will generate URLs without the prefix. In production, if the reverse proxy correctly sets `X-Forwarded-For` and `X-Forwarded-Prefix`, I also need `app.UseForwardedHeaders()` before `UsePathBase` so the absolute URL in `Location` uses the public host and scheme rather than the internal pod address. For integration tests, I configure the `HttpClient.BaseAddress` to include the path base, or I set `Request.PathBase` explicitly on the test request. The lowercase URL option is working correctly — the missing piece is path base propagation through `ForwardedHeaders` middleware for production and test client configuration for integration tests.

---

#### Q5. (D) A junior developer proposes conventional routing for a new public JSON API because "Startup.cs tutorials use `MapControllerRoute`." Argue for or against attribute routing for Web APIs in ASP.NET Core 8.

---

**Answer**

Conventional routing is designed for MVC views where action names appear in URLs and a single catch-all template covers many controllers. For a public JSON API, conventional routing exposes implementation names — `/Orders/GetOrderById/5` — because the action name is part of the URL, and multiple GET actions on one controller collide since conventional routes do not distinguish HTTP verbs. Attribute routing with `[ApiController]` is the correct choice because each action declares its own resource URL with HTTP verb constraints, OpenAPI/Swashbuckle reads the attributes directly to generate accurate path documentation, route constraints disambiguate overlapping templates, and the contract is co-located with the action so changes are visible in the same file. Conventional routing is appropriate for legacy MVC admin areas or Razor page surfaces — not for greenfield REST APIs. The tutorial pattern the developer references uses `MapControllerRoute` for MVC view routing, not for JSON APIs. For pure API projects I use `MapControllers()` only, with no `MapControllerRoute`.

---

#### Q6. (R) Review `[ApiController]` route prefix behavior. Integration tests call `/api/v1/orders` but receive 404 — unit tests on the controller pass.

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult Get(int id) => Ok(_orders.Get(id));
}

// Test project helper:
var client = _factory.CreateClient();
var response = await client.GetAsync("/api/orders/1");
```

---

**Answer**

The controller route template is `api/v1/[controller]` which resolves to `/api/v1/orders`, but the test client calls `/api/orders/1` — missing the `v1` segment. Unit tests invoke the action method directly and bypass routing entirely, so they pass regardless of what the route template says. The fix is to update the test URL to `GetAsync("/api/v1/orders/1")`. The broader lesson is that unit tests cannot catch routing mismatches — only `WebApplicationFactory` integration tests that send real HTTP requests through the full pipeline can verify that a URL reaches the intended action. I add an integration test asserting every public endpoint URL before release, using the exact path from the route attribute as the canonical source of truth. If the team wants a centralized version prefix rather than repeating `v1` on every controller, I configure it via `options.Conventions.Add(...)` in `AddControllers`.

---

#### Q7. (R) Review duplicate HTTP method registration. Swagger shows two operations for `DELETE /api/items/{id}`; one returns 405 in production.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) { _repo.Delete(id); return NoContent(); }

    [HttpDelete("{id}")]
    public IActionResult Remove(string id) { _repo.Delete(int.Parse(id)); return NoContent(); }
}
```

---

**Answer**

Two DELETE actions differ only by parameter type — `{id:int}` and `{id}` — for the same template shape. When a numeric ID arrives, both templates can match, causing an ambiguous route resolution that returns `405 Method Not Allowed` intermittently. Swagger shows duplicate operation IDs which can break client code generation. The string overload also hides an error — `int.Parse(id)` throws `FormatException` on non-numeric input rather than returning `404`. The fix is a single `[HttpDelete("{id:int}")]` action with the integer constraint, removing the string overload entirely. If the API must accept non-integer IDs — GUID, slug — I design a single action with an appropriate constraint and handle non-existent resources with `NotFound()` rather than a parse exception.

---

#### Q8. (R) Review nested resource routes and link generation for a parent/child REST surface.

```csharp
[Route("api/customers/{customerId:int}/orders")]
[ApiController]
public class CustomerOrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<OrderDto> Create(int customerId, [FromBody] CreateOrderDto dto)
    {
        var order = _svc.Create(customerId, dto);
        return CreatedAtAction(nameof(Get), new { orderId = order.Id }, order);
    }

    [HttpGet("{orderId:int}")]
    public ActionResult<OrderDto> Get(int customerId, int orderId) => Ok(_svc.Get(customerId, orderId));
}
```

`CreatedAtAction` Location returns `/api/customers/3/orders?orderId=7` without the order segment.

---

**Answer**

`CreatedAtAction` must include all route parameters that appear in the target action's template — the GET action at `{orderId:int}` requires both `customerId` and `orderId` in the path. Since `customerId` is missing from the `new { orderId = order.Id }` route values, link generation cannot populate the `{customerId:int}` segment and falls back to appending `orderId` as a query string. The fix is `return CreatedAtAction(nameof(Get), new { customerId, orderId = order.Id }, order.ToDto())` — both parent and child identifiers in the route values. An alternative is to name the GET route explicitly with `[HttpGet("{orderId:int}", Name = "GetCustomerOrder")]` and use `CreatedAtRoute("GetCustomerOrder", new { customerId, orderId = order.Id }, dto)` for a more rename-stable reference. An integration test that POSTs a new order and then GETs the URL from the `Location` header should pass before merge — this kind of link generation bug only surfaces through the full routing pipeline.

---
