# ViewModels & Strongly Typed Views — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 05. ViewModels & Strongly Typed Views](#chapter-05-viewmodels-strongly-typed-views)
  - [Q1. What is a ViewModel in ASP.NET Core MVC?](#chapter-05-viewmodels-strongly-typed-views-q1)
  - [Q2. What is the difference between a domain entity and a ViewMod…](#chapter-05-viewmodels-strongly-typed-views-q2)
  - [Q3. What is a strongly typed view?](#chapter-05-viewmodels-strongly-typed-views-q3)
  - [Q4. Why should you not pass EF entities directly to Razor views?](#chapter-05-viewmodels-strongly-typed-views-q4)
  - [Q5. What is over-posting (mass assignment) and how do ViewModels…](#chapter-05-viewmodels-strongly-typed-views-q5)
  - [Q6. What is the difference between a Create ViewModel and an Edi…](#chapter-05-viewmodels-strongly-typed-views-q6)
  - [Q7. What is the purpose of a read-only/details ViewModel?](#chapter-05-viewmodels-strongly-typed-views-q7)
  - [Q8. Why should sensitive fields (e.g., `IsAdmin`, internal margi…](#chapter-05-viewmodels-strongly-typed-views-q8)
  - [Q9. What is the difference between mapping in the controller vs …](#chapter-05-viewmodels-strongly-typed-views-q9)
  - [Q10. What problems occur when one DTO is shared between MVC views…](#chapter-05-viewmodels-strongly-typed-views-q10)
  - [Q11. How should navigation properties be handled in ViewModels fo…](#chapter-05-viewmodels-strongly-typed-views-q11)
  - [Q12. Where should validation attributes be placed — entity or Vie…](#chapter-05-viewmodels-strongly-typed-views-q12)
  - [Q13. What is the difference between presentation logic and busine…](#chapter-05-viewmodels-strongly-typed-views-q13)
  - [Q14. How do ViewModels help with unit testing controllers?](#chapter-05-viewmodels-strongly-typed-views-q14)
  - [Q15. What is a composite/page ViewModel and when might you split …](#chapter-05-viewmodels-strongly-typed-views-q15)
  - [Q16. Why can returning an entity to `PartialView` cause serializa…](#chapter-05-viewmodels-strongly-typed-views-q16)
  - [Q17. What is `[BindNever]` and when is it used on ViewModels?](#chapter-05-viewmodels-strongly-typed-views-q17)
  - [Q18. How do nullable reference types affect ViewModel design?](#chapter-05-viewmodels-strongly-typed-views-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 05. ViewModels & Strongly Typed Views

### Q1. What is a ViewModel in ASP.NET Core MVC? {#chapter-05-viewmodels-strongly-typed-views-q1}

What is a ViewModel in ASP.NET Core MVC?

**Answer:** A ViewModel is a plain C# class shaped specifically for a view's display and input needs — containing only the properties, validation rules, and UI metadata that a particular Razor page requires. It decouples the presentation layer from domain entities and database schema.

- ViewModels live in the web project (or a shared contracts project) — not in the domain or EF entity layer.
- Each screen or partial typically has its own ViewModel (`ProductEditViewModel`, `OrderListItemViewModel`) rather than one generic type.
- Controllers map entities to ViewModels on GET and map ViewModels back to entities or commands on POST.
- Strongly typed views declare `@model MyViewModel` and get compile-time checking for property access.

---

### Q2. What is the difference between a domain entity and a ViewModel? {#chapter-05-viewmodels-strongly-typed-views-q2}

What is the difference between a domain entity and a ViewModel?

**Answer:** A domain entity models business concepts and persistence — ids, navigations, concurrency tokens, and database constraints. A ViewModel models a UI concern — display labels, dropdown options, formatted read-only fields, and whitelisted editable inputs.

- Entities carry EF annotations, navigation properties, and internal fields (`InternalMargin`, `RowVersion`); ViewModels expose only what the user should see or edit.
- Entities change when the database schema changes; ViewModels change when the UI changes — independent lifecycles.
- Passing entities to views couples Razor to EF and invites over-posting; ViewModels define an explicit trust boundary.
- Mapping between them happens in the controller, a dedicated mapper, or an application service — never by exposing entities directly.

---

### Q3. What is a strongly typed view? {#chapter-05-viewmodels-strongly-typed-views-q3}

What is a strongly typed view?

**Answer:** A strongly typed view declares its model type with `@model MyViewModel` at the top, giving the Razor compiler and IDE knowledge of available properties. Access uses `Model.PropertyName` with IntelliSense, compile-time errors on typos, and clear data contracts.

- Contrast with untyped views that rely on `ViewBag`/`ViewData` dynamic access — no compile-time safety.
- Tag helpers such as `asp-for="PropertyName"` require a strongly typed model to generate correct `name`, `id`, and validation attributes.
- The controller passes the model via `return View(viewModel)` — ASP.NET Core sets `ViewData.Model` automatically.
- Partial views can also be strongly typed with their own `@model` distinct from the parent page model.

---

### Q4. Why should you not pass EF entities directly to Razor views? {#chapter-05-viewmodels-strongly-typed-views-q4}

Why should you not pass EF entities directly to Razor views?

**Answer:** EF entities expose the full persistence graph — navigation properties, shadow properties, and internal columns — to HTML and model binding. This causes over-posting on POST, lazy-load surprises in the view, schema coupling, and circular reference errors when serializing for AJAX.

- Hidden or `[BindNever]` fields on entities do not stop attackers from posting extra form keys.
- Navigation properties may trigger unintended database queries during rendering (N+1 in views).
- EF change-tracker conflicts arise when GET loads a tracked entity and POST binds a second instance of the same type.
- ViewModels project flat, intentional shapes — `CustomerName` as a string instead of a `Customer` navigation object.

---

### Q5. What is over-posting (mass assignment) and how do ViewModels prevent it? {#chapter-05-viewmodels-strongly-typed-views-q5}

What is over-posting (mass assignment) and how do ViewModels prevent it?

**Answer:** Over-posting occurs when model binding sets properties the user should not control — such as `IsAdmin` or `DiscountPercent` — because those properties exist on the bound type even if the Razor form omits them. Attackers add extra form fields via dev tools to escalate privileges or change prices.

- ViewModels whitelist only editable fields — properties not on the ViewModel cannot be bound from the POST.
- Server-side mapping copies only ViewModel properties onto the tracked entity; undeclared POST keys are ignored.
- `[Bind(Include = "...")]` on entities is a weaker alternative — ViewModels are the preferred MVC pattern.
- AutoMapper `ReverseMap()` can reintroduce over-posting if it maps all entity members — use explicit allow lists on POST.

---

### Q6. What is the difference between a Create ViewModel and an Edit ViewModel? {#chapter-05-viewmodels-strongly-typed-views-q6}

What is the difference between a Create ViewModel and an Edit ViewModel?

**Answer:** Create and Edit ViewModels reflect different trust boundaries — create has no id, requires identity fields like `Sku`, and omits admin-only flags; edit carries an id (often hidden), treats immutable fields as read-only, and may include concurrency tokens.

- `CreateProductViewModel`: no `ProductId`, required `Sku`, no `IsDiscontinued` on insert forms.
- `EditProductViewModel`: route-bound id, display-only `Sku`, mutable fields only, optional `[Timestamp] RowVersion`.
- Separate actions (`Create` POST vs `Edit` POST) allow distinct authorization and validation rules.
- A single god ViewModel for both flows risks id tampering (`ProductId = 0` overwriting rows) and wrong validation on read-only fields.

---

### Q7. What is the purpose of a read-only/details ViewModel? {#chapter-05-viewmodels-strongly-typed-views-q7}

What is the purpose of a read-only/details ViewModel?

**Answer:** A details ViewModel exposes only display-safe fields for read-only pages — formatted dates, computed totals, status labels — with no editable inputs or sensitive internal data. It prevents accidental round-tripping, hidden-field leakage, and edit-template mistakes on read pages.

- Contains display strings (`CustomerDisplayName`, `FormattedTotal`) rather than raw FK ids and navigations.
- Excludes internal notes, margin percentages, audit timestamps, and PII not needed for the viewer's role.
- Details views use plain text or `@Html.DisplayFor` — not `asp-for` tag helpers that generate hidden inputs.
- Separate from edit ViewModels even when showing the same entity — different shape, different authorization.

---

### Q8. Why should sensitive fields (e.g., `IsAdmin`, internal margin) be excluded from ViewModels? {#chapter-05-viewmodels-strongly-typed-views-q8}

Why should sensitive fields (e.g., `IsAdmin`, internal margin) be excluded from ViewModels?

**Answer:** Any property on a bound ViewModel can be set via crafted POST requests regardless of whether Razor rendered an input for it. Sensitive fields must not exist on user-facing ViewModels — role flags and internal pricing belong on admin-only ViewModels behind authorization.

- HTML hidden fields are not security — attackers POST arbitrary key/value pairs.
- Internal margin or cost data in ViewModels may appear in HTML source even on read-only pages if templates leak them.
- Admin operations should use separate endpoints, ViewModels, and `[Authorize(Roles = "Admin")]` guards.
- Compliance (PII, financial data) requires minimizing data exposed to the browser, not just hiding it visually.

---

### Q9. What is the difference between mapping in the controller vs using AutoMapper? {#chapter-05-viewmodels-strongly-typed-views-q9}

What is the difference between mapping in the controller vs using AutoMapper?

**Answer:** Manual mapping in the controller assigns each property explicitly — maximum control, no magic, easy to audit for over-posting. AutoMapper uses convention-based profiles to reduce boilerplate but can hide security gaps if profiles use broad `ReverseMap()` or `ForAllMembers` rules.

- Manual: `order.ShipDate = vm.ShipDate` — verbose but safe and obvious in code review.
- AutoMapper: `CreateMap<Order, OrderEditViewModel>()` for GET; POST maps only allowed members with explicit ignores.
- AutoMapper `ProjectTo<T>()` helps read-only lists with EF `IQueryable` projection — still inspect generated SQL.
- Prefer manual mapping on security-sensitive POST paths; use AutoMapper for complex GET projections off the hot path.

---

### Q10. What problems occur when one DTO is shared between MVC views and REST APIs? {#chapter-05-viewmodels-strongly-typed-views-q10}

What problems occur when one DTO is shared between MVC views and REST APIs?

**Answer:** A shared DTO forces conflicting serialization and exposure rules — API clients need full contracts with camelCase JSON, while MVC forms need whitelisted, human-oriented fields without secrets. One type leads to `[JsonIgnore]` hacks, accidental cost leakage in HTML, and breaking API changes when the UI changes.

- MVC ViewModels answer "what does this form render and round-trip?"; API DTOs answer "what does the JSON contract look like?"
- API versioning and OpenAPI metadata do not belong on ViewModels; display names and select lists do not belong on API DTOs.
- Share mapping from the domain entity to each outward type — do not inherit API DTOs from ViewModels.
- A thin shared record for common scalars (`Name`, `Sku`) is acceptable; diverge outward types for each consumer.

---

### Q11. How should navigation properties be handled in ViewModels for partial views? {#chapter-05-viewmodels-strongly-typed-views-q11}

How should navigation properties be handled in ViewModels for partial views?

**Answer:** Flatten navigations into display-friendly scalar properties on the ViewModel — `CustomerName`, `CategoryLabel` — rather than passing EF navigation objects to partials. Partials receive small, focused ViewModels with only the fields they render.

- `_OrderLinePartial` uses `@model OrderLineViewModel` with `ProductName` and `Quantity` — not `@model OrderLine` with `Product` navigation.
- Populate flattened fields via LINQ projection in the controller or query layer before the view runs.
- Avoid `@inject` + lazy loading in partials inside loops — batch data upstream into the ViewModel.
- Nested ViewModels (`AddressViewModel` inside `CheckoutViewModel`) are fine when the partial owns that subgraph — still no EF types.

---

### Q12. Where should validation attributes be placed — entity or ViewModel? {#chapter-05-viewmodels-strongly-typed-views-q12}

Where should validation attributes be placed — entity or ViewModel?

**Answer:** Input validation attributes (`[Required]`, `[StringLength]`, `[Range]`) belong on the ViewModel — the type MVC binds and validates on POST. Database constraints (max length, precision, indexes) belong in EF Fluent API on entities; domain rules belong in services or domain validators.

- MVC validates the action parameter type — if that is the ViewModel, entity annotations are bypassed when mapping skips the entity during binding.
- Entity annotations tie business rule changes to migrations and the wrong layer.
- Use `IValidatableObject` or FluentValidation on ViewModels for cross-field rules (password confirmation, date ranges).
- Keep `[Timestamp]` and DB-specific attributes on entities; keep `[Compare]`, `[Display]`, and form rules on ViewModels.

---

### Q13. What is the difference between presentation logic and business logic in ViewModels? {#chapter-05-viewmodels-strongly-typed-views-q13}

What is the difference between presentation logic and business logic in ViewModels?

**Answer:** Presentation logic formats data for display — computed labels, select-list options, `bool` to "Yes"/"No" strings, and `[Display(Name = "...")]` metadata. Business logic enforces domain rules — pricing caps, inventory checks, authorization decisions — and belongs in services, not ViewModels.

- Acceptable in ViewModel: `public string StatusLabel => IsActive ? "Active" : "Inactive";` for display.
- Not acceptable: `public decimal Total => Lines.Sum(l => l.Qty * l.GetDiscountedPrice());` with DB calls or domain rules.
- Business validation that must hold regardless of UI belongs in the domain/service layer, duplicated-checked on POST after ViewModel validation passes.
- ViewModels may hold UI state (`SelectedCategoryId`, `AvailableCategories` list) — not transactional behavior.

---

### Q14. How do ViewModels help with unit testing controllers? {#chapter-05-viewmodels-strongly-typed-views-q14}

How do ViewModels help with unit testing controllers?

**Answer:** ViewModels are plain POCOs — tests construct them directly, invoke controller actions, and assert on `ViewResult` models without spinning up EF, SQL, or Razor. Tests verify mapping, validation gates, and authorization without rendering views.

- Arrange: `var vm = new EditProductViewModel { ProductId = 1, Name = "Test" };`
- Act: `var result = await controller.Edit(1, vm) as ViewResult;`
- Assert: `Assert.IsType<EditProductViewModel>(result.Model); Assert.False(controller.ModelState.IsValid);`
- Mock services return entities; controller maps to/from ViewModels — tests stay focused on HTTP/orchestration concerns.

---

### Q15. What is a composite/page ViewModel and when might you split it? {#chapter-05-viewmodels-strongly-typed-views-q15}

What is a composite/page ViewModel and when might you split it?

**Answer:** A composite ViewModel aggregates multiple UI regions into one `@model` for a dashboard or wizard page. Split it when the page grows many unrelated properties, partials reuse the entire model, or widgets need independent queries and authorization — decompose into View Component ViewModels instead.

- God ViewModels (40+ properties, nested lists) create merge conflicts, authorization leaks in partials, and expensive single queries.
- Split by widget boundary: `OrdersGridViewModel`, `ProfileCardViewModel` — each loaded by a View Component in the same HTTP request.
- Page shell ViewModel holds layout metadata only; heavy widgets use `Component.InvokeAsync` with focused types.
- Avoid splitting into dozens of AJAX round-trips unless a widget is genuinely slow — View Components preserve one page load.

---

### Q16. Why can returning an entity to `PartialView` cause serialization errors? {#chapter-05-viewmodels-strongly-typed-views-q16}

Why can returning an entity to `PartialView` cause serialization errors?

**Answer:** EF entities with bidirectional navigations (`Order.Customer.Orders`) form object graphs that System.Text.Json cannot serialize by default — it throws a cycle detection exception. Even Razor partials risk accidental enumeration of circular graphs through debugging or custom helpers.

- `return PartialView("_OrderSummary", order)` passes a graph, not a flat DTO — AJAX or SignalR endpoints reusing the same action hit JSON serializers.
- Lazy-loaded navigations cause unpredictable query counts and null references in partials.
- Define `OrderSummaryViewModel` with flat fields; project in LINQ before passing to the partial.
- `ReferenceHandler.IgnoreCycles` is a band-aid — fix the contract with ViewModels.

---

### Q17. What is `[BindNever]` and when is it used on ViewModels? {#chapter-05-viewmodels-strongly-typed-views-q17}

What is `[BindNever]` and when is it used on ViewModels?

**Answer:** `[BindNever]` tells the model binder to skip a property during POST binding — the binder will not set that property from form values even if keys are present. On ViewModels it protects properties populated only on GET (dropdown lists, display ids) from being overwritten by tampered POST data.

- Apply to `AvailableCategories`, `RowVersion` display copies, or route-supplied ids set server-side after binding.
- Prefer omitting sensitive properties from the ViewModel entirely over `[BindNever]` — exclusion beats opt-out.
- On entities, `[BindNever]` on navigations prevents binding graphs — but ViewModels are the primary defense.
- `[BindNever]` does not remove properties from GET rendering — it only affects inbound binding.

---

### Q18. How do nullable reference types affect ViewModel design? {#chapter-05-viewmodels-strongly-typed-views-q18}

How do nullable reference types affect ViewModel design?

**Answer:** Nullable reference types (`#nullable enable`) must align with HTML form semantics — optional text fields need `string?`, required fields need `[Required]` with `= ""` or the `required` modifier. Model binding sets omitted optional fields to `null`, which contradicts non-nullable `string` declarations and causes `NullReferenceException` in `.Trim()` calls.

- Optional: `public string? MiddleName { get; set; }` — use `model.MiddleName?.Trim()`.
- Required: `public string FirstName { get; set; } = "";` plus `[Required]` — satisfies NRT and binding.
- Non-nullable reference without initializer triggers CS8618 — fix with defaults, not `#nullable disable` suppression.
- NRT on ViewModels reflects form optional/required behavior, not necessarily database nullability.

---

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Answer:** Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently.

- Views should render data the controller or ViewModel already prepared.
- Authorization belongs in filters, policies, or controller/service checks before the view executes.
- Calculations in Razor cannot be tested independently and often diverge from API or batch logic.
- Keep Razor limited to presentation formatting — not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Answer:** Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema.

- Lazy-loaded navigations can trigger unexpected queries during rendering.
- Mass assignment can update properties the user should not control (e.g., `IsAdmin`).
- Use dedicated ViewModels with only the fields the view needs.
- Map between entities and ViewModels in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Answer:** Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values.

- Remove `[FromBody]` for conventional form POSTs and let model binding read form fields.
- Use `[FromBody]` only when the client sends JSON with the correct Content-Type.
- Silent binding failure is a common source of "my POST action receives null model" bugs.
- AJAX forms using `FormData` follow the same form binding rules as full-page forms.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Answer:** Client-side validation is bypassable — attackers POST directly without browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect.

- Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent.
- Client validation improves UX for legitimate users only.
- Remote validation and unobtrusive rules are not security boundaries.
- Treat missing server validation as a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Answer:** Returning the same view after a successful POST causes duplicate submission when the user refreshes the page — the browser resubmits the POST body.

- Use Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create/update.
- PRG separates the mutation (POST) from the display (GET).
- Flash success messages via TempData on the redirect target.
- AJAX partial POSTs have a similar concern — disable submit during request or use idempotent server logic.

---

#### Gotcha 6. `ModelState` after redirect

**Answer:** `ModelState` is request-scoped and does not survive `RedirectToAction`. Validation errors are lost unless rehydrated through TempData, a second validation pass on GET, or by redisplaying the form without redirect on failure only.

- Common pattern: redirect only on success; on validation failure return `View(model)` with errors inline.
- To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.
- Do not assume errors automatically follow the user after redirect.
- AJAX partial forms avoid redirect and can return the form partial with `ModelState` errors directly.

---

#### Gotcha 7. TempData read twice in layout and view

**Answer:** TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless you use `Peek()` or `Keep()`.

- Use `TempData.Peek("Message")` in the layout to read without consuming.
- Or call `TempData.Keep("Message")` after the layout read so the view can read it too.
- Prefer a single consumption point — typically the layout or a dedicated partial, not both.
- Cookie-based TempData has size limits; avoid storing large payloads.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Answer:** Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route.

- Every area controller must declare `[Area("AreaName")]` matching its folder.
- Area routing is registered separately in `Program.cs` with the `{area:exists}` constraint.
- Without the attribute, MVC treats the controller as a root controller.
- Verify area registration order — specific area routes before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Answer:** Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment.

- From within an area, omitting `asp-area` keeps links inside the current area — sometimes incorrectly.
- Cross-area links require both `asp-area` and `asp-controller` (and `asp-action`).
- Wrong URLs produce 404 or hit unintended controllers.
- Same rule applies to `Url.Action` — pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Answer:** A missing unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value — not null or empty.

- Use `bool?` with `[Required]` to require an explicit true selection for consent checkboxes.
- Or use the hidden-field pattern: hidden input `false` plus checkbox `true` so unchecked still posts `false` deliberately.
- Server-side, verify explicit consent with a dedicated check rather than relying on `[Required]` alone.
- This applies to both full-page forms and AJAX form posts.

---

#### Gotcha 11. Collection binding with gap indices

**Answer:** Deleting a row from a dynamic form leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate.

- Reindex client-side after row deletion so indices are contiguous starting at zero.
- Or implement a custom `IModelBinder` that tolerates non-contiguous indices.
- Partial views rendering collection editors must maintain consistent index naming.
- Test add/delete row scenarios explicitly in complex form POSTs.

---

#### Gotcha 12. `@Html.Raw` with user content

**Answer:** Default Razor encoding prevents XSS by HTML-encoding output. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side.

- Encode first, then apply safe formatting — never wrap raw user input in HTML.
- AJAX-loaded partials injected via `innerHTML` execute injected script the same as full pages.
- Prefer `@Model.UserComment` (auto-encoded) or sanitize with a trusted HTML sanitizer library.
- Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Answer:** Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` header or `__RequestVerificationToken` form field or POSTs fail with 400 antiforgery errors.

- Read the hidden field value from the page and include it on every mutating AJAX request.
- Same-origin requests send the antiforgery cookie automatically.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods — missing tokens fail before the action runs.
- Do not disable antiforgery on MVC cookie-auth endpoints to "fix" AJAX — add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Answer:** Hubs are not registered in DI for direct injection into controllers. Use `IHubContext<THub>` to broadcast messages from controllers, services, or background jobs.

- Injecting a concrete `Hub` fails activation or produces an instance without connection context.
- `IHubContext<T>` is a singleton proxy registered by `AddSignalR()`.
- Pair with Redis backplane or Azure SignalR for multi-instance fan-out.
- Keep hubs thin; business logic stays in scoped or transient services.

---

#### Gotcha 15. SignalR scale-out without backplane

**Answer:** Sticky sessions alone do not fan-out events across server instances. Multi-node deployments need a Redis backplane or Azure SignalR Service so messages sent from any instance reach clients on all instances.

- Controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane.
- Sticky sessions route connections but do not route cross-instance messages.
- Group membership and connection IDs are local to each instance.
- Register `AddStackExchangeRedis` or `AddAzureSignalR` when scaling beyond a single node.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

## Gotchas — ASP.NET Core MVC (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this MVC edit flow. The GET page renders, but POST throws `InvalidOperationException` about a tracked entity, and the Razor view shows columns that must never appear in HTML.

```csharp
// GET
public async Task<IActionResult> Edit(int id)
{
    var order = await _db.Orders.Include(o => o.Customer).FirstAsync(o => o.Id == id);
    return View(order); // @model Order
}

// POST
[HttpPost]
public async Task<IActionResult> Edit(int id, Order model)
{
    _db.Orders.Update(model);
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

The `Order` entity has `Customer`, `LineItems`, `InternalMarginPercent`, and `RowVersion`.

---

**Answer:**

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

---

#### Q2. (R) Review this POST action for privilege escalation via mass assignment. QA passes because testers only change `ShipDate`.

```csharp
public class OrderEditViewModel
{
    public int Id { get; set; }
    public DateTime ShipDate { get; set; }
}

// Hidden in DB entity but NOT on ViewModel:
// public bool IsPriority { get; set; }
// public decimal DiscountPercent { get; set; }

[HttpPost]
public async Task<IActionResult> Edit(OrderEditViewModel vm)
{
    var order = await _db.Orders.FindAsync(vm.Id);
    _mapper.Map(vm, order); // AutoMapper profile: CreateMap<OrderEditViewModel, Order>().ReverseMap();
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

Attacker POSTs extra JSON/form fields: `IsPriority=true&DiscountPercent=100`.

---

**Answer:**

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

---

#### Q3. (D) A team shares one `ProductDto` between the REST API (`ProductsController`) and MVC admin screens (`ProductsController` in Areas/Admin). API clients need `SupplierCost` and audit timestamps; the browser form must not expose them. What breaks if you keep one type, and how do you split responsibilities without duplicating every field?

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review create vs edit ViewModel design. New products save with `Id = 0` overwriting an existing row; edit forms show validation errors on fields that should be read-only.

```csharp
public class ProductViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public string Sku { get; set; } = "";
    public bool IsDiscontinued { get; set; } // admin-only on edit
}

[HttpPost]
public async Task<IActionResult> Save(ProductViewModel vm)
{
    var entity = vm.ProductId == 0 ? new Product() : await _db.Products.FindAsync(vm.ProductId);
    entity.Name = vm.Name;
    entity.UnitPrice = vm.UnitPrice;
    entity.Sku = vm.Sku;
    entity.IsDiscontinued = vm.IsDiscontinued;
    if (vm.ProductId == 0) _db.Products.Add(entity);
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

Create and Edit both use `@model ProductViewModel` and the same `Save` action.

---

**Answer:**

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

---

#### Q5. (R) Review mapping from entity to ViewModel. Support reports that after deploy, customer phone numbers and internal notes appear in "View Source" on the order details page.

```csharp
public class OrderDetailsViewModel
{
    public int OrderId { get; set; }
    public string CustomerDisplayName { get; set; } = "";
    public decimal Total { get; set; }
}

// AutoMapper profile
CreateMap<Order, OrderDetailsViewModel>()
    .ForMember(d => d.CustomerDisplayName, o => o.MapFrom(s => s.Customer.Name));

// Controller
var vm = _mapper.Map<OrderDetailsViewModel>(order);
return View(vm);
```

```html
@* Views/Orders/Details.cshtml *@
@model OrderDetailsViewModel
<input asp-for="CustomerDisplayName" />
@* Developer copied scaffold from Edit view *@
```

Entity `Customer` also has `Phone`, `Email`, `InternalNotes` — not on ViewModel.

---

**Answer:**

_Answer not found._

---

#### Q6. (R) Review nullable reference types on a strongly typed create form. Compiler is clean; production logs show `NullReferenceException` in POST and optional middle name never round-trips.

```csharp
#nullable enable
public class RegisterViewModel
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    public string MiddleName { get; set; }  // optional
    [Required][EmailAddress] public string Email { get; set; }
}

[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid) return View(model);
    _users.Create(model.FirstName, model.MiddleName.Trim(), model.Email);
    return RedirectToAction(nameof(Index));
}
```

Form omits `MiddleName` when blank; model binding sets it to `null`.

---

**Answer:**

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

---

#### Q7. (R) Review this AJAX partial refresh. The first load works; subsequent calls return 500 with JSON serialization cycle errors in logs.

```csharp
[HttpGet]
public IActionResult OrderSummary(int id)
{
    var order = _db.Orders
        .Include(o => o.Customer)
        .Include(o => o.LineItems)
        .First(o => o.Id == id);
    return PartialView("_OrderSummary", order); // @model Order
}

// _OrderSummary.cshtml — also used by SignalR hub pushing Order JSON to clients
```

`Customer` has `ICollection<Order> Orders`; `LineItem` has navigation back to `Order`.

---

**Answer:**

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

---

#### Q8. (R) Review validation placement. Changing a business rule requires a DB migration; unit tests for the ViewModel pass but wrong totals still save.

```csharp
public class Order
{
    public int Id { get; set; }
    [Required][Range(0.01, 10000)] public decimal UnitPrice { get; set; }
    [Range(1, 100)] public int Quantity { get; set; }
}

public class OrderLineViewModel
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

[HttpPost]
public async Task<IActionResult> AddLine(OrderLineViewModel vm)
{
    var line = _mapper.Map<OrderLine>(vm);
    _db.OrderLines.Add(line);
    await _db.SaveChangesAsync();
    return Ok();
}
```

Rule: `LineTotal` must not exceed $50,000 per line.

---

**Answer:**

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

---

#### Q9. (D) A dashboard action builds one ViewModel for a page with orders grid, user profile card, notification feed, and chart series. The type has 40+ properties and three nested lists. Refactors are painful and partial views reuse the whole model. What are the concrete risks, and how would you decompose without fragmenting the page into dozens of controller round-trips?

---

**Answer:**

_Answer not found._

---

#### Q10. (R) Review list performance. The orders index page times out at ~2k rows after switching from manual projection to AutoMapper.

```csharp
public IActionResult Index()
{
    var orders = _db.Orders
        .Include(o => o.Customer)
        .Include(o => o.LineItems)
        .OrderByDescending(o => o.CreatedUtc)
        .ToList();

    var vms = _mapper.Map<List<OrderListItemViewModel>>(orders);
    return View(vms);
}

// Profile uses ReverseMap and maps Customer.Name -> CustomerName
CreateMap<Order, OrderListItemViewModel>().ReverseMap();
```

`OrderListItemViewModel` needs: `OrderId`, `CustomerName`, `Total`, `Status` — nothing else.

---

**Answer:**

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

---
