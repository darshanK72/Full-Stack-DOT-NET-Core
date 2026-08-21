# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/03. Routing & API Conventions`

---

#### Q1. (R) Review this controller after a rename from `CustomerController` to `ClientsController`. Partner calls to `POST /api/customers` return 404; Swagger still lists `/api/Clients`.

**Answer:** `[Route("api/[controller]")]` derives the segment from the class name — renaming to `ClientsController` changed the public URL to `/api/clients` without a compatibility shim. Production APIs need explicit stable routes or versioning when renaming controllers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `[controller]` token replaced `customers` with `clients` | Breaking change for all integrated partners |
| Conventions | Lowercase URL option may emit `/api/clients` vs partner `/api/customers` | Case-sensitive clients 404 |
| Documentation | Swagger reflects new name only | Partner docs diverge |
| Design | Resource name tied to C# class name | Refactors become contract changes |

**Fix (priority order):**

1. Pin route: `[Route("api/customers")]` on controller (or `[Route("api/[controller]")]` + `[ControllerName("customers")]`).
2. Keep `/api/clients` as deprecated alias during migration if rename is intentional.
3. Add contract tests asserting partner URLs before release.

**Production takeaway:** **`[controller]` is convenient, not stable** — public resource names should be explicit in route templates.

---

#### Q2. (R) Review ambiguous attribute routes. `GET /api/products/10` intermittently hits the wrong action depending on build order.

**Answer:** `{category}` is a catch-all string segment that competes with `{id:int}` — numeric slugs bind as category name `"10"` when that template wins ordering. Literal segments like `featured` must be declared, and overlapping parameter routes need constraints or consolidation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `{category}` matches any string including numeric | `10` routed to category action instead of by-id |
| Routing | Ambiguous template precedence | Intermittent behavior across deployments |
| REST design | Category and id share one path level | Model confusion — prefer `/categories/{name}` vs `/{id:int}` |
| API contract | Clients cannot predict response shape | Same URL returns category list or single product |

**Fix (priority order):**

1. Split routes: `GET /api/products/{id:int}` and `GET /api/products/categories/{category}`.
2. Or constrain category: `[HttpGet("category/{category}")]` for non-numeric categories only.
3. Register literal `featured` before parameterized templates (still prefer explicit paths).

**Production takeaway:** Attribute routing **requires explicit disambiguation** — int constraints alone do not fix string route competition.

---

#### Q3. (R) Review RESTful route design for a code review. Which templates would you reject before merge?

**Answer:** Verb phrases in URLs (`GenerateInvoice`, `GetInvoice`, `SendEmail`) are RPC-style and duplicate HTTP method meaning — reject in favor of noun-based resources and proper verbs on standard paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| REST | Verbs in path (`GenerateInvoice`, `GetInvoice`) | Non-standard; breaks HTTP verb semantics |
| Consistency | Mixed prefix patterns on actions | OpenAPI clutter; harder gateway policies |
| Design | POST for read-like `GetInvoice` path with GET verb missing | Wrong idempotency expectations |
| Maintainability | Action names embedded in URLs | URL churn when refactoring actions |

**Fix (priority order):**

1. `POST /api/invoices` (generate/create), `GET /api/invoices/{invoiceId}`, `POST /api/invoices/{invoiceId}/email` or sub-resource `/notifications`.
2. Remove redundant verb segments from templates.
3. Document side-effect sub-resources explicitly in OpenAPI.

**Production takeaway:** Code review here tests **resource naming judgment**, not memorizing `[Route]` syntax.

---

#### Q4. (M) After enabling lowercase URLs (`RouteOptions.LowercaseUrls = true`), `CreatedAtAction` generates `Location: /api/orders/5` but the gateway publicly exposes `/api/v1/store/orders/5`. What routing and link-generation pieces are missing?

**Answer:** `UsePathBase` sets the request path base for matching but link generation must include that path base (and forwarded headers for public scheme/host) — configure `HttpContext.Request.PathBase` awareness via middleware order and optionally `LinkGenerator` base address for absolute URLs.

- **`UsePathBase("/api/v1/store")`:** Must run early so routing and link generation see the path base — verify order before `MapControllers`.
- **Location header:** `CreatedAtAction` may omit path base if request PathBase not set on test client — integration tests must set `Request.Path` + PathBase or use `CreateClient` with `BaseAddress`.
- **Gateway:** Configure `ForwardedHeaders` so absolute URLs use public host/scheme, not internal pod URL.
- **Options:** `services.Configure<RouteOptions>` lowercase affects generated URLs — ensure partner docs use same casing policy.
- **Fix:** `Created(uri, value)` with `Url.Action`/`LinkGenerator.GetUriByAction` including path base, or set client BaseAddress in tests to match gateway.

**Production takeaway:** **Route templates and public URLs diverge** behind path bases — link generation is a production routing topic, not just controller attributes.

---

#### Q5. (D) A junior developer proposes conventional routing for a new public JSON API because "Startup.cs tutorials use `MapControllerRoute`." Argue for or against attribute routing for Web APIs in ASP.NET Core 8.

**Answer:** Public Web APIs should use attribute routing with `[ApiController]` — conventional `{controller}/{action}` routes expose implementation names, complicate RESTful URLs, and fight OpenAPI grouping; conventional routing remains for MVC views, not JSON APIs.

- **Against conventional for APIs:** URLs like `/Orders/GetOrderById` leak action names; HTTP method constraints are weaker; multiple GET actions collide.
- **For attribute routing:** Co-locate template with action; express REST resources; `[HttpGet("{id}")]` clarity; API explorer/Swagger reads attributes directly.
- **When conventional appears:** Legacy MVC apps or admin areas — not greenfield REST APIs.
- **Hybrid:** `MapControllers()` only — no `MapControllerRoute` needed for pure API projects.
- **Team norm:** Document `[Route("api/v1/[controller]")]` at assembly or use `AddControllers(options => options.Conventions.Add(...))` for prefix consistency.

**Production takeaway:** Karat wants **convention choice reasoning** — attribute routing is the production default for ASP.NET Core Web APIs.

---

#### Q6. (R) Review `[ApiController]` route prefix behavior. Integration tests call `/api/v1/orders` but receive 404 — unit tests on the controller pass.

**Answer:** The controller route includes `v1` but the test client requests `/api/orders/1` — missing version segment. `[ApiController]` does not infer global prefixes; the test must match the attribute template exactly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Testing | Wrong URL in integration test | False confidence from passing unit tests |
| Routing | Template is `api/v1/[controller]` | `/api/orders` does not match |
| Process | Unit tests invoke action directly | Bypass routing — misses template bugs |
| Versioning | v1 in route requires client update | Partners must include version in path |

**Fix (priority order):**

1. Update test: `GetAsync("/api/v1/orders/1")`.
2. Add global route prefix via `options.Conventions` if team wants centralized version segment.
3. Prefer `WebApplicationFactory` routing tests for every public endpoint path.

**Production takeaway:** **Unit tests without routing lie** — API conventions must be verified through HTTP with correct templates.

---

#### Q7. (R) Review duplicate HTTP method registration. Swagger shows two operations for `DELETE /api/items/{id}`; one returns 405 in production.

**Answer:** Two DELETE actions differ only by parameter type (`int` vs `string`) on the same template — routing cannot stably choose, causing ambiguous match or build-time warnings. Keep one action and parse or constrain the parameter.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | Duplicate `[HttpDelete("{id}")]` templates | Ambiguous endpoint selection |
| Runtime | One match may 405 Method Not Allowed | Unpredictable delete behavior |
| Design | `int.Parse` in string action | 500 on non-numeric id instead of 404 |
| OpenAPI | Duplicate operation IDs | Client generation fails |

**Fix (priority order):**

1. Single `[HttpDelete("{id:int}")]` action.
2. Remove string overload; use route constraint for int ids.
3. Regenerate Swagger with unique operation names.

**Production takeaway:** **One endpoint, one handler** — duplicate attribute routes are a merge blocker.

---

#### Q8. (R) Review nested resource routes and link generation for a parent/child REST surface.

**Answer:** `CreatedAtAction` must include **all** route parameters matching the GET template — `orderId` belongs in the path `{orderId:int}`, not as a dangling query value because `nameof(Get)` target expects both `customerId` and `orderId` in the path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Link generation | Missing `customerId` or wrong param name in `CreatedAtAction` values | Location URL missing `{orderId}` segment |
| Routing | Generated `/api/customers/3/orders?orderId=7` | GET expects `/api/customers/3/orders/7` |
| REST | Broken nested resource URI | Clients cannot follow Location |
| Testing | No assertion on Location path shape | Regression on create flows |

**Fix (priority order):**

1. `return CreatedAtAction(nameof(Get), new { customerId, orderId = order.Id }, order.ToDto());`
2. Or name the GET route: `[HttpGet("{orderId:int}", Name = "GetCustomerOrder")]` and use `CreatedAtRoute`.
3. Integration test: POST then GET Location URL returns 200.

**Production takeaway:** Nested REST routes multiply **route value requirements** for link generation — every parent segment must appear in `CreatedAtAction` values.
