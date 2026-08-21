# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/05. ViewModels & Strongly Typed Views`

---

#### Q1. (R) Review this MVC edit flow. The GET page renders, but POST throws `InvalidOperationException` about a tracked entity, and the Razor view shows columns that must never appear in HTML.

**Answer:** Passing EF entities to Razor couples the UI to persistence, leaks internal fields into HTML, and makes POST bind back into the same type — which then collides with the already-tracked instance from GET when `Update()` runs. Use a dedicated edit ViewModel for display and input; map to the tracked entity on POST.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | `@model Order` exposes entity shape to the view | `InternalMarginPercent`, navigations appear in markup or hidden fields |
| Change tracking | GET loads tracked entity; POST `Update(model)` attaches second instance | `InvalidOperationException` — duplicate key in change tracker |
| Security / binding | Mass assignment surface equals full entity | Client can post unexpected scalar or FK values |
| Concurrency | `RowVersion` not handled in stub update | Lost updates or token ignored on POST |

**Fix (priority order):**

1. Introduce `OrderEditViewModel` with only editable fields (`Id`, `ShipDate`, display-only customer name).
2. GET: project or map tracked entity → ViewModel; do not pass entity to the view.
3. POST: load tracked entity with `Find`/`FirstAsync`, map **from ViewModel onto tracked instance**, call `SaveChanges` — never `_db.Update(untrackedStub)` when another instance may be tracked.
4. Handle `RowVersion` with `[Timestamp]` on a byte array property copied through the ViewModel, or catch `DbUpdateConcurrencyException`.
5. Use `[ValidateNever]` on navigation props if an entity must remain in a view model for legacy code — prefer flat ViewModels.

**Production takeaway:** Strongly typed views should be typed to **view concerns**, not DbContext entities — the entity belongs in the service/repository layer, not in `@model`.

---

#### Q2. (R) Review this POST action for privilege escalation via mass assignment. QA passes because testers only change `ShipDate`.

**Answer:** `ReverseMap()` on an entity map allows inbound binding to populate **every** mappable property on `Order`, including `IsPriority` and `DiscountPercent`, when attackers add extra form fields — classic over-posting even though the ViewModel looks safe.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Over-posting / mass assignment via AutoMapper to entity | Privilege and pricing fields changed without UI exposure |
| Mapping | `ReverseMap()` maps ViewModel → all unprotected entity members | Hidden DB columns become writable from HTTP |
| Design | ViewModel whitelist illusion | Developers assume absence from ViewModel means absence from binding |

**Fix (priority order):**

1. Remove `ReverseMap()`; define **one-way** `CreateMap<Order, OrderEditViewModel>()` for GET only.
2. POST: load tracked entity; assign **explicitly** — `order.ShipDate = vm.ShipDate` — or use `CreateMap<OrderEditViewModel, Order>()` with `.ForAllMembers(opt => opt.Condition(...))` / ignore all except allowed members.
3. Never bind POST directly to `Order` or map untrusted input onto tracked entities without a field allow list.
4. Add integration test that POSTs extra fields and asserts `IsPriority` / `DiscountPercent` unchanged.

**Production takeaway:** ViewModels prevent over-posting only when the **server-side write path** ignores undeclared input — mapping profiles are part of the attack surface. See Model Binding module — binding is greedy.

---

#### Q3. (D) A team shares one `ProductDto` between the REST API and MVC admin screens. API clients need `SupplierCost` and audit timestamps; the browser form must not expose them. What breaks if you keep one type, and how do you split responsibilities without duplicating every field?

**Answer:** A shared DTO forces you to either leak sensitive API fields into HTML or maintain awkward `[JsonIgnore]` / conditional serialization rules that differ by endpoint — both fail under change. Split **API contracts** (DTOs) from **ViewModels** (MVC), and share only a thin common core or mapping layer if needed.

| Approach | MVC admin form | REST API | Risk |
|---|---|---|---|
| Single `ProductDto` | Must hide `SupplierCost` with view logic or attributes | Works | Forgotten `[HiddenInput]` → cost in HTML; wrong JSON attrs on API |
| `ProductEditViewModel` + `ProductResponseDto` | Whitelist for forms | Full contract for clients | Two types to map — intentional |
| Entity in both | Worst of both worlds | Serialization + over-posting | Do not use |

- **ViewModel:** shape driven by Razor — display names, select lists, validation messages, no secrets.
- **DTO:** versioned API contract — camelCase JSON, optional fields for PATCH, OpenAPI metadata.
- **Shared mapping:** AutoMapper profiles or manual mappers from `Product` entity → each outward type; inbound POST maps ViewModel → entity, API maps Request DTO → entity or command.
- **Duplication control:** base record with shared scalars (`Name`, `Sku`, `UnitPrice`) inherited by `ProductEditViewModel` and `CreateProductRequest` — do not inherit API DTO from ViewModel.

**Production takeaway:** DTO and ViewModel answer different questions — "what does the client JSON look like?" vs "what does this form need to render and round-trip?" — conflating them is how internal costs reach the browser.

---

#### Q4. (R) Review create vs edit ViewModel design. New products save with `Id = 0` overwriting an existing row; edit forms show validation errors on fields that should be read-only.

**Answer:** One ViewModel and one `Save` action merge incompatible rules: create must not trust client-supplied ids, edit must treat `Sku` and flags as read-only or admin-only, and `FindAsync(0)` or mis-bound ids cause wrong-row updates.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Identity | Shared POST for create/edit | `ProductId` tampering targets row 0 or another id |
| Validation | Same annotations for both flows | Required `Sku` on create but immutable on edit — wrong errors |
| UX | `IsDiscontinued` on create form | Business rule violated on insert |
| Routing | Single action name | Harder to authorize create vs edit separately |

**Fix (priority order):**

1. Split `CreateProductViewModel` (no id, required `Sku`) and `EditProductViewModel` (id + `[HiddenInput]` or route-only id, `Sku` display-only).
2. Separate actions: `[HttpPost] Create(CreateProductViewModel)` and `[HttpPost] Edit(int id, EditProductViewModel)` — id from route compared to hidden field for tamper check.
3. On create, **ignore** client `ProductId`; on edit, load tracked entity and map only mutable fields.
4. Use `[BindNever]` or omit properties on create model for admin-only fields.
5. Apply `[Authorize(Roles = "Admin")]` only on edit/discontinue actions.

**Production takeaway:** Create and edit are different **trust boundaries** — one god ViewModel saves typing until id 0 overwrites production data.

---

#### Q5. (R) Review mapping from entity to ViewModel. Support reports customer phone numbers and internal notes appear in "View Source" on the order details page.

**Answer:** The ViewModel is correct, but reusing edit-style tag helpers (`asp-for`) on a details page generates hidden inputs and name attributes that can round-trip or expose scaffolding mistakes; if the controller ever passes the **entity** again or the profile uses `.ForAllOtherMembers`, sensitive columns leak. Details pages should be read-only markup, not input helpers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| View layer | `asp-for` on details view | Generates `<input name="...">` — copy/paste from Edit template |
| Mapping leak | Profile or `.Adapt()` maps more than intended | Extra properties on VM if profile broadened later |
| Process | `@model OrderDetailsViewModel` but entity passed in dev branch | `InternalNotes` rendered via `@Html.DisplayFor` on wrong model |
| Security | PII in HTML source | Compliance issue even if not visible on screen |

**Fix (priority order):**

1. Details view: use `@Model.CustomerDisplayName` in plain text or `<span>` — no `asp-for` unless editing.
2. Audit AutoMapper profile — explicit `ForMember` list; no `ReverseMap` on details maps.
3. Code review rule: ViewModels for read pages contain **only** display fields; never map entity → VM with global conventions in one line.
4. Add Razor analyzer or test that Details action result does not contain `type="hidden"` for sensitive fields.

**Production takeaway:** Mapping leaks are often **view template** leaks — tag helpers designed for edit forms do not belong on read-only strongly typed views.

---

#### Q6. (R) Review nullable reference types on a strongly typed create form. Compiler is clean; production logs show `NullReferenceException` in POST and optional middle name never round-trips.

**Answer:** `#nullable enable` without `?` on optional strings tells the compiler `MiddleName` is never null, but model binding **does** set omitted optional fields to null — so `.Trim()` throws. Optional form fields need `string?` and null-conditional use; required strings need `[Required]` plus `= ""` or null-forgiving only after validation passes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| NRT | `string MiddleName` marked non-nullable | Compiler silence; runtime null from binding |
| Runtime | `model.MiddleName.Trim()` | `NullReferenceException` on empty optional field |
| Round-trip | Omitted field → null → not re-displayed | UX: optional value lost when returning `View(model)` after error |
| Validation | `[Required]` missing on required props | Non-nullable reference without initializer — CS8618 suppressed incorrectly |

**Fix (priority order):**

1. `public string? MiddleName { get; set; }` and `_users.Create(..., model.MiddleName?.Trim() ?? "", ...)`.
2. Required: `public string FirstName { get; set; } = "";` or `required` modifier + `[Required]`.
3. After invalid POST, return View with bound model so optional fields repopulate from `ModelState`.
4. Enable `<Nullable>enable</Nullable>` in csproj and fix warnings — they flag binding mismatches.

**Production takeaway:** NRT on ViewModels must match **HTML form optional/required semantics**, not database nullability alone.

---

#### Q7. (R) Review this AJAX partial refresh. The first load works; subsequent calls return 500 with JSON serialization cycle errors in logs.

**Answer:** Using `@model Order` for a partial view ties UI to a graph with circular navigations (`Customer.Orders` → `Order`). When the same entity is returned as JSON (SignalR or mistaken `return Json(order)`), System.Text.Json throws a cycle exception — and even Razor can accidentally enumerate cycles in debugging or custom helpers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | Entity as view model for partial | Lazy/circular navigations loaded unpredictably |
| Serialization | `Order` → `Customer` → `Orders` → … | `JsonException: A possible object cycle was detected` |
| Performance | `Include` loads full graph for a summary chip | Over-fetch on every AJAX poll |
| Coupling | Partial view knows EF shape | Refactor breaks `_OrderSummary.cshtml` |

**Fix (priority order):**

1. Define `OrderSummaryViewModel` with flat fields (`OrderId`, `CustomerName`, `Total`, `Status`).
2. Project in LINQ: `.Select(o => new OrderSummaryViewModel { ... })` — no `Include` needed for display fields.
3. Return `PartialView("_OrderSummary", vm)`; for SignalR push, serialize the **ViewModel**, not `Order`.
4. If JSON of graphs is unavoidable, `ReferenceHandler.IgnoreCycles` is a band-aid — fix the contract instead.

**Production takeaway:** Circular reference errors mean the **wrong type crossed the wire** — ViewModels break cycles by design. See EF module — do not serialize entity graphs to clients.

---

#### Q8. (R) Review validation placement. Changing a business rule requires a DB migration; unit tests for the ViewModel pass but wrong totals still save.

**Answer:** Data annotations on the EF entity tie validation to persistence metadata; mapping to entity bypasses ViewModel rules; and `$50k` line cap is a **domain rule**, not a column constraint — it belongs in the ViewModel (UI) and a domain/service validator, not only on `Order`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Layering | `[Range]` on entity | Fluent migrations for rule tweaks; wrong layer |
| Bypass | Map VM → entity without validating VM | Annotations on entity never run if VM lacks them |
| Business logic | Line total cap missing everywhere | Invalid orders persist |
| Testing | Entity annotation tests ≠ POST pipeline | False confidence |

**Fix (priority order):**

1. Move input validation to `OrderLineViewModel` — `[Range]`, custom `[LineTotalMax(50000)]` on VM or `IValidatableObject`.
2. Remove presentation validation from entity; keep DB constraints (max length, precision) in Fluent API.
3. Enforce rule in service: `if (vm.UnitPrice * vm.Quantity > 50000) return ValidationProblem(...)`.
4. `[ApiController]` / MVC filter runs validation on **action parameter type** — that must be the ViewModel.

**Production takeaway:** Validate what you **bind**; persist entities after validation and mapping. Entity annotations are a legacy pattern — ViewModels + FluentValidation + domain checks stack for Karat depth.

---

#### Q9. (D) A dashboard action builds one ViewModel with 40+ properties and three nested lists. Refactors are painful and partial views reuse the whole model. What are the concrete risks, and how would you decompose without fragmenting the page into dozens of controller round-trips?

**Answer:** God ViewModels create hidden coupling — every partial view can read sensitive slices of the graph, unit tests need massive setup, and parallel development causes merge conflicts on one class. Decompose by **view component boundaries**, not by arbitrary property count alone.

**Risks:**

| Risk | Manifestation |
|---|---|
| Authorization leak | `_Notifications.cshtml` displays admin-only fields from shared VM |
| Partial reuse | `@model DashboardViewModel` in child partial — breaks encapsulation |
| Performance | Single action loads orders + chart + profile even when partial fails |
| Change blast radius | New chart property touches same file as PII profile fields |
| Test cost | One fixture builds entire dashboard to test one widget |

**Decomposition (without N+1 round-trips):**

1. **View Components** (or tag helpers) with own ViewModels: `OrdersGridViewModel`, `ProfileCardViewModel`, `NotificationListViewModel` — each loaded via `InvokeAsync` with focused queries.
2. **Page shell** ViewModel holds only layout metadata; heavy widgets fetch via View Component in same request (still one HTTP request from browser).
3. Optional **secondary AJAX** only for slow widgets (chart) — lazy load with dedicated endpoint returning small VM JSON.
4. Shared **read models** / queries in application layer; do not nest widgets inside one god class.
5. Parent view: `@await Component.InvokeAsync("OrdersGrid", new { status = "Open" })` — strongly typed per component.

**Production takeaway:** Strong typing scales per **partial/view component**, not per page — one `@model` per screen is a smell when the screen is a composite dashboard.

---

#### Q10. (R) Review list performance. The orders index page times out at ~2k rows after switching from manual projection to AutoMapper.

**Answer:** The action materializes full `Order` entities with `Include` for all navigations, then maps in memory — AutoMapper does not replace SQL projection and adds reflection cost on a large tracked graph. List pages need `IQueryable` projection to the list ViewModel in the database.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Data access | `.Include(Customer).Include(LineItems).ToList()` | Loads entire object graph — memory and SQL bloat |
| Mapping | `Map<List<OrderListItemViewModel>>(orders)` after materialize | Client-side mapping; CPU on hot path |
| Profile | `ReverseMap()` unused on read but invites misuse | Inbound map could reintroduce over-posting elsewhere |
| Hot path | Index called every few seconds | Timeouts under load |

**Fix (priority order):**

1. Project in EF:  
   `_db.Orders.OrderByDescending(o => o.CreatedUtc).Select(o => new OrderListItemViewModel { OrderId = o.Id, CustomerName = o.Customer.Name, Total = o.LineItems.Sum(l => l.Qty * l.Price), Status = o.Status }).ToListAsync()`
2. No `Include` when Select navigates — EF translates to JOIN/SUBQUERY.
3. Optional: `.AsNoTracking()` on read-only lists.
4. Reserve AutoMapper for complex **single-entity** maps off the hot list path, or use `ProjectTo<OrderListItemViewModel>(_mapper.ConfigurationProvider)` **instead of** Include + Map — still ensure SQL is inspected.
5. Paginate — never map 2k+ rows to a grid without skip/take.

**Production takeaway:** AutoMapper on a hot list is a code smell — **IQueryable → ViewModel projection** is the production pattern; mapping libraries do not make Includes free. See EF CRUD module — detached/stub patterns vs read projections.

---
