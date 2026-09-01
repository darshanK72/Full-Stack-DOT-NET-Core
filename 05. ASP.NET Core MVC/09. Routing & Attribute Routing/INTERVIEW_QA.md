# Routing & Attribute Routing — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 09. Routing & Attribute Routing](#chapter-09-routing-attribute-routing)
  - [Q1. What is conventional routing in ASP.NET Core MVC?](#chapter-09-routing-attribute-routing-q1)
  - [Q2. What is the default route pattern `{controller=Home}/{action…](#chapter-09-routing-attribute-routing-q2)
  - [Q3. What is attribute routing in MVC controllers?](#chapter-09-routing-attribute-routing-q3)
  - [Q4. What is the difference between conventional routing and attr…](#chapter-09-routing-attribute-routing-q4)
  - [Q5. What are route constraints (e.g., `:int`, `:exists`)?](#chapter-09-routing-attribute-routing-q5)
  - [Q6. Why does route registration order matter in `MapControllerRo…](#chapter-09-routing-attribute-routing-q6)
  - [Q7. How does the `{area:exists}` constraint work?](#chapter-09-routing-attribute-routing-q7)
  - [Q8. What is the difference between `[Route]` on a controller vs …](#chapter-09-routing-attribute-routing-q8)
  - [Q9. What does a leading slash in `[HttpGet("/export/{year}")]` m…](#chapter-09-routing-attribute-routing-q9)
  - [Q10. What is a catch-all route parameter (`{*slug}`)?](#chapter-09-routing-attribute-routing-q10)
  - [Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action …](#chapter-09-routing-attribute-routing-q11)
  - [Q12. What happens when two actions match the same route?](#chapter-09-routing-attribute-routing-q12)
  - [Q13. How does Tag Helper link generation (`asp-controller`, `asp-…](#chapter-09-routing-attribute-routing-q13)
  - [Q14. Why must `area` be specified when generating links to area c…](#chapter-09-routing-attribute-routing-q14)
  - [Q15. What is `LowercaseUrls` and how does it affect link generati…](#chapter-09-routing-attribute-routing-q15)
  - [Q16. What is the difference between "no route matched" and "405 M…](#chapter-09-routing-attribute-routing-q16)
  - [Q17. How do optional route parameters (`{id?}`) and defaults inte…](#chapter-09-routing-attribute-routing-q17)
  - [Q18. What is the areas route pattern and how does it differ from …](#chapter-09-routing-attribute-routing-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 09. Routing & Attribute Routing

### Q1. What is conventional routing in ASP.NET Core MVC? {#chapter-09-routing-attribute-routing-q1}

What is conventional routing in ASP.NET Core MVC?

**Answer:** Conventional routing maps URLs to controllers and actions using route templates registered in `Program.cs` with `MapControllerRoute`, typically `{controller=Home}/{action=Index}/{id?}`. The dispatcher extracts segment values and invokes the matching action method.

- Route names and patterns are centralized in endpoint configuration rather than on controller classes.
- Defaults supply `Home` and `Index` when segments are omitted, making `/` resolve to `HomeController.Index`.
- Multiple named routes can coexist; the endpoint routing middleware selects the first matching template by registration order and specificity.
- Conventional routing remains the default in MVC templates alongside optional attribute routes on controllers.

---

### Q2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`? {#chapter-09-routing-attribute-routing-q2}

What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?

**Answer:** This template maps the first URL segment to a controller name (without the `Controller` suffix), the second to an action method name, and an optional third `{id}` segment to an action parameter named `id`.

- `/` uses defaults → `HomeController.Index()` with `id = null`.
- `/Products/Details/42` → `ProductsController.Details(42)` when such an action exists.
- Matching is case-insensitive by default for controller and action names in ASP.NET Core 8.
- The optional `{id?}` token allows actions with or without an `id` parameter on the same template.

---

### Q3. What is attribute routing in MVC controllers? {#chapter-09-routing-attribute-routing-q3}

What is attribute routing in MVC controllers?

**Answer:** Attribute routing decorates controllers and actions with `[Route]`, `[HttpGet]`, `[HttpPost]`, and related attributes to define URL templates directly on the types that handle them. `MapControllers()` in `Program.cs` discovers these routes at startup.

- Controller-level `[Route("api/[controller]")]` prefixes all actions in that controller.
- Action-level `[HttpGet("{id:int}")]` adds HTTP method constraints and parameter tokens.
- Attribute routes can coexist with conventional routes in the same application.
- Explicit templates are preferred for REST-style APIs and marketing-friendly fixed URLs within MVC apps.

---

### Q4. What is the difference between conventional routing and attribute routing? {#chapter-09-routing-attribute-routing-q4}

What is the difference between conventional routing and attribute routing?

**Answer:** Conventional routing defines URL patterns centrally in `Program.cs` and infers controller/action from segments. Attribute routing colocates URL templates on controller classes, making each action's public URL explicit in code.

- Conventional routing suits traditional `{controller}/{action}` sites with consistent URL shapes.
- Attribute routing suits fine-grained control, versioning prefixes, and literal segments that would collide with conventional defaults.
- Link generation with Tag Helpers works with both; attribute routes require matching template tokens in `asp-route-*` values.
- Mixing both requires careful ordering — attribute routes and conventional routes compete in the same endpoint route table.

---

### Q5. What are route constraints (e.g., `:int`, `:exists`)? {#chapter-09-routing-attribute-routing-q5}

What are route constraints (e.g., `:int`, `:exists`)?

**Answer:** Route constraints restrict which values a route parameter accepts. `:int` requires an integer; `:guid`, `:alpha`, `:minlength(n)`, and custom `IRouteConstraint` implementations filter matches before an action is selected.

- `{id:int}` rejects `orders/abc` so the route does not match and another route may handle the request.
- `{area:exists}` ensures the area segment matches a registered area name in the application.
- Constraints participate in URL generation as well as matching — invalid values may prevent link creation.
- Failed constraint match yields "no route matched" (404), not an action invocation with invalid data.

---

### Q6. Why does route registration order matter in `MapControllerRoute`? {#chapter-09-routing-attribute-routing-q6}

Why does route registration order matter in `MapControllerRoute`?

**Answer:** Endpoint routing evaluates registered routes in order; the first route template that matches the request wins. More specific routes must be registered before general catch-all patterns like the default `{controller}/{action}/{id?}`.

- A slug route `products/{id}` registered before the default route captures `/products/sale` as `id = "sale"` instead of reaching a `Sale` action via conventional segments.
- Named routes do not affect matching priority — registration sequence in `Program.cs` does.
- Attribute routes from `MapControllers()` are merged into the same endpoint collection with their own order metadata.
- Integration tests should cover ambiguous URLs when adding new conventional routes.

---

### Q7. How does the `{area:exists}` constraint work? {#chapter-09-routing-attribute-routing-q7}

How does the `{area:exists}` constraint work?

**Answer:** The `exists` constraint on an `area` route parameter verifies that the captured area name corresponds to a known MVC area registered in the application (controllers decorated with `[Area("Name")]`). If the area does not exist, the route fails to match.

- Used in the standard areas template: `{area:exists}/{controller=Home}/{action=Index}/{id?}`.
- Prevents arbitrary first segments from being interpreted as areas when they are not defined.
- Area discovery relies on `[Area]` attributes — folder placement alone is insufficient.
- Link generation with `asp-area` must use a registered area name or URL generation fails or omits the segment incorrectly.

---

### Q8. What is the difference between `[Route]` on a controller vs on an action? {#chapter-09-routing-attribute-routing-q8}

What is the difference between `[Route]` on a controller vs on an action?

**Answer:** Controller-level `[Route("prefix")]` establishes a shared URL prefix for all actions in that controller. Action-level `[Route("segment")]` or verb attributes append or override segments relative to the controller prefix.

- `[Route("[controller]")]` on the controller plus `[HttpGet("{id}")]` on an action yields `/Products/{id}` for `ProductsController`.
- Action-only `[HttpGet("/absolute")]` with a leading slash ignores controller prefix and maps from site root.
- HTTP method attributes (`[HttpGet]`, `[HttpPost]`) are a form of attribute routing with method constraints.
- Action templates combine with controller templates unless the action template is root-absolute (starts with `/`).

---

### Q9. What does a leading slash in `[HttpGet("/export/{year}")]` mean? {#chapter-09-routing-attribute-routing-q9}

What does a leading slash in `[HttpGet("/export/{year}")]` mean?

**Answer:** A route template starting with `/` is absolute from the application root and does not combine with any controller-level route prefix. `/export/{year}` maps to that path regardless of a `[Route("api")]` on the controller.

- Without the leading slash, `export/{year}` would append to the controller prefix (e.g., `/api/export/2024`).
- Absolute routes are useful for fixed marketing or report URLs independent of controller naming conventions.
- Root-absolute templates ignore `[Route]` prefixes on the controller class.
- Link generation must use matching absolute templates or appropriate `asp-route-*` values.

---

### Q10. What is a catch-all route parameter (`{*slug}`)? {#chapter-09-routing-attribute-routing-q10}

What is a catch-all route parameter (`{*slug}`)?

**Answer:** A catch-all parameter uses the `*` prefix (e.g., `{*slug}`) and greedily captures the remainder of the URL path including slashes, typically as a single string segment for file paths or CMS content.

- `/docs/{*slug}` matches `/docs/a/b/c` with `slug = "a/b/c"`.
- Catch-all parameters must usually be the last segment in the template.
- Useful for serving nested virtual paths from one action (documentation browsers, wildcard CMS pages).
- Route constraints on catch-all parameters are limited compared to single-segment parameters.

---

### Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection? {#chapter-09-routing-attribute-routing-q11}

How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?

**Answer:** HTTP method attributes constrain which requests can select an action during endpoint matching. `[HttpGet]` matches GET requests; `[HttpPost]` matches POST; unsupported verbs on a matched route may return 405 Method Not Allowed.

- An action without a verb attribute may accept any method unless restricted by convention or `[ActionName]`.
- Multiple actions on the same route template must differ by HTTP method to disambiguate.
- `[AcceptVerbs("GET", "HEAD")]` allows listing multiple verbs explicitly.
- Method constraints apply to attribute-routed endpoints and to actions reached through conventional routing when verb attributes are present.

---

### Q12. What happens when two actions match the same route? {#chapter-09-routing-attribute-routing-q12}

What happens when two actions match the same route?

**Answer:** ASP.NET Core throws `AmbiguousActionException` at runtime (or fails startup discovery in some cases) when multiple actions are equally eligible for the same HTTP method and route template. The framework requires unambiguous endpoint selection.

- Differentiate with distinct route templates, HTTP methods, or route constraints.
- `[HttpPost]` vs `[HttpGet]` on the same path template is valid; two `[HttpGet]` actions on the same template is not.
- Action naming alone does not disambiguate if both match the incoming URL equally.
- Refactor colliding actions or add literal segments and constraints to restore single match.

---

### Q13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing? {#chapter-09-routing-attribute-routing-q13}

How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?

**Answer:** Anchor and Form Tag Helpers call into URL generation (`IUrlHelper`) using registered endpoint data, ambient route values, and supplied `asp-route-*` attributes to produce URLs that match the route table.

- Generated links reflect attribute routes and conventional routes without hardcoding paths.
- Ambient values from the current request fill omitted `asp-controller` / `asp-action` parameters.
- Incorrect or missing route values produce URLs that 404 when clicked because no endpoint matches.
- `asp-protocol` and `asp-host` override scheme and host for absolute URL generation.

---

### Q14. Why must `area` be specified when generating links to area controllers from outside the area? {#chapter-09-routing-attribute-routing-q14}

Why must `area` be specified when generating links to area controllers from outside the area?

**Answer:** URL generation uses ambient route values from the current request. A view outside an area has no ambient `area` value, so `asp-controller="Dashboard"` targets a root controller, not `Areas/Admin/Controllers/DashboardController`.

- Explicit `asp-area="Admin"` sets the area route value for correct path generation.
- From within an area, ambient `area` flows to links unless overridden with `asp-area=""` for root controllers.
- Missing `asp-area` is a common cause of 404 links from main site navigation to admin modules.
- Area route templates require the area segment in the generated URL.

---

### Q15. What is `LowercaseUrls` and how does it affect link generation? {#chapter-09-routing-attribute-routing-q15}

What is `LowercaseUrls` and how does it affect link generation?

**Answer:** `LowercaseUrls` is an option on `RouteOptions` (configured via `services.Configure<RouteOptions>`) that normalizes generated URLs to lowercase for controller, action, and route parameter names in link output.

- `/Products/Details` becomes `/products/details` in generated links when enabled.
- Incoming request URL matching remains case-insensitive by default unless `LowercaseUrls` is paired with routing options that enforce case.
- Improves URL consistency for SEO and Linux-hosted deployments where path casing can matter behind proxies.
- Tag Helper and `Url.Action` output respect this setting automatically.

---

### Q16. What is the difference between "no route matched" and "405 Method Not Allowed"? {#chapter-09-routing-attribute-routing-q16}

What is the difference between "no route matched" and "405 Method Not Allowed"?

**Answer:** No route matched (404) means no endpoint template fit the URL path. 405 Method Not Allowed means a route matched the path but no action accepts the request's HTTP method (e.g., POST to a GET-only action).

- Wrong URL shape or missing area segment → typically 404 Not Found.
- Correct URL with wrong verb (GET on `[HttpPost]` only action) → 405 with `Allow` header listing permitted methods when configured.
- Distinguishing them guides debugging: 404 fixes routing templates; 405 fixes HTTP method attributes or form method.
- Conventional routes without verb attributes may accept unintended methods if not guarded with `[HttpGet]` etc.

---

### Q17. How do optional route parameters (`{id?}`) and defaults interact? {#chapter-09-routing-attribute-routing-q17}

How do optional route parameters (`{id?}`) and defaults interact?

**Answer:** The `?` marks a route parameter as optional in the template. Inline defaults in the pattern (`{action=Index}`) or `defaults:` anonymous object in `MapControllerRoute` supply values when segments are omitted.

- `/catalog` with pattern `catalog/{action=List}/{id?}` yields `action = List`, `id = null`.
- Defaults in the route registration are not the same as C# optional parameters — they live in the route table.
- Optional parameters must appear after required segments in the template.
- Link generation omits optional segments when values equal defaults, producing shorter URLs when configured.

---

### Q18. What is the areas route pattern and how does it differ from the default route? {#chapter-09-routing-attribute-routing-q18}

What is the areas route pattern and how does it differ from the default route?

**Answer:** The standard areas pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, registered with `MapControllerRoute` and requiring `[Area("AreaName")]` on area controllers. It inserts an area segment before controller and action, routing to `Areas/{AreaName}/Controllers` and `Areas/{AreaName}/Views`.

- Default route has no area segment: `{controller=Home}/{action=Index}/{id?}` for root controllers in `/Controllers`.
- Areas enable parallel controller names (e.g., `Admin/HomeController` vs root `HomeController`).
- Register the areas route before or with careful ordering relative to the default route so area URLs resolve correctly.
- Views and `_ViewStart` under `Areas/{AreaName}/Views` follow separate layout resolution from root `Views`.

---

> **Target:** ASP.NET Core 8 MVC

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

#### Q1. (R) Review this `ProductsController` and `Program.cs`. `GET /products/sale` shows a product named "sale" instead of the sale landing page.

```csharp
// Program.cs
app.MapControllerRoute(
    name: "productSlug",
    pattern: "products/{id}",
    defaults: new { controller = "Products", action = "Details" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

public class ProductsController : Controller
{
    public IActionResult Index() => View(_repo.All());

    public IActionResult Details(string id) => View(_repo.BySlug(id));

    public IActionResult Sale() => View(_repo.SaleItems());
}
```

Marketing expects `/products/sale` to hit `Sale()`; QA reports it returns a product whose slug is `"sale"`.

---

**Answer:**

```csharp
app.MapControllerRoute(
    name: "productSlug",
    pattern: "products/{id}",
    defaults: new { controller = "Products", action = "Details" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

public class ProductsController : Controller
{
    public IActionResult Index() => View(_repo.All());
    public IActionResult Details(string id) => View(_repo.BySlug(id));
    public IActionResult Sale() => View(_repo.SaleItems());
}
```

**Answer:** The dedicated `products/{id}` route is registered **before** the default route and maps `/products/sale` to `Details(id: "sale")` — the literal segment `sale` is a slug, not the `Sale` action. Conventional `/products/sale` would only reach `Sale()` via the default `{controller}/{action}` template, but the slug route wins first.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route template ambiguity | Slug/`{id}` pattern vs literal action name `Sale` | Same path segment means product slug or dedicated action |
| Conventional routing | `{controller}/{action}/{id?}` encourages `/products/details/sale` for slugs | Marketing URL `/products/sale` collides with slug routing |
| API contract | No disambiguation (literal prefix vs parameter) | Wrong view/data — silent functional bug |

**Fix (priority order):**

1. Separate namespaces: `[HttpGet("sale")]` or `[Route("products/sale")]` on `Sale`, and `[HttpGet("products/{slug}")]` on `Details` — literal segments beat bare parameters in attribute routing.
2. For conventional-only apps: rename slug action route via dedicated pattern in `Program.cs`: `products/{slug}` → `Details`, register **before** default; keep `products/sale` → `Sale` via explicit `MapControllerRoute` with literal `sale`.
3. Add routing integration tests for `/products/sale`, `/products/details/sale`, and `/products/sale-items`.

**Production takeaway:** MVC slug URLs and marketing literals at the same path level need explicit route templates — see [05. ASP.NET Core/07. Routing & Endpoints](../../05.%20ASP.NET%20Core/07.%20Routing%20&%20Endpoints/KARAT_INTERVIEW_ANSWERS.md) Q4 for attribute-route specificity; the same rule applies to conventional + slug patterns.

---

---

#### Q2. (M) A team registers two conventional routes for an MVC storefront. Explain how optional `{id?}` and default values interact when a user visits `/catalog`, `/catalog/featured`, and `/catalog/list/42`.

```csharp
app.MapControllerRoute(
    name: "catalog",
    pattern: "catalog/{action=List}/{id?}",
    defaults: new { controller = "Catalog" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Which controller action handles each URL, and what happens if `List` is renamed but the route pattern is not updated?

---

**Answer:**

_Answer not found._

---

#### Q3. (R) Review this route registration and action. `GET /orders/not-a-number` returns 500 with `FormatException` in logs instead of a client-friendly not-found.

```csharp
app.MapControllerRoute(
    name: "order",
    pattern: "orders/{id:int}",
    defaults: new { controller = "Orders", action = "Details" });

public class OrdersController : Controller
{
    public IActionResult Details(int id) => View(_orders.Get(id));
}
```

No attribute routes exist on `OrdersController`.

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review `Program.cs` route order. After deploy, every unknown URL renders the home page with HTTP 200 instead of 404, and `/admin/users` never reaches `AdminController`.

```csharp
app.MapControllerRoute(
    name: "fallback",
    pattern: "{*path}",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this shared `_Layout.cshtml` link and area setup. Clicking **Manage Users** from the Shop area returns 404, but the same link works from the root site.

```html
<!-- _Layout.cshtml (shared by root and Shop area views) -->
<a asp-controller="Users" asp-action="Index">Manage Users</a>
```

```csharp
// Program.cs
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Areas/Admin/Controllers/UsersController.cs — no [Area] attribute missing
[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Index() => View();
}
```

Current request when bug occurs: `/shop/products` (Shop area).

---

**Answer:**

_Answer not found._

---

#### Q6. (M) Compare attribute routing applied at the **controller** vs **action** level. Given the snippets below, what are the final URLs for `Monthly` and `Export`, and which tokens does each action inherit?

```csharp
[Route("reports/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("monthly")]
    public IActionResult Monthly() => View();

    [HttpGet("/export/{year:int}")]
    public IActionResult Export(int year) => File(_svc.Export(year), "text/csv");
}
```

When would you put `[Route]` on the controller vs only on actions in a mixed MVC + API surface?

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review this documentation site controller. `GET /docs/getting-started/install` returns 404, but `GET /docs/getting-started` works. Static files middleware is registered before routing.

```csharp
[Route("docs")]
public class DocsController : Controller
{
    [HttpGet("{*slug}")]
    public IActionResult Page(string slug)
    {
        var content = _store.Find(slug);
        return content is null ? NotFound() : View("Doc", content);
    }
}
```

---

**Answer:**

_Answer not found._

---

#### Q8. (M) For the same conventional route `{controller=Home}/{action=Index}/{id?}`, contrast the HTTP status and routing outcome for:

1. `GET /home/index` when `Index` exists and allows GET  
2. `POST /home/index` when only GET is implemented (no `[HttpPost]` on `Index`)  
3. `GET /homme/index` (typo in controller name)

Explain how endpoint routing differs from "no route matched" vs "route matched, method not allowed."

---

**Answer:**

_Answer not found._

---

#### Q9. (P) Production enables lowercase URLs for SEO. After enabling the option below, organic links work but several redirects and `Url.Action` calls still emit PascalCase paths that 404 on Linux containers.

```csharp
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Somewhere in AccountController:
return Redirect("/Account/Login?returnUrl=/Products/Details/5");
```

What must change in route registration, link generation, and hard-coded redirects for consistent behavior cross-platform?

---

**Answer:**

_Answer not found._

---

#### Q10. (D) A legacy ASP.NET Core 2.2 MVC app is upgraded to .NET 8. The old `Startup.cs` still calls `UseMvcWithDefaultRoute()` while newer code uses `MapControllerRoute` inside `UseEndpoints`. Reviewers ask whether to keep both for compatibility.

```csharp
// Old (commented in places, still active in one branch)
app.UseMvcWithDefaultRoute();

// New
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

What breaks if both run, what is the modern replacement, and how does endpoint routing change action selection compared to legacy IRouter middleware?

---

**Answer:**

_Answer not found._

---

#### Q11. (R) Review attribute + conventional mixing on one controller. Integration tests expect `GET /api/reports/summary` but receive 404; `GET /Reports/Summary` works. Tag Helper links in Razor views point at `/Reports/Summary`.

```csharp
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

[Route("api/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("summary")]
    public IActionResult Summary() => Json(_svc.Summary());

    public IActionResult Index() => View();
}
```

```html
<a asp-controller="Reports" asp-action="Summary">Summary API</a>
```

---

**Answer:**

_Answer not found._

---
