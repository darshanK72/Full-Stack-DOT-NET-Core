# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/02. API Controllers & Action Results`

---

#### Q1. (R) Review this controller action under load. Integration tests pass locally; production threads spike and requests time out under concurrent traffic.

**Answer:** Blocking on `.Result` inside a synchronous action captures the request thread while waiting for async I/O — under load this causes thread-pool starvation and cascading timeouts. The action must be `async Task<ActionResult<T>>` with `await` end-to-end.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GenerateAsync` | Thread-pool starvation; classic ASP.NET Core production failure |
| API design | Sync action wrapping async service | Misleading signature hides blocking behavior |
| Scalability | Each request holds a thread during report generation | Kestrel queue grows; health checks fail |
| Testing | Low concurrency tests miss deadlock/starvation | Passes CI, fails peak traffic |

**Fix (priority order):**

1. Change to `public async Task<ActionResult<ReportDto>> Get(int id)` and `return Ok(await _reportService.GenerateAsync(id));`
2. Ensure service method stays async through database and file I/O — no sync-over-async deeper in the stack.
3. Add load test or parallel integration test to CI for hot endpoints.

**Production takeaway:** See debrief async snippet — **sync controller + `.Result`** is one of the most common Karat production traps. See C# Module 06 — async all the way.

---

#### Q2. (R) Review resource creation. Mobile clients create accounts successfully but cannot find the new user URI for subsequent PATCH calls — OpenAPI documents `201` with a Location header.

**Answer:** Returning `200 OK` with a body omits the `Location` header clients use to discover the canonical resource URI — create operations should return `201 Created` via `CreatedAtAction` or `CreatedAtRoute` pointing at the GET-by-id action.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | `Ok(user)` on create returns `200` | Violates REST create convention; mobile SDK expects `201` |
| Contract | No `Location` header | Clients cannot link to `/api/users/{id}` without parsing body |
| Caching / semantics | `200` implies existing representation | Intermediaries may cache incorrectly |
| OpenAPI drift | Docs promise `201` | Partner integration tests fail in CI |

**Fix (priority order):**

1. `return CreatedAtAction(nameof(GetById), new { id = user.Id }, userDto);` — rename action consistently.
2. Return a response DTO, not EF entity with navigation properties.
3. Align Swagger response metadata with `[ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]`.

**Production takeaway:** Debrief explicitly calls out **201 + Location** — this is contract hygiene, not ceremony.

---

#### Q3. (R) Review `CreatedAtAction` usage. After deploy, the Location header points to `GET /api/users/GetUser/5` which returns 404.

**Answer:** `CreatedAtAction` resolves the target action by name — `nameof(GetUser)` does not match `GetById`, so link generation targets a non-existent action or wrong route template. Action names, route values, and HTTP verb must align with an existing GET endpoint.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | Wrong action name in `CreatedAtAction` | Location URL 404 — clients cannot fetch created resource |
| API | Mismatch between `GetUser` vs `GetById` | Compile-time string would fail; `nameof` hid wrong symbol if renamed |
| Contract | Broken hypermedia without HATEOAS | Even minimal REST expects working Location |
| Testing | Missing assertion on Location header in integration tests | Bug ships to mobile |

**Fix (priority order):**

1. `return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);`
2. Verify route values include all template parameters (`id` matches `[HttpGet("{id:int}")]`.
3. Add `WebApplicationFactory` test asserting `Response.Headers.Location` and follow-up GET succeeds.

**Production takeaway:** **`CreatedAtAction` is link generation** — broken names are production 404s, not refactor nits.

---

#### Q4. (P) When should a production Web API action return `ActionResult<T>` vs `IActionResult`, and how does that choice affect OpenAPI schema generation and unit testing?

**Answer:** Prefer `ActionResult<T>` when success returns a typed body and you still need `NotFound`, `BadRequest`, or other results — Swagger documents the success type while allowing multiple status codes; use bare `IActionResult` when return shape varies wildly or is non-generic.

- **`ActionResult<T>`:** Documents `T` for OpenAPI/NSwag; compiler helps when you `return dto` directly; still allows `return NotFound()` as implicit conversion.
- **`IActionResult`:** Flexible for file downloads, redirects, or heterogeneous responses — OpenAPI may need `[ProducesResponseType]` attributes for each status.
- **Avoid `Task<T>` alone** on API actions if you need to return `404`/`400` without throwing — you lose unified result types.
- **Testing:** Assert `result.Result` is `OkObjectResult` with typed value, or use `Assert.IsType<ActionResult<UserDto>>`.
- **Minimal APIs:** `Results<T>` is the parallel pattern — controllers use `ActionResult<T>` for the same reason.

**Production takeaway:** The choice is about **contract clarity in OpenAPI and compile-time safety**, not performance.

---

#### Q5. (R) Review DELETE and update responses. Cache invalidation middleware keys on status code; QA reports stale list pages after delete.

**Answer:** DELETE should return `204 NoContent` when the body is empty — returning `200` with a JSON wrapper prevents cache middleware from recognizing delete semantics, and empty `Ok()` on PUT is ambiguous. Use `NoContent()` for successful delete/replace without a body.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | DELETE returns `200` with body | CDN/edge cache rules may not invalidate list keys |
| API design | Ad-hoc `{ deleted: true }` envelope | Clients parse unnecessary payload |
| Consistency | PUT returns `Ok()` with no content | Should be `204 NoContent` unless returning updated resource |
| REST | DELETE success commonly `204` | Tooling and HTTP clients expect no body |

**Fix (priority order):**

1. `return NoContent();` after successful delete.
2. PUT either returns `204` or `200` with full updated DTO — pick one and document.
3. Align cache middleware with `204`/`404` on DELETE for list invalidation.

**Production takeaway:** **Status code choice drives infrastructure behavior** — not just JSON shape.

---

#### Q6. (R) Review validation behavior. Frontend sends invalid JSON bodies but receives `200`-series responses with null fields processed as defaults.

**Answer:** Without `[ApiController]`, automatic 400 validation on model state is disabled — `[Required]` attributes are ignored unless you check `ModelState.IsValid`. Add `[ApiController]` or explicit validation and return `ValidationProblem()`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation | Missing `[ApiController]` | Invalid requests reach service layer |
| Data integrity | Empty SKU creates products with `""` | Bad rows in database |
| HTTP | Should return `400 Bad Request` | Clients cannot show field errors |
| API contract | No `ProblemDetails` shape | Frontend parsing breaks |

**Fix (priority order):**

1. Add `[ApiController]` to controller — enables automatic 400 on validation failure.
2. Or explicitly: `if (!ModelState.IsValid) return ValidationProblem(ModelState);`
3. Enable `AddProblemDetails()` in `Program.cs` for consistent RFC 7807 responses.

**Production takeaway:** **`[ApiController]` is not decorative** — it activates API-specific behaviors including validation 400.

---

#### Q7. (D) Review this "thin controller" refactor proposal. The interviewer asks whether moving logic to the service layer actually fixed the design problems.

**Answer:** Injecting services did not make the controller thin — orchestration, authorization gaps, and cross-cutting concerns (email, analytics) still live in the action. A true application service or command handler should own the workflow; the controller should authorize, validate, call one method, and map the result.

- **Still fat:** Stock check, payment, order placement, email, and analytics in one action — hard to test and reuse from message consumers.
- **Missing `[Authorize]`:** Payment operations exposed without policy — security review failure regardless of layering.
- **HTTP mapping:** Controller should map domain exceptions to `Conflict`, `402 Payment Required`, etc. — not embed business rules inline.
- **Better shape:** `CheckoutCommand` handled by `IOrderCheckoutHandler.CheckoutAsync` returning `Result<Order>`; controller maps to `CreatedAtAction`.
- **Trade-off:** One orchestrator class is fine for small teams — but this action is not "thin" yet.

**Production takeaway:** Karat distinguishes **DI wiring** from **separation of concerns** — thin controller means one job per action.

---

#### Q8. (R) Review return types and leaked domain models. Security scan flags internal fields in JSON responses.

**Answer:** Returning EF entities directly serializes every public property — including internal audit and notes fields — and couples the HTTP contract to the database schema. Project to a response DTO and return `ActionResult<OrderDto>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `InternalNotes`, `PaymentAudit` in JSON | Data leak to clients |
| Design | Entity used as API contract | Schema changes break clients on every migration |
| Performance | `.Include` loads unnecessary graphs | Over-fetching on every GET |
| Serialization | Circular references may cause 500 | JsonException at runtime |

**Fix (priority order):**

1. Map to `OrderDto` with only client fields — `return Ok(entity.ToDto());`
2. Remove broad `.Include` — project in query or map in service.
3. Add integration test asserting response JSON excludes internal property names.

**Production takeaway:** **`Ok(entity)` is a common leak** — action results carry DTOs, not persistence models.

---

#### Q9. (M) Compare `CreatedAtAction`, `CreatedAtRoute`, and `Created(uri, value)` for a multi-tenant API where the public URL is `https://api.example.com/tenant/{tenantId}/orders/{id}`. Which helper survives renamed actions and attribute route refactors?

**Answer:** `CreatedAtAction` resolves via action name and route values — it survives action renames when you use `nameof` and include `tenantId` in route values; `CreatedAtRoute` is stronger when you name a route template explicitly; raw `Created(string uri, ...)` is brittle when host or path prefix changes behind a gateway.

- **`CreatedAtAction(nameof(Get), new { tenantId, id = order.Id }, dto)`:** Participates in routing system; fixes host/scheme when `LinkGenerator` configured with forwarded headers.
- **`CreatedAtRoute("GetOrderById", values, dto)`:** Requires `[HttpGet("{id}", Name = "GetOrderById")]` — survives action method renames if route name kept stable.
- **`Created($"/tenant/{tenantId}/orders/{id}", dto)`:** Hard-coded path breaks behind path base, API management prefix, or lowercase URL policy.
- **Multi-tenant:** Must pass **all** route parameters (`tenantId`, `id`) or Location 404s.
- **Prefer:** Named route or `CreatedAtAction` with `nameof` — avoid manual string URLs in production.

**Production takeaway:** Link generation must go through **`LinkGenerator`/routing** so reverse-proxy path bases and renames do not break clients.
