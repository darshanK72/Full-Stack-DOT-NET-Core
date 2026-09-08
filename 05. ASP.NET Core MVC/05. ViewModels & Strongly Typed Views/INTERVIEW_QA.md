# ViewModels & Strongly Typed Views — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a ViewModel in ASP.NET Core MVC?](#q1-what-is-a-viewmodel-in-aspnet-core-mvc)
2. [Q2. What is the difference between a domain entity and a ViewModel?](#q2-what-is-the-difference-between-a-domain-entity-and-a-viewmodel)
3. [Q3. What is a strongly typed view?](#q3-what-is-a-strongly-typed-view)
4. [Q4. Why should you not pass EF entities directly to Razor views?](#q4-why-should-you-not-pass-ef-entities-directly-to-razor-views)
5. [Q5. What is over-posting (mass assignment) and how do ViewModels prevent it?](#q5-what-is-over-posting-mass-assignment-and-how-do-viewmodels-prevent-it)
6. [Q6. What is the difference between a Create ViewModel and an Edit ViewModel?](#q6-what-is-the-difference-between-a-create-viewmodel-and-an-edit-viewmodel)
7. [Q7. What is the purpose of a read-only/details ViewModel?](#q7-what-is-the-purpose-of-a-read-onlydetails-viewmodel)
8. [Q8. Why should sensitive fields (e.g., `IsAdmin`, internal margin) be excluded from ViewModels?](#q8-why-should-sensitive-fields-eg-isadmin-internal-margin-be-excluded-from-viewmodels)
9. [Q9. What is the difference between mapping in the controller vs using AutoMapper?](#q9-what-is-the-difference-between-mapping-in-the-controller-vs-using-automapper)
10. [Q10. What problems occur when one DTO is shared between MVC views and REST APIs?](#q10-what-problems-occur-when-one-dto-is-shared-between-mvc-views-and-rest-apis)
11. [Q11. How should navigation properties be handled in ViewModels for partial views?](#q11-how-should-navigation-properties-be-handled-in-viewmodels-for-partial-views)
12. [Q12. Where should validation attributes be placed — entity or ViewModel?](#q12-where-should-validation-attributes-be-placed-entity-or-viewmodel)
13. [Q13. What is the difference between presentation logic and business logic in ViewModels?](#q13-what-is-the-difference-between-presentation-logic-and-business-logic-in-viewmodels)
14. [Q14. How do ViewModels help with unit testing controllers?](#q14-how-do-viewmodels-help-with-unit-testing-controllers)
15. [Q15. What is a composite/page ViewModel and when might you split it?](#q15-what-is-a-compositepage-viewmodel-and-when-might-you-split-it)
16. [Q16. Why can returning an entity to `PartialView` cause serialization errors?](#q16-why-can-returning-an-entity-to-partialview-cause-serialization-errors)
17. [Q17. What is `[BindNever]` and when is it used on ViewModels?](#q17-what-is-bindnever-and-when-is-it-used-on-viewmodels)
18. [Q18. How do nullable reference types affect ViewModel design?](#q18-how-do-nullable-reference-types-affect-viewmodel-design)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is a ViewModel in ASP.NET Core MVC?

**Concepts**
- Plain C# class shaped for a specific view's display and input needs
- Decoupling the presentation layer from domain entities
- One ViewModel per screen rather than one generic type
- Controller mapping entities to ViewModels on GET and back on POST
- `@model MyViewModel` providing compile-time checking in Razor

**Answer**

The key insight behind ViewModels is that what a view needs to render and what the database stores are almost never the same shape. A ViewModel is a plain C# class containing only the properties, validation rules, and UI metadata that a particular Razor page requires — no navigation properties, no audit columns, no fields the user should not see or edit. They live in the web project (or a shared contracts project) rather than in the domain or EF entity layer, so changes to the database schema do not automatically ripple into the UI. Controllers act as the translation layer: on GET they map entities or service results to ViewModels and pass them to the view; on POST they receive a ViewModel from model binding and map it back to entities or service commands before persisting. Strongly typed views declare `@model MyViewModel` and get compile-time property checking, IntelliSense, and tag-helper integration as a result.

---

## Q2. What is the difference between a domain entity and a ViewModel?

**Concepts**
- Domain entity modeling persistence and business concepts
- ViewModel modeling a UI screen's display and input contract
- Independent lifecycles for schema changes vs UI changes
- ViewModel as an explicit trust boundary for user input
- Navigation properties, concurrency tokens, and internal fields on entities only

**Answer**

A domain entity models business concepts and their persistence — it carries EF annotations, navigation properties, concurrency tokens like `RowVersion`, and internal fields such as `InternalMarginPercent` that no user should ever edit directly. A ViewModel models a UI concern — display labels, dropdown options, formatted read-only fields, and a whitelist of editable inputs appropriate for one specific screen. Their lifecycles are independent: entities change when the database schema changes, ViewModels change when the UI changes, and coupling them together means a column rename forces a form redesign or vice versa. Passing entities to views removes that boundary, inviting over-posting on POST and leaking schema details like FK ids and shadow properties into HTML. Mapping between them in the controller or a dedicated mapper is intentional friction that forces you to think about exactly which data crosses the trust boundary.

---

## Q3. What is a strongly typed view?

**Concepts**
- `@model MyViewModel` declaration enabling compile-time property access
- IntelliSense and build-time typo detection
- `Model.PropertyName` vs `ViewBag` dynamic access
- Tag helpers requiring strongly typed model for correct attribute generation
- Partial views having their own distinct `@model` type

**Answer**

A strongly typed view declares its model type with `@model MyViewModel` at the top, giving the Razor compiler full knowledge of available properties. This means that accessing `Model.TotalAmount` with a typo becomes a build error rather than a runtime null reference, and IntelliSense completes property names while editing the view. The practical impact is most visible with tag helpers like `asp-for="PropertyName"` — they require a strongly typed model to generate correct `name`, `id`, and client validation attributes because they work from expression trees, not string literals. The controller passes the model with `return View(viewModel)`, and ASP.NET Core sets `ViewData.Model` automatically. Contrast this with untyped views that use `ViewBag.TotalAmount` — a dynamic access that compiles regardless of whether that key was ever set, producing silent null renders that only appear at runtime.

---

## Q4. Why should you not pass EF entities directly to Razor views?

**Concepts**
- Navigation properties triggering N+1 queries during rendering
- Mass assignment surface including all entity properties on POST
- EF change-tracker conflicts between GET and POST instances
- Schema coupling tying Razor to database column structure
- Shadow properties and internal fields leaking into HTML

**Answer**

Passing EF entities to Razor creates several overlapping problems. Navigation properties may trigger lazy-load queries during rendering — a `foreach` over `Order.LineItems` in a view can fire one SQL query per order if the navigations were not eagerly loaded, producing N+1 issues that are invisible in development with small data. On POST, binding back to the entity type means every property on the entity is a potential attack surface: an attacker can add `IsAdmin=true` to the form body even if the view never rendered an input for it, and the binder will happily set it. Change tracking creates a third issue — if the GET action loaded a tracked entity and the POST action creates a second instance of the same type from binding, calling `_db.Update(model)` on the untracked POST instance throws `InvalidOperationException` because the tracker already has an entry for that key. Using a ViewModel with only the required fields eliminates all three problems at once.

---

## Q5. What is over-posting (mass assignment) and how do ViewModels prevent it?

**Concepts**
- Model binding setting all matching properties from POST keys
- `IsAdmin`, `DiscountPercent`, and similar sensitive fields as attack targets
- ViewModel property whitelist as the primary defense
- Server-side explicit property mapping after ViewModel binding
- `[Bind(Include = "...")]` as a weaker alternative to ViewModels

**Answer**

Over-posting happens because model binding is greedy — it sets every property on the bound type that matches a POST key, regardless of whether the HTML form included an input for it. An attacker using browser dev tools or curl can add `IsAdmin=true&DiscountPercent=100` to any form POST targeting an action that binds an `Order` entity or a ViewModel that includes those properties. ViewModels prevent this by whitelisting only the properties the form should accept — if `IsAdmin` is not a property on `OrderEditViewModel`, there is no way for it to be set through model binding. The server-side mapping step — `order.ShipDate = vm.ShipDate` — then copies only the ViewModel's declared fields onto the tracked entity, so undeclared POST keys are silently ignored at the binding stage and never reach the entity. `[Bind(Include = "...")]` on entities is a weaker alternative because it is easy to forget to update when new properties are added, and it still uses the entity type rather than a purpose-built surface.

---

## Q6. What is the difference between a Create ViewModel and an Edit ViewModel?

**Concepts**
- Create ViewModel having no id to prevent client-supplied identity
- Edit ViewModel carrying an id and treating immutable fields as read-only
- Separate authorization and validation rules per action
- Concurrency token (`RowVersion`) on edit ViewModel only
- God ViewModel for both flows risking id tampering and wrong validation

**Answer**

Create and Edit ViewModels reflect different trust boundaries, so they should be separate types. A create ViewModel has no `ProductId` — the server assigns the id, so accepting one from the client opens the door to overwriting an existing record with id 0 or a crafted value. It requires identity-establishing fields like `Sku` that must be unique on insert. An edit ViewModel carries the id (bound from the route, not a hidden field where possible), treats immutable fields like `Sku` as display-only strings not included in the POST surface, and may include a `[Timestamp] byte[] RowVersion` for optimistic concurrency. Separate action methods — `[HttpPost] Create(CreateProductViewModel)` and `[HttpPost] Edit(int id, EditProductViewModel)` — also enable distinct authorization policies, such as restricting admin-only fields to an `[Authorize(Roles = "Admin")]` edit action. Sharing one type for both flows risks id tampering on create and wrong validation on read-only fields.

---

## Q7. What is the purpose of a read-only/details ViewModel?

**Concepts**
- Display-safe fields only with no editable input surface
- Formatted strings and computed totals replacing raw FK ids
- No `asp-for` inputs that generate hidden round-trip values
- Exclusion of PII and internal data not needed for the viewer's role
- Different shape and authorization than the edit ViewModel for the same entity

**Answer**

A details ViewModel exposes only what needs to be rendered on a read-only page, specifically avoiding properties that would round-trip through hidden form inputs or expose sensitive data in the HTML source. It contains display strings like `CustomerDisplayName` and `FormattedTotal` rather than raw FK ids and navigation objects, so the view can render with `@Model.FormattedTotal` or `@Html.DisplayFor` rather than `asp-for` tag helpers that generate `<input>` elements. That distinction matters because `asp-for` on a `<input>` renders the property value inside the `value` attribute, which appears in view source — on a read page you want text nodes, not form elements. The details ViewModel also excludes internal notes, cost margins, audit timestamps, and PII not relevant to the viewer's role, since minimizing data sent to the browser is a compliance requirement independent of whether you plan to do anything with it.

---

## Q8. Why should sensitive fields (e.g., `IsAdmin`, internal margin) be excluded from ViewModels?

**Concepts**
- POST binding setting any declared property from request keys
- HTML hidden fields providing no security boundary
- Internal data in ViewModel properties leaking into HTML source
- Admin-only fields requiring separate endpoints and authorization
- Compliance minimization of data exposed to the browser

**Answer**

Any property on a ViewModel can be set by a crafted POST request, regardless of whether Razor rendered an input for it. The absence of an `<input>` for `IsAdmin` in the form template does not prevent an attacker from adding `IsAdmin=true` to the POST body — model binding reads the request, not the rendered HTML. Sensitive fields simply must not exist on user-facing ViewModels. A second problem is data leakage: even on GET, if a ViewModel includes `InternalMarginPercent`, that value appears in the HTML source — either in a rendered input's `value` attribute, in serialized JSON for JavaScript, or in hidden fields for round-tripping. PII and financial data exposed to the browser must be minimized to satisfy GDPR and other compliance requirements regardless of visual presentation. Admin operations require separate endpoints with separate ViewModels behind `[Authorize(Roles = "Admin")]` — the separation is not optional.

---

## Q9. What is the difference between mapping in the controller vs using AutoMapper?

**Concepts**
- Manual property assignment as explicit and audit-friendly
- AutoMapper convention-based profiles reducing boilerplate
- `ReverseMap()` introducing over-posting risk on POST paths
- `ProjectTo<T>()` for EF `IQueryable` projection at the database level
- Security-sensitive POST paths favoring manual mapping

**Answer**

Manual mapping in the controller assigns each property explicitly — `order.ShipDate = vm.ShipDate` — which is verbose but transparent, easy to audit for over-posting, and visible to code reviewers checking that sensitive properties are never copied. AutoMapper reduces this boilerplate through convention-based `CreateMap<TSource, TDestination>()` profiles, which is valuable for complex GET projections with many fields. The risk with AutoMapper appears on POST paths when profiles use broad `ReverseMap()` or `ForAllMembers` rules: these map every property bidirectionally, which means a reverse map from `OrderEditViewModel` back to `Order` may silently copy undeclared attack fields if someone adds them to the ViewModel later. For GET projections from EF entities into list ViewModels, AutoMapper's `ProjectTo<T>(configuration)` is excellent because it generates SQL-level projection rather than loading full entities. I use manual assignment on security-sensitive POST write paths and AutoMapper `ProjectTo` for complex read queries.

---

## Q10. What problems occur when one DTO is shared between MVC views and REST APIs?

**Concepts**
- Conflicting serialization rules between MVC and JSON API consumers
- Sensitive fields needing `[JsonIgnore]` hacks on a shared type
- API versioning breaking MVC forms and vice versa
- Display metadata and select lists polluting API DTOs
- OpenAPI schema entangling with UI validation concerns

**Answer**

A shared DTO forces conflicting requirements onto one type. API clients need camelCase JSON, full audit fields, `SupplierCost` for business intelligence, and a stable versioned contract. MVC forms need whitelisted editable properties, display names for labels, select list options, and no cost data visible in HTML source. Satisfying both with one type leads to `[JsonIgnore]` attributes to hide UI fields from the API, `[Display]` attributes that pollute the API schema, and `[Required]` rules that may mean different things in the two contexts. When the API contract needs a breaking change, it also breaks the form, and vice versa. I share mapping from the domain entity to each outward type — `CreateMap<Product, ProductApiDto>()` and `CreateMap<Product, ProductEditViewModel>()` — but keep the types separate. A thin shared record for truly common scalars like `Id`, `Name`, and `Sku` is acceptable when both consumers genuinely need the same fields with the same semantics; diverge to separate types when validation rules, field selection, or serialization behavior differ.

---

## Q11. How should navigation properties be handled in ViewModels for partial views?

**Concepts**
- Flattening navigations into display-friendly scalars
- Partial view receiving a focused ViewModel with only the fields it renders
- Upstream LINQ projection populating flattened fields
- Nested ViewModels acceptable when the partial owns that subgraph
- `@inject` + lazy loading in partials inside loops as an anti-pattern

**Answer**

The key rule is that partials should receive flat, display-ready data rather than EF navigation objects. Instead of passing `@model OrderLine` with a `Product` navigation to a line-item partial, I define `OrderLineViewModel` with `ProductName` and `Quantity` as scalar strings and populate them via LINQ projection in the controller before the view runs. This eliminates lazy-load queries inside the partial loop, removes the EF dependency from the view layer, and makes the partial independently renderable with a simple POCO. Nested ViewModels — `AddressViewModel` nested inside `CheckoutViewModel` — are fine when the partial is responsible for rendering that entire subgraph and the data is prepared upstream. What I avoid is using `@inject IProductService` inside a partial that renders inside a loop, because that turns one partial invocation into one service call per item, creating N+1 at the view layer with no batching opportunity.

---

## Q12. Where should validation attributes be placed — entity or ViewModel?

**Concepts**
- Input validation attributes belonging on the ViewModel the action binds
- Database constraints expressed in EF Fluent API on entities
- Domain invariants enforced in services or domain validators
- MVC validating the action parameter type, not the mapped entity
- `IValidatableObject` and FluentValidation for cross-field rules on ViewModels

**Answer**

Validation attributes belong on the type that MVC binds — the ViewModel — because MVC's validation pipeline runs against the action parameter type, not against any entity the ViewModel later maps into. If `[Required]` and `[StringLength]` live on the EF entity but the action parameter is a ViewModel, those annotations are bypassed during binding and only checked if you manually validate the entity, which is a separate and non-obvious step. Database constraints such as max column length and precision belong in EF Fluent API configuration since they are persistence metadata, not user input rules. Domain rules like "a discount cannot exceed 50% for non-premium customers" belong in the service layer so they apply consistently across MVC, API, and batch import paths. I use `IValidatableObject` on ViewModels for cross-field rules that involve two properties from the same form, and FluentValidation for larger rule sets that would clutter the ViewModel class with attributes.

---

## Q13. What is the difference between presentation logic and business logic in ViewModels?

**Concepts**
- Presentation logic formatting data for display
- Business logic enforcing domain rules
- Computed display labels as acceptable ViewModel properties
- DB calls and domain checks as unacceptable in ViewModels
- UI state properties (`SelectedCategoryId`, select lists) vs transactional behavior

**Answer**

Presentation logic in a ViewModel is acceptable — things like `public string StatusLabel => IsActive ? "Active" : "Inactive"` or `[Display(Name = "Ship Date")]` metadata that shape how the view renders existing data. These are pure transformations of already-validated, already-computed values. Business logic is not acceptable in ViewModels: pricing caps, inventory checks, discount calculations, and authorization decisions depend on domain rules, service state, or database queries that must be enforced consistently across all entry points into the system. A computed `LineTotal` property that calls `GetDiscountedPrice()` with a database access is business logic that belongs in the service layer, tested independently, and the result stored in a ViewModel property rather than computed in the ViewModel itself. ViewModels may also hold UI state like `SelectedCategoryId` and `AvailableCategories` to support select lists — that is presentation infrastructure, not behavior.

---

## Q14. How do ViewModels help with unit testing controllers?

**Concepts**
- POCO ViewModel constructed directly in tests
- No EF, SQL, or Razor required to test controller logic
- Asserting on `ViewResult.Model` type and property values
- Mock services returning entities; controller mapping to ViewModels
- Tests focused on HTTP orchestration and mapping correctness

**Answer**

ViewModels are plain POCOs, so tests can construct them directly, pass them to controller actions, and assert on the returned `ViewResult` without spinning up Entity Framework, SQL Server, or the Razor rendering pipeline. The test arranges a ViewModel with specific values — `var vm = new EditProductViewModel { ProductId = 1, Name = "" }` — calls the action, and asserts on `controller.ModelState.IsValid`, `result.Model`, and redirect destinations. Mock services return entities; the test verifies that the controller maps them correctly to ViewModels and that validation gates work as expected. This isolation is only possible because ViewModels decouple the controller from persistence types: a controller action that takes `Order` as its parameter requires a real or fake EF context, while one that takes `OrderEditViewModel` requires only a mock service returning an `Order` entity for the read case and accepting a command for the write case.

---

## Q15. What is a composite/page ViewModel and when might you split it?

**Concepts**
- Composite ViewModel aggregating multiple UI regions for one page
- God ViewModel risks: merge conflicts, authorization leaks, expensive queries
- View Components with focused ViewModels as the decomposition strategy
- Page shell holding layout metadata while widgets load independently
- Avoiding unnecessary AJAX round-trips through `Component.InvokeAsync`

**Answer**

A composite ViewModel aggregates multiple UI regions — an orders grid, a profile card, a notification count, a chart series — into one `@model` so the controller loads everything in one action and the layout renders with a single model type. This works well for pages with a few related data sets. The problems begin when the ViewModel grows to 40+ properties and several nested lists: merge conflicts multiply, code reviewers cannot easily trace which widget uses which property, and partials that receive the entire page model expose data to widgets that should not see it (authorization leaks). I split at widget boundaries when a section has its own data loading, authorization context, or is reused across pages — each widget becomes a View Component that injects its own dependencies and loads its data in `InvokeAsync`. The page shell ViewModel holds only route metadata and layout settings, while `@await Component.InvokeAsync("OrdersGrid")` handles the heavy lifting per widget, all in a single page-load request without additional AJAX round-trips.

---

## Q16. Why can returning an entity to `PartialView` cause serialization errors?

**Concepts**
- Bidirectional navigation properties forming circular object graphs
- `System.Text.Json` cycle detection throwing `JsonException`
- Lazy-loaded navigations causing unpredictable query counts in partials
- `ReferenceHandler.IgnoreCycles` as a band-aid rather than a fix
- Flat ViewModel breaking the cycle by design

**Answer**

EF entities with bidirectional navigations — `Order.Customer.Orders` pointing back to the collection that contains the original order — form circular object graphs that `System.Text.Json` cannot serialize without a cycle policy, throwing `JsonException: A possible object cycle was detected`. This surfaces when the same partial is reused by a SignalR hub or an AJAX endpoint that returns JSON, both of which hit the serializer rather than just the Razor renderer. Even in pure Razor rendering, lazy-loaded navigations accessed inside the partial can trigger unexpected database queries, and the graph traversal in debugging or custom helpers can follow circular references into infinite loops. The fix is to define a flat `OrderSummaryViewModel` with `OrderId`, `CustomerName`, `Total`, and `Status` as scalar properties, project to it with LINQ before the action returns, and pass the ViewModel to the partial. Configuring `ReferenceHandler.IgnoreCycles` on the serializer options quiets the exception but does not fix the underlying over-fetch and does not help with Razor rendering issues.

---

## Q17. What is `[BindNever]` and when is it used on ViewModels?

**Concepts**
- `[BindNever]` excluding a property from inbound POST binding
- Protecting server-populated properties from tampered POST keys
- Preference for omitting sensitive properties entirely over opt-out
- `[BindNever]` not preventing GET rendering
- Pairing with `[ValidateNever]` for properties that should neither bind nor validate

**Answer**

`[BindNever]` tells the model binder to skip a property when processing POST requests — the binder will not set that property from form keys even if they are present in the request. On ViewModels it protects properties that are populated server-side on GET and should never be overwritten from user input: dropdown list collections (`AvailableCategories`), server-assigned route ids, or display-only timestamps populated before the view renders. I prefer the stronger approach of simply not including sensitive properties on the ViewModel at all — if `IsAdmin` is not declared, there is no property to bind and no `[BindNever]` needed. `[BindNever]` is most useful on hybrid ViewModels where the same type is used for both GET (where it needs a display-only property) and POST (where that property must not be writable). It does not affect GET rendering — the property still renders normally in Razor; it only suppresses the inbound binding step.

---

## Q18. How do nullable reference types affect ViewModel design?

**Concepts**
- Optional form fields requiring `string?` to match binding behavior
- Required fields needing `[Required]` plus `= ""` initializer or `required` modifier
- Model binding setting omitted optional fields to `null`
- `CS8618` warning from non-nullable reference without initializer
- NRT reflecting HTML form semantics, not database nullability

**Answer**

Nullable reference types must align with what HTML forms actually send — not with database schema nullability, which is a different concern. When a form omits an optional field like `MiddleName`, model binding sets it to `null`, so the property must be declared as `string?` or the app gets a `NullReferenceException` on the first `.Trim()` or `.Length` call, despite the compiler's `CS8618` warning being suppressed by a non-nullable declaration. Required fields — ones with `[Required]` — should be declared as `string FirstName { get; set; } = ""` with an initializer, so the NRT system and model binding agree: the empty string is the default before binding sets the real value, and `[Required]` rejects the empty string as invalid. The `#nullable enable` / `<Nullable>enable</Nullable>` setting in the project amplifies these issues from warnings to errors, which is why enabling NRT on a ViewModel-heavy app often reveals a batch of hidden binding mismatches. The rule of thumb is: if the HTML form field is optional, use `string?`; if it is required, use `string` with an initializer and `[Required]`.

---

## Gotchas — ViewModels & Strongly Typed Views (Interview Traps)

---

#### Gotcha 1. Fat ViewModels containing business logic or methods

**Concepts**
- ViewModel responsibility — flat data contract for the view, nothing more
- `ApplyBusinessRules()` on ViewModel — policy hidden in UI layer, bypassed by API endpoints
- Business methods on ViewModel — duplicates service layer and is untestable in isolation
- Thin ViewModel — plain properties mapped from domain in the controller or mapping service

**Answer**

A ViewModel with methods like `ApplyBusinessRules()` or `CalculateDiscount()` encodes business logic in the UI layer. Any API endpoint or background job that creates the same entity without going through the ViewModel bypasses those rules, causing silent correctness divergence. Business logic belongs in domain services or application services called by both MVC controllers and API controllers before mapping to a ViewModel. The ViewModel should be a flat data-transfer object containing exactly the properties the view renders — no computed policy, no mutation logic, and no external service calls. If a ViewModel property requires a computed value, compute it in the controller or mapping service and set it as a plain property.

---

#### Gotcha 2. Using the same ViewModel for list display and form editing

**Concepts**
- List ViewModel — read-only display fields, possibly paginated, no validation annotations
- Edit ViewModel — writable fields, validation attributes, file upload support
- Shared ViewModel — `[Required]` on display-only fields causes false `ModelState` failures
- Command/Query separation — `ProductListViewModel` vs `ProductEditViewModel` per purpose

**Answer**

Sharing one ViewModel between a list display action and an edit form creates conflicting annotation pressure: `[Required]` on `Name` is correct for the edit form but irrelevant and harmful for the list view that populates `Name` from the database. When the list action binds to the shared type through a search form, the `[Required]` on display-only fields causes `ModelState.IsValid` to fail even with valid search input. The correct pattern is separate ViewModels per purpose: `ProductListViewModel` for display with `IReadOnlyList<ProductRow>`, and `ProductEditViewModel` for the form with `[Required]`, `[MaxLength]`, and other input-validation annotations applied only to the fields the user edits.

---

#### Gotcha 3. `IEnumerable<T>` collection binding in forms producing index gaps

**Concepts**
- Model binder — expects contiguous zero-based indices `Items[0]`, `Items[1]`, `Items[2]`
- Row deletion — leaves gaps like `Items[0]`, `Items[2]`, causing binder to stop at first gap
- Client-side reindexing — JavaScript must renumber remaining rows after deletion
- Dictionary binder — key-value approach tolerates non-contiguous keys as an alternative

**Answer**

When a Razor form renders `name="Items[0].Qty"`, `name="Items[1].Qty"`, `name="Items[2].Qty"` and the user deletes the middle row client-side, the remaining rows post as `Items[0]` and `Items[2]`. The default model binder stops collecting at the first missing index — `Items[1]` is absent, so `Items[2]` and beyond are silently dropped. The fix is to reindex remaining rows with JavaScript after each deletion so indices are always contiguous starting at zero. Alternatively, use the dictionary-key approach where each row posts `Items[rowGuid].Qty` with a unique key, then implement a custom `IModelBinder` that tolerates non-contiguous keys. Never rely on the browser's rendered index staying valid after dynamic row operations.

---

#### Gotcha 4. Forgetting to re-populate dropdown data on validation failure

**Concepts**
- Select list data — loaded in GET action, not preserved in POST body or `ModelState`
- POST with validation failure — `return View(model)` re-renders the form
- Null `SelectListItems` — throws `NullReferenceException` or renders empty dropdown
- Helper method — call `PopulateDropdowns(model)` before every `return View(model)` path

**Answer**

A `CreateProductViewModel` has a `CategoryId` int and a `Categories IEnumerable<SelectListItem>` populated in the GET action. When the POST action returns `View(model)` on validation failure, the `model` was bound from the POST body — which contains no `Categories` data. The view tries to render the dropdown with `Model.Categories` which is null, throwing a `NullReferenceException`. The fix is to re-populate all dropdown and select data before returning the view on failure. A private helper method `PopulateDropdowns(model)` called at the end of the GET action and before every `return View(model)` in the POST action is the standard pattern. Missing this is one of the most common MVC form bugs.

---

#### Gotcha 5. Nullable reference type warnings suppressed rather than handled properly

**Concepts**
- `#nullable enable` — project-level NRT enforcement in .NET 6+
- `= null!` null-forgiving initializer — suppresses warning without preventing runtime null
- POST body absence — model binder can still produce null for `string` properties
- Defensive approach — `string?` with null checks in Razor, or `[Required]` to enforce non-null

**Answer**

In projects with nullable reference types enabled, marking a ViewModel property as `string Name { get; set; } = null!;` suppresses the NRT warning but the model binder can still produce null if the form field is absent from the POST body or a binding error occurs. At runtime `@Model.Name.Length` in Razor throws `NullReferenceException`. The fix is to treat ViewModel properties defensively: use `string?` with null checks in the view, add `[Required]` to enforce non-null at the binding layer, or initialize to `""` so the property is empty string rather than null when the field is missing. Collection properties should always be initialized to empty lists so the view does not need null checks on `foreach` loops.

---

#### Gotcha 6. Navigation properties on ViewModels causing circular reference during serialization

**Concepts**
- Circular ViewModel — `OrderViewModel.Customer` with `CustomerViewModel.Orders` back-reference
- `System.Text.Json` — throws `JsonException: A possible object cycle was detected`
- `Newtonsoft.Json` — enters infinite recursion on circular object graphs
- Flat ViewModel — scalar properties only, no circular navigation links

**Answer**

A `CustomerViewModel` with an `Orders` property containing `OrderViewModel` objects each referencing back to `CustomerViewModel` creates a circular object graph. When the controller calls `return Json(vm)` or the view engine serializes it for inline model data, `System.Text.Json` throws `JsonException: A possible object cycle was detected` and `Newtonsoft.Json` enters infinite recursion. ViewModels for server-rendered MVC views do not need circular navigation references — the view renders scalar properties. The correct fix is to break the cycle: `CustomerViewModel` contains `OrderSummary[]` (id, date, total only) with no back-reference, and `OrderDetailViewModel` contains a flat `string CustomerName` rather than a nested `CustomerViewModel`.

---

#### Gotcha 7. Nested ViewModel objects not initialized causing `NullReferenceException` in Razor

**Concepts**
- `public AddressViewModel Address { get; set; }` — null by default without initialization
- `return View(new CustomerViewModel())` — leaves `Address = null`
- `@Model.Address.City` — throws `NullReferenceException` when Address is null
- Initialize in constructor — always-valid ViewModel objects prevent the entire null-ref class

**Answer**

A `CustomerViewModel` with `public AddressViewModel Address { get; set; }` has a null `Address` unless the action initializes it. `return View(new CustomerViewModel())` leaves `Address` at its type default of null. The Razor view accessing `@Model.Address.City` then throws `NullReferenceException`. The fix is to initialize all nested ViewModel properties in the constructor: `public CustomerViewModel() { Address = new AddressViewModel(); }` or in an object initializer in the action. Collection properties should always be initialized to empty lists: `public List<OrderItemViewModel> Items { get; set; } = new();`. Always-valid ViewModel objects prevent the entire class of null-ref errors that only surface when specific routes are tested.

---

#### Gotcha 8. Using `Id` in the ViewModel allowing IDOR attacks via POST body tampering

**Concepts**
- Edit ViewModel with `Id` — attacker can submit any resource id in the POST body (IDOR)
- Route `id` parameter — more trustworthy than a hidden form field
- Bind exclusion — exclude `Id` from binding; read only from the route
- Ownership check — verify the route id belongs to the current user before saving

**Answer**

A `ProductEditViewModel` with a public `Id` property allows a user to submit any product id in the form body and update a product they do not own. Even if the GET form sets the correct `Id` as a hidden field, a tampered POST body with a different `Id` binds that id and updates the wrong record — an Insecure Direct Object Reference (IDOR) vulnerability. The correct pattern is to take the resource identifier only from the route parameter — `[HttpPost("{id:int}")]` — and ignore or omit `Id` from the ViewModel entirely. Always verify the route `id` matches a resource owned by the current user before saving, using `IAuthorizationService` or a manual ownership check.

---

#### Gotcha 9. Fat ViewModel loading all possible fields for all possible views

**Concepts**
- Shared ViewModel with 40 properties — forces loading all data for every view
- N+1 from eager loading — navigations loaded for properties the list view never renders
- Purpose-built ViewModel per action — minimal query, minimal serialization overhead
- `SELECT *` risk — schema changes add properties that views and serializers expose

**Answer**

A shared `CustomerViewModel` with 40 properties serving the list, detail, and edit views forces the controller to load all associated data — addresses, orders, contacts, payment methods — even when the list view renders only `Name` and `Email`. The EF query must eagerly load all navigations to avoid lazy-load exceptions, multiplying joins and data transfer. Purpose-built ViewModels per action keep queries minimal: `CustomerListItemViewModel` has `Id`, `Name`, `Email`, and `Status`; `CustomerEditViewModel` has the editable form fields; `CustomerDetailViewModel` has the display fields plus summary counts. This also makes `SELECT` queries faster and removes dead data from the HTTP response body if the endpoint returns JSON.

---

#### Gotcha 10. Mixing `ViewBag` alongside a strongly typed ViewModel for supplemental data

**Concepts**
- `@model ProductViewModel` — typed ViewModel for main view data
- `ViewBag.PageTitle` alongside ViewModel — splits view contract into typed and untyped channels
- Magic string keys — `ViewBag.PageTitle` typo produces null silently, no build error
- Consolidated ViewModel — all view data as properties on the ViewModel or a base class

**Answer**

Mixing a strongly typed `@model ProductViewModel` with `ViewBag.PageTitle` and `ViewData["Breadcrumb"]` splits the view's data contract between a typed part and an untyped dictionary. The typed part is safe and IntelliSense-supported; the dictionary part is invisible to the compiler, prone to key-name typos, and loses type information when read in Razor. When the controller changes `ViewBag.PageTitle` to `ViewBag.Title`, the layout's `@ViewBag.PageTitle` silently returns null with no build error. The fix is to consolidate all view data — page title, breadcrumbs, any cross-cutting metadata — as properties on the ViewModel or a base ViewModel class. Every piece of data the view needs should come from `@Model`, not from a parallel `ViewBag` channel.

---
## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- EF entity as view model exposing persistence graph to HTML
- Change-tracker conflict between GET-loaded and POST-bound instances
- `_db.Update(untrackedStub)` throwing when GET instance is still tracked
- ViewModel whitelisting as the architectural fix
- `RowVersion` concurrency token requiring explicit handling

**Answer**

There are three overlapping problems here. First, `@model Order` exposes `InternalMarginPercent`, `Customer` navigation, and `RowVersion` to the Razor template — these appear in the HTML source in input values, which is a data leakage issue regardless of what the view actually renders. Second, the GET action loads a tracked `Order` into the EF change tracker; when the POST action calls `_db.Orders.Update(model)` with the untracked, model-bound instance of the same entity type and the same primary key, EF throws `InvalidOperationException` because it cannot track two instances with the same key in one `DbContext` lifetime. Third, binding directly to `Order` means any property on that entity — including `InternalMarginPercent` — can be set from crafted POST keys.

The fix is to introduce a dedicated `OrderEditViewModel` with only the editable fields (`Id`, `ShipDate`, display-only customer name). On GET, load the tracked entity and map it to the ViewModel before passing to the view. On POST, load the tracked entity with `Find` or `FirstAsync`, then map only the ViewModel's declared fields onto the tracked instance before calling `SaveChangesAsync` — never call `_db.Update` on an untracked POST-bound object when a GET-loaded tracked instance may still be in scope. If concurrency protection is needed, include `RowVersion` as a `byte[]` property on the ViewModel, copy it through to the tracked entity, and catch `DbUpdateConcurrencyException` in the POST action.

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

**Concepts**
- `ReverseMap()` creating a bidirectional map including all entity members
- ViewModel whitelist illusion from absent properties at binding stage
- AutoMapper mapping ViewModel → entity writing all matching entity properties
- One-way `CreateMap` for GET vs explicit allow-list map for POST
- Integration test asserting undeclared POST fields remain unchanged

**Answer**

The ViewModel correctly omits `IsPriority` and `DiscountPercent` — model binding will not set them on `OrderEditViewModel` since those properties do not exist on it. The problem is one step later: `_mapper.Map(vm, order)` with a profile that uses `ReverseMap()` generates a reverse map from `OrderEditViewModel` back to `Order` that covers every property AutoMapper can match by convention. When AutoMapper maps `vm` onto the existing tracked `order` instance, it updates every `Order` property that has a matching source, and since `Order` has `IsPriority` and `DiscountPercent` while the source `vm` does not, AutoMapper leaves them at whatever value the tracked entity currently holds — which sounds safe but is not, because the attacker's extra POST fields could have been used to overwrite the tracked entity before AutoMapper runs in some configurations, or other code paths could be exploited.

More critically, the `ReverseMap()` profile is a standing risk: anyone who later adds `IsPriority` to `OrderEditViewModel` (even temporarily) immediately makes it writable through this action. The fix is to remove `ReverseMap()` and define explicit one-way maps — `CreateMap<Order, OrderEditViewModel>()` for GET projection only — and on POST, assign properties explicitly: `order.ShipDate = vm.ShipDate`. Adding an integration test that POSTs `IsPriority=true` and asserts the database value remains unchanged is the safeguard that catches regressions when the profile is modified.

---

#### Q3. (D) A team shares one `ProductDto` between the REST API (`ProductsController`) and MVC admin screens (`ProductsController` in Areas/Admin). API clients need `SupplierCost` and audit timestamps; the browser form must not expose them. What breaks if you keep one type, and how do you split responsibilities without duplicating every field?

**Concepts**
- Conflicting serialization and exposure rules on one type
- `[JsonIgnore]` hacks leaking sensitive fields into HTML on MVC path
- API versioning breaking MVC forms on a shared contract
- Separate `ProductApiDto` and `ProductEditViewModel` mapped from entity
- Thin shared record for genuinely common scalars

**Answer**

Keeping one type forces irreconcilable compromises. `SupplierCost` and audit timestamps need to appear in JSON for API clients, so they must be properties on the type — but they also then appear in the HTML source of the MVC admin form as input values, which is a compliance risk. Adding `[JsonIgnore]` hides them from the API response but not from the Razor renderer, and vice versa. When the API needs a breaking change (adding a new required field, renaming a property for a new API version), it also changes the MVC form's model shape, potentially breaking validation attributes, label text, or form structure. Display metadata like `[Display(Name = "Product Name")]` and select-list properties for dropdowns do not belong on API DTOs, while OpenAPI schema annotations do not belong on ViewModels.

The solution is two separate outward types mapped independently from the `Product` entity: `ProductApiDto` for the REST layer (includes `SupplierCost`, audit fields, camelCase JSON contract, OpenAPI annotations) and `ProductEditViewModel` for the MVC admin form (only editable fields, `[Display]` metadata, dropdown lists, no cost data). Both are created by projecting or mapping from the domain entity in their respective controller layers. If `Id`, `Name`, and `Sku` are genuinely identical in both types with no semantic difference, a thin shared record for those three scalars is reasonable — but diverge the moment validation rules, field visibility, or serialization behavior differs between the two consumers.

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

**Concepts**
- Shared ViewModel merging incompatible create and edit trust boundaries
- Client-supplied `ProductId` enabling id tampering on create
- Same validation rules applied to immutable fields on edit
- Separate `CreateProductViewModel` and `EditProductViewModel` as the fix
- Route-bound id comparison as tamper protection on edit

**Answer**

The `vm.ProductId == 0` check is the architectural tell — the action is trying to serve two incompatible purposes with one type, and the discrimination is based on a client-supplied value that an attacker can set to any existing product id to overwrite it. A create request with `ProductId=42` will load the product with id 42 from the database and overwrite it with the submitted data, because the `vm.ProductId == 0` guard fails.

The fix is to split into `CreateProductViewModel` (no `ProductId`, required `Sku`) and `EditProductViewModel` (id comes from the route, `Sku` is display-only and not posted, optional `IsDiscontinued` only for admin roles). Separate actions — `[HttpPost] Create(CreateProductViewModel)` and `[HttpPost] Edit(int id, EditProductViewModel)` — receive the correct type for each operation. On create, the server assigns the id; on edit, the route-supplied `id` is compared to any hidden field value for tamper detection before loading the entity. `[Authorize(Roles = "Admin")]` can be applied to the edit action independently to restrict `IsDiscontinued` changes. Separate types also mean separate validation attributes — `Sku` is `[Required]` on create and absent on edit, so no cross-contamination of rules.

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

**Concepts**
- `<input asp-for>` emitting `value` attribute with model data in HTML source
- Edit view scaffold generating form inputs copied to a read-only details page
- `@Html.DisplayFor` and plain `@Model.Property` for read-only rendering
- ViewModel correctly excluding sensitive fields at the type level
- Details view using display helpers, not form input helpers

**Answer**

The ViewModel correctly excludes `Phone`, `Email`, and `InternalNotes` — those fields are not on `OrderDetailsViewModel` and AutoMapper will not map them. The phone numbers and internal notes appearing in view source are coming from a different path: the view was copied from the Edit scaffold, which generates `<input asp-for="CustomerDisplayName" />`. The `asp-for` tag helper on an `<input>` element renders the property value inside the HTML `value` attribute — `<input type="text" id="CustomerDisplayName" name="CustomerDisplayName" value="Acme Corp">` — which appears verbatim in view source.

The fix is to replace the scaffold inputs with display-only markup. For a read-only details page, use `@Html.DisplayFor(m => m.CustomerDisplayName)` or simply `@Model.CustomerDisplayName` inside a `<span>` or `<dd>` element. These render only the text node — no `name`, no `value` attribute, no form element that could carry data or be submitted. Remove the `<form>` wrapper entirely since details pages are not POSTing anything. The broader rule is that details pages should never use `asp-for` on `<input>` elements — that tag helper is for edit forms, not display pages.

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

**Concepts**
- Non-nullable `string` declaration conflicting with binding setting `null` for omitted fields
- `string?` matching HTML optional field semantics
- Null-conditional `?.Trim()` for optional string handling
- `= ""` initializer satisfying NRT for required strings
- `CS8618` warning from non-nullable reference without initializer

**Answer**

`#nullable enable` without `?` on `MiddleName` tells the compiler that the property is never null — but model binding does not consult the NRT annotation. When the form omits the optional `MiddleName` field, the binder sets it to `null`, so `model.MiddleName.Trim()` throws `NullReferenceException` at runtime despite the compiler seeing no warning. The NRT declaration created a false sense of safety.

The optional `MiddleName` field must be declared as `string?` to match what model binding actually produces for omitted optional inputs, and the `.Trim()` call must use the null-conditional operator: `model.MiddleName?.Trim() ?? ""`. The required strings (`FirstName`, `LastName`, `Email`) should be declared with an empty string initializer — `public string FirstName { get; set; } = ""` — plus `[Required]` to satisfy NRT (preventing `CS8618`) while letting `[Required]` enforce the non-empty contract. The optional field round-trip problem — value lost when returning `View(model)` after a validation error — is also fixed by `string?` since `ModelState` preserves the null value correctly and re-renders the empty input without throwing. Enabling `<Nullable>enable</Nullable>` in the project file and treating `CS8618` warnings as errors surfaces these mismatches at build time.

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

**Concepts**
- Circular navigation graph causing `JsonException` on serialization
- Entity as partial view model coupling Razor to EF graph shape
- `Include` over-fetching full graph for a summary display
- Flat `OrderSummaryViewModel` breaking the cycle by design
- `ReferenceHandler.IgnoreCycles` as a band-aid, not a fix

**Answer**

The Razor partial works on the first load because the Razor renderer does not serialize the object graph — it reads individual properties. The 500 errors on subsequent calls come from the SignalR hub path, which serializes the `Order` entity to JSON. `Order.Customer.Orders` creates a cycle — the customer navigation holds a collection that contains the original order, which System.Text.Json detects and refuses by default. The intermittent pattern (first load works) happens because the first request hits only the Razor path and the second triggers the hub broadcast.

The fix is to define `OrderSummaryViewModel` with flat scalar fields — `OrderId`, `CustomerName`, `Total`, `Status` — and project to it with LINQ: `.Select(o => new OrderSummaryViewModel { OrderId = o.Id, CustomerName = o.Customer.Name, Total = o.LineItems.Sum(l => l.Qty * l.Price), Status = o.Status })`. No `Include` is needed since the LINQ projection traverses navigations in a single SQL query without materializing entity graphs. Both the partial view and the SignalR hub then work with the flat ViewModel, which has no circular references and serializes cleanly. Setting `ReferenceHandler.IgnoreCycles` on the JSON serializer options silences the exception but does not prevent the over-fetch, and does not help if cycles cause infinite rendering in other contexts.

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

**Concepts**
- Data annotations on EF entity at the wrong layer for UI validation
- MVC validating action parameter type, not the mapped entity
- `$50k` line total cap as a domain/service rule, not a column constraint
- Input validation attributes belonging on the ViewModel the action binds
- Service-layer enforcement required regardless of ViewModel validation

**Answer**

There are two separate problems. First, `[Range]` and `[Required]` on the `Order` entity are in the wrong layer — MVC validates the action parameter type (`OrderLineViewModel`), so entity annotations are bypassed entirely when the ViewModel does not declare them. The ViewModel has no `[Range]` on `UnitPrice` or `Quantity`, which means the controller accepts any values. Changing the entity annotation does not affect the form's validation behavior and incorrectly coupling a presentation rule to a persistence type forces database migrations when business rules change. Second, the $50k line total cap is a domain invariant that cannot be expressed as a single-property annotation — `LineTotal` is a computed property, and `[Range]` does not apply to computed values in the MVC validation pipeline.

The fix is to move input validation to `OrderLineViewModel`: add `[Range(0.01, 9999.99)]` on `UnitPrice` and `[Range(1, 100)]` on `Quantity`, then implement `IValidatableObject.Validate` or a FluentValidation rule to check `UnitPrice * Quantity <= 50000`. Remove presentation validation from the entity and keep only EF Fluent API constraints (column precision, max length) there. After the ViewModel passes `ModelState.IsValid`, the service layer should also check the business rule — service-layer validation is the authoritative gate regardless of what the ViewModel validates, because the service may be called from paths other than the MVC form.

---

#### Q9. (D) A dashboard action builds one ViewModel for a page with orders grid, user profile card, notification feed, and chart series. The type has 40+ properties and three nested lists. Refactors are painful and partial views reuse the whole model. What are the concrete risks, and how would you decompose without fragmenting the page into dozens of controller round-trips?

**Concepts**
- God ViewModel merge conflicts and ownership ambiguity
- Authorization leaks when partials receive the entire page model
- Expensive single query loading all widgets regardless of visibility
- View Components with focused ViewModels as the decomposition unit
- Single page-load with `Component.InvokeAsync` avoiding AJAX fragmentation

**Answer**

The concrete risks of a 40-property god ViewModel are authorization leaks, merge conflicts, and expensive all-or-nothing data loading. When a partial receives the entire page model and renders the orders grid, it has access to the user's salary data, notification content, and chart series that the orders partial should not see — a developer refactoring one section may accidentally render another section's sensitive data. Merge conflicts compound as multiple feature teams add properties to the same class. The data loading for all four widgets happens in one action, so a slow chart-series query blocks the entire page even when the user only needed the profile card.

I would decompose using View Components — one per widget: `OrdersGridViewComponent`, `ProfileCardViewComponent`, `NotificationFeedViewComponent`, `ChartSeriesViewComponent`. Each View Component declares its own focused ViewModel type, injects only the services it needs, loads its data in `InvokeAsync`, and renders its own `Default.cshtml` partial. The page shell action sets only layout metadata (`PageTitle`, breadcrumbs) and the Razor view calls `@await Component.InvokeAsync("OrdersGrid")` for each widget. All four components execute within the same HTTP request — there are no extra AJAX round-trips — but each loads exactly the data it needs, can be authorized independently, and can be tested as an isolated class. Only if a widget is genuinely slow (chart series with a 2-second query) do I convert it to lazy AJAX loading; the rest stay synchronous View Components in the single page load.

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

**Concepts**
- `Include` materializing full entity graph instead of projecting to ViewModel columns
- AutoMapper in-memory mapping not replacing SQL-level projection
- `ProjectTo<T>()` translating LINQ to SQL projection
- `AsNoTracking()` for read-only list queries
- Pagination as a mandatory companion to any list projection

**Answer**

The timeout comes from `.Include(Customer).Include(LineItems).ToList()` — this loads every column of every `Order`, every `Customer`, and every `LineItem` for 2,000 rows into memory, then AutoMapper transforms that bloated in-memory graph into a four-field ViewModel. AutoMapper does not replace SQL projection; it is an in-memory operation that runs after all the data has already been fetched. The `ReverseMap()` profile is also a standing security risk on this query, as noted elsewhere.

The fix is to project to the ViewModel directly in EF using LINQ:

```csharp
var vms = await _db.Orders
    .OrderByDescending(o => o.CreatedUtc)
    .Select(o => new OrderListItemViewModel
    {
        OrderId = o.Id,
        CustomerName = o.Customer.Name,
        Total = o.LineItems.Sum(l => l.Qty * l.Price),
        Status = o.Status
    })
    .ToListAsync();
```

No `Include` is needed because the LINQ `Select` traverses navigations and EF translates the whole projection to a single SQL query with a JOIN and subquery for the sum. Adding `.AsNoTracking()` before `Select` further reduces overhead on a read-only list. The query should also be paginated with `.Skip(page * size).Take(size)` — loading 2,000 rows even as a slim ViewModel is unnecessary and will time out again as the table grows. If AutoMapper `ProjectTo<OrderListItemViewModel>(_mapper.ConfigurationProvider)` is preferred over a manual `Select`, it achieves the same SQL projection and avoids the in-memory mapping cost.
