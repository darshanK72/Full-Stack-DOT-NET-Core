# Routing & Attribute Routing — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is conventional routing in ASP.NET Core MVC?](#q1-what-is-conventional-routing-in-aspnet-core-mvc)
2. [Q2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?](#q2-what-is-the-default-route-pattern-controllerhomeactionindexid)
3. [Q3. What is attribute routing in MVC controllers?](#q3-what-is-attribute-routing-in-mvc-controllers)
4. [Q4. What is the difference between conventional routing and attribute routing?](#q4-what-is-the-difference-between-conventional-routing-and-attribute-routing)
5. [Q5. What are route constraints (e.g., `:int`, `:exists`)?](#q5-what-are-route-constraints-eg-int-exists)
6. [Q6. Why does route registration order matter in `MapControllerRoute`?](#q6-why-does-route-registration-order-matter-in-mapcontrollerroute)
7. [Q7. How does the `{area:exists}` constraint work?](#q7-how-does-the-areaexists-constraint-work)
8. [Q8. What is the difference between `[Route]` on a controller vs on an action?](#q8-what-is-the-difference-between-route-on-a-controller-vs-on-an-action)
9. [Q9. What does a leading slash in `[HttpGet("/export/{year}")]` mean?](#q9-what-does-a-leading-slash-in-httpgetexportyear-mean)
10. [Q10. What is a catch-all route parameter (`{*slug}`)?](#q10-what-is-a-catch-all-route-parameter-slug)
11. [Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?](#q11-how-do-http-verbs-httpget-httppost-affect-action-selection)
12. [Q12. What happens when two actions match the same route?](#q12-what-happens-when-two-actions-match-the-same-route)
13. [Q13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?](#q13-how-does-tag-helper-link-generation-asp-controller-asp-action-relate-to-routing)
14. [Q14. Why must `area` be specified when generating links to area controllers from outside the area?](#q14-why-must-area-be-specified-when-generating-links-to-area-controllers-from-outside-the-area)
15. [Q15. What is `LowercaseUrls` and how does it affect link generation?](#q15-what-is-lowercaseurls-and-how-does-it-affect-link-generation)
16. [Q16. What is the difference between "no route matched" and "405 Method Not Allowed"?](#q16-what-is-the-difference-between-no-route-matched-and-405-method-not-allowed)
17. [Q17. How do optional route parameters (`{id?}`) and defaults interact?](#q17-how-do-optional-route-parameters-id-and-defaults-interact)
18. [Q18. What is the areas route pattern and how does it differ from the default route?](#q18-what-is-the-areas-route-pattern-and-how-does-it-differ-from-the-default-route)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is conventional routing in ASP.NET Core MVC?

**Concepts**
- Centralized route template registration in `Program.cs`
- `MapControllerRoute` and the endpoint routing middleware
- Segment-to-controller/action name mapping
- Route defaults for omitted URL segments
- Registration order and template specificity

**Answer**

Conventional routing maps URLs to controllers and actions using route templates registered centrally in `Program.cs` via `MapControllerRoute`. The endpoint routing middleware extracts segment values from the URL at runtime and invokes the matching action method — so `{controller=Home}/{action=Index}/{id?}` maps the first segment to a controller name (without the `Controller` suffix) and the second to an action name. Because defaults are baked into the template, a bare `/` resolves to `HomeController.Index()` with no additional configuration. Multiple named routes can coexist, which means registration order and specificity determine which route wins — more specific templates must come before general catch-all patterns.

---

## Q2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?

**Concepts**
- Segment-to-controller/action/id mapping convention
- Inline route defaults for omitted URL segments
- Case-insensitive controller and action name matching
- Optional `{id?}` parameter token

**Answer**

The template maps the first URL segment to a controller name (minus the `Controller` suffix), the second to an action method, and an optional third to a parameter named `id`. The inline defaults — `controller=Home` and `action=Index` — mean a bare `/` resolves to `HomeController.Index()` with `id = null`. Matching is case-insensitive by default in ASP.NET Core 8, so `/products/details/42` and `/Products/Details/42` both reach `ProductsController.Details(42)`. The `?` on `{id?}` lets the same route template handle actions that accept or omit an id parameter.

---

## Q3. What is attribute routing in MVC controllers?

**Concepts**
- `[Route]`, `[HttpGet]`, `[HttpPost]` decorating controllers and actions directly
- `MapControllers()` for attribute route discovery at startup
- Controller-level prefix combined with action-level template
- Coexistence of attribute and conventional routes

**Answer**

Attribute routing colocates URL templates directly on the controller or action that handles them, rather than centralizing them in `Program.cs`. Calling `MapControllers()` at startup discovers all controllers and their route attributes and registers them into the endpoint route table. A controller-level `[Route("api/[controller]")]` prefixes every action in that class, while an action-level `[HttpGet("{id:int}")]` appends HTTP method constraints and parameter tokens. Attribute routes can coexist with conventional routes in the same application and are preferred for REST-style APIs and fixed URLs where you want the route shape to be explicit in code rather than inferred from class names.

---

## Q4. What is the difference between conventional routing and attribute routing?

**Concepts**
- Conventional routing — centralized patterns inferred from controller and action names
- Attribute routing — explicit colocated templates on controller classes
- Fine-grained URL control vs consistent URL shape trade-off
- Link generation with Tag Helpers across both styles

**Answer**

The key distinction is where the URL pattern lives. Conventional routing keeps everything in `Program.cs` and infers controller and action from URL segments, which suits sites with a consistent `{controller}/{action}` shape. Attribute routing puts the template on the class itself, making each action's public URL explicit — this is better for versioning prefixes, literal segments, or scenarios where the URL cannot follow the controller-naming convention. Tag Helpers work with both; attribute routes need the correct template tokens in `asp-route-*` values. When mixing both in the same application, I need to be careful because attribute routes and conventional routes compete in the same endpoint route table.

---

## Q5. What are route constraints (e.g., `:int`, `:exists`)?

**Concepts**
- Built-in constraints — `:int`, `:guid`, `:alpha`, `:minlength(n)`
- `IRouteConstraint` for custom validation logic
- Constraint failure yielding "no route matched" 404 rather than a bound parameter
- Constraint participation in URL generation

**Answer**

Route constraints restrict which values a route parameter will accept before an action is selected. `:int` rejects non-numeric segments so `/orders/abc` does not match `orders/{id:int}`, allowing another route to handle the request instead. The `{area:exists}` constraint verifies the captured segment matches a registered area name, preventing arbitrary first segments from being misinterpreted as areas. Constraints also participate in URL generation — if a supplied value violates the constraint, link generation fails or omits the segment. A failed constraint always yields a "no route matched" 404, not an action invocation with invalid data passed to the parameter.

---

## Q6. Why does route registration order matter in `MapControllerRoute`?

**Concepts**
- First-match evaluation order in endpoint routing
- Specific routes must precede catch-all patterns
- Registration sequence in `Program.cs` over named route priority
- Attribute route order metadata merged into the endpoint collection

**Answer**

Endpoint routing evaluates registered routes in the order they appear in `Program.cs` and takes the first template that matches the request. This means more specific routes must be registered before general ones like the default `{controller}/{action}/{id?}`. A slug route `products/{id}` registered before the default captures `/products/sale` as `id = "sale"` rather than routing to the `Sale` action via the conventional template. Named routes do not affect matching priority — only registration sequence matters. Attribute routes from `MapControllers()` are merged into the same endpoint collection with their own order metadata, so integration tests should cover ambiguous URLs whenever new conventional routes are added.

---

## Q7. How does the `{area:exists}` constraint work?

**Concepts**
- `exists` constraint verifying captured segment against registered area names
- `[Area("Name")]` attribute as the registration mechanism — not folder placement
- Standard areas route template using the constraint
- Link generation requiring a registered area name

**Answer**

The `exists` constraint on an `area` parameter checks that the captured first URL segment corresponds to a known area in the application — specifically, a controller decorated with `[Area("Name")]`. If no such area is registered, the route fails to match and the request falls through to the next route. This prevents arbitrary first segments from being mistakenly interpreted as area names. Physical folder placement under `Areas/` is not enough — the `[Area]` attribute on the controller class is what registers the area name. For link generation, `asp-area` must use a registered name or the URL will be generated incorrectly.

---

## Q8. What is the difference between `[Route]` on a controller vs on an action?

**Concepts**
- Controller-level `[Route]` establishing a shared URL prefix for all actions
- Action-level template appending to or overriding the controller prefix
- Root-absolute action template (leading `/`) ignoring the controller prefix
- HTTP method attributes as attribute routing with an implicit verb constraint

**Answer**

A controller-level `[Route("prefix")]` establishes a shared URL prefix that every action in the class inherits. An action-level template appends a segment relative to that prefix — so `[Route("[controller]")]` on the controller plus `[HttpGet("{id}")]` on an action yields `/Products/{id}` for `ProductsController`. The important exception is a leading slash: `[HttpGet("/absolute")]` is root-absolute and ignores the controller prefix entirely, mapping from the site root regardless of what the controller declares. HTTP method attributes like `[HttpGet]` and `[HttpPost]` are themselves a form of attribute routing that also impose a verb constraint on the template.

---

## Q9. What does a leading slash in `[HttpGet("/export/{year}")]` mean?

**Concepts**
- Root-absolute route template ignoring any controller-level prefix
- Contrast with relative template that appends to the controller prefix
- Fixed marketing or report URLs independent of controller naming

**Answer**

A route template starting with `/` is absolute from the application root and does not combine with any controller-level `[Route]` prefix. So `/export/{year}` maps directly to that path regardless of a `[Route("api")]` on the controller — the generated URL is `/export/2024` rather than `/api/export/2024`. Without the leading slash, the relative template `export/{year}` would append to the controller prefix. Root-absolute templates are useful when you need a fixed URL independent of the controller naming convention, such as a legacy report path or a marketing-friendly URL that must not change when the controller is renamed or reorganized.

---

## Q10. What is a catch-all route parameter (`{*slug}`)?

**Concepts**
- Greedy capture of remaining URL path including slashes as a single string
- Must be the last segment in the route template
- Use cases — nested virtual paths, documentation browsers, CMS content
- Limited constraint support compared to single-segment parameters

**Answer**

A catch-all parameter prefixed with `*` greedily captures the entire remainder of the URL path, including slash characters, as a single string value. `/docs/{*slug}` matches `/docs/a/b/c` with `slug = "a/b/c"`, which makes it ideal for documentation browsers or CMS pages where paths can be arbitrarily deep. The catch-all must be the last segment in the template because it consumes everything after the preceding literal. Route constraints on catch-all parameters are more limited compared to single-segment parameters, so complex validation needs to happen in the action method rather than in the route template.

---

## Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?

**Concepts**
- HTTP method constraints narrowing action selection
- 405 Method Not Allowed when path matches but verb does not
- Multiple actions on the same route template differentiated by verb
- `[AcceptVerbs]` for listing multiple allowed methods on one action

**Answer**

HTTP method attributes constrain which requests can select an action during endpoint matching. `[HttpGet]` only accepts GET; `[HttpPost]` only accepts POST; when a route path matches but no action accepts the incoming verb, the framework returns 405 Method Not Allowed rather than 404. This means two actions can legitimately share the same route template as long as they are differentiated by verb — the standard REST resource pattern. An action without any verb attribute accepts any HTTP method, which is why annotating actions explicitly is good practice. `[AcceptVerbs("GET", "HEAD")]` lists multiple methods when a single attribute per action is not sufficient.

---

## Q12. What happens when two actions match the same route?

**Concepts**
- `AmbiguousActionException` thrown when multiple actions are equally eligible
- Distinct HTTP methods as the valid disambiguation mechanism
- Route template or constraint differences required for same-verb actions
- Action naming alone insufficient for disambiguation

**Answer**

When multiple actions are equally eligible for the same HTTP method and route template, ASP.NET Core throws `AmbiguousActionException` because the framework requires unambiguous endpoint selection. Two `[HttpGet]` actions on the same path is invalid, but `[HttpPost]` and `[HttpGet]` on the same path is valid because the verb differentiates them. Action naming alone does not disambiguate — if both actions match the incoming URL and method equally, the framework cannot choose. The fix is to add distinct route templates, HTTP method constraints, or route constraints that make one action more specific than the other.

---

## Q13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?

**Concepts**
- `IUrlHelper` consuming registered endpoint data for URL generation
- Ambient route values from the current request filling omitted parameters
- `asp-route-*` attributes supplying additional route values
- Missing or incorrect route values producing 404 links

**Answer**

Anchor and Form Tag Helpers call into `IUrlHelper` with the registered endpoint data, ambient route values from the current request, and any `asp-route-*` attributes you supply. Generated URLs automatically reflect the route table, so if a controller route changes, the Tag Helper picks it up without hardcoding paths. Ambient values from the current request fill in omitted `asp-controller` and `asp-action` parameters, which is convenient but can be surprising when the ambient context carries area or route values you did not intend. Incorrect or missing route values silently produce URLs that 404 when clicked because no endpoint in the route table matches.

---

## Q14. Why must `area` be specified when generating links to area controllers from outside the area?

**Concepts**
- Absent `area` ambient value in non-area requests
- Explicit `asp-area` required for root-to-area link generation
- Ambient area flowing within the same area context
- `asp-area=""` to escape an area and target root controllers

**Answer**

URL generation uses ambient route values from the executing request. A view outside any area has no ambient `area` value, so `asp-controller="Dashboard"` targets a root controller rather than `Areas/Admin/Controllers/DashboardController`. Explicit `asp-area="Admin"` is required to set the area route value for correct path generation. The reverse is also true: from within an area, the ambient area flows to links unless you override it with `asp-area=""` to clear it and target a root controller. Missing `asp-area` is one of the most common causes of 404 links from main site navigation to admin modules.

---

## Q15. What is `LowercaseUrls` and how does it affect link generation?

**Concepts**
- `RouteOptions.LowercaseUrls` normalizing generated URL output
- Applied to controller, action, and route parameter names in link output
- SEO consistency and Linux deployment path-casing sensitivity
- Tag Helper and `Url.Action` honoring the setting automatically

**Answer**

`LowercaseUrls` is a `RouteOptions` setting that normalizes generated URLs to lowercase, so `/Products/Details` becomes `/products/details` in all Tag Helper and `Url.Action` output. Incoming request matching remains case-insensitive by default, but on Linux-hosted deployments proxies or CDNs can be case-sensitive, meaning uppercase-generated links produce 404s behind them. The setting applies automatically to `asp-controller`, `asp-action`, and route parameter names without manual work. I would pair it with `LowercaseQueryStrings = true` for full consistency on SEO-important routes.

---

## Q16. What is the difference between "no route matched" and "405 Method Not Allowed"?

**Concepts**
- 404 "no route matched" — URL path does not fit any endpoint template
- 405 Method Not Allowed — path matched but HTTP verb rejected
- `Allow` header listing permitted methods on a 405 response
- Debugging distinction — 404 fixes templates, 405 fixes verb attributes

**Answer**

These outcomes differ in what matched. A 404 "no route matched" means no endpoint template fit the URL path at all — the URL shape is wrong, a required segment is missing, or a constraint failed. A 405 Method Not Allowed means the path matched a route but no action accepts the request's HTTP method, such as a GET request to an action decorated only with `[HttpPost]`. A 405 response should include an `Allow` header listing permitted methods. This distinction matters for debugging: a 404 means I should inspect route templates, while a 405 means I need to fix HTTP method attributes or the form method used by the client.

---

## Q17. How do optional route parameters (`{id?}`) and defaults interact?

**Concepts**
- `?` marking a segment as optional in the route template
- Inline defaults `{action=Index}` vs `defaults:` anonymous object in `MapControllerRoute`
- Route table defaults distinct from C# optional method parameters
- Link generation omitting optional segments when values equal defaults

**Answer**

The `?` suffix marks a parameter optional so the template matches URLs both with and without that segment. Inline defaults like `{action=List}` or a `defaults:` anonymous object in `MapControllerRoute` supply values when segments are omitted — so `/catalog` with pattern `catalog/{action=List}/{id?}` yields `action = List` and `id = null`. These route table defaults are distinct from C# optional parameters in the method signature, and confusing them produces subtle bugs. Link generation omits optional segments when supplied values equal the configured defaults, producing shorter canonical URLs automatically.

---

## Q18. What is the areas route pattern and how does it differ from the default route?

**Concepts**
- Standard areas pattern `{area:exists}/{controller=Home}/{action=Index}/{id?}`
- `[Area("AreaName")]` requirement on area controllers
- Area-specific view location under `Areas/{AreaName}/Views`
- Registration order — areas route before default route

**Answer**

The standard areas pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, registered with `MapControllerRoute` and requiring `[Area("AreaName")]` on area controllers. It inserts an area segment before controller and action, routing to `Areas/{AreaName}/Controllers` and resolving views from `Areas/{AreaName}/Views` rather than the root folders. The default route `{controller=Home}/{action=Index}/{id?}` has no area segment and serves root controllers. Because both patterns share a similar generic structure, the area route must be registered before the default so the `{area:exists}` constraint can consume the leading segment correctly before the default route claims it as the controller name.

---

> **Target:** ASP.NET Core 8 MVC

---

## Gotchas — Routing & Attribute Routing (Interview Traps)

---

#### Gotcha 1. Conventional route order — more specific routes shadowed by catch-all

**Concepts**
- `MapControllerRoute` registration order — first match wins
- Catch-all default route — `{controller=Home}/{action=Index}/{id?}` matches almost everything
- Specific routes — must be registered before the catch-all default
- Shadowed route — request matches the wrong action silently, returning wrong data or 404

**Answer**

In conventional routing, routes are evaluated in registration order and the first match wins. Registering the default `{controller=Home}/{action=Index}/{id?}` route first means any more specific route registered afterward — such as a custom `/admin/dashboard` route — is never reached because the default already matched. Area routes and special-path routes must come before the default catch-all. The symptom is the action receiving the wrong `id` value or a completely different action being invoked when only the URL is changed. Attribute routing bypasses this ordering entirely since each attribute route is matched against the URL independently, making it the safer choice for complex routing needs.

---

#### Gotcha 2. Duplicate attribute route templates causing `AmbiguousMatchException`

**Concepts**
- Two actions with identical `[HttpGet("items")]` — both match, framework cannot choose
- `AmbiguousMatchException` — thrown at route selection, not at compile time
- Route disambiguation — use different templates, constraints, or HTTP verb restrictions
- Optional parameter — does not create two distinct templates from one attribute

**Answer**

Two actions with `[HttpGet("catalog/items")]` have identical route templates — the router finds two candidates and throws `AmbiguousMatchException`. The exception is a runtime failure that only manifests when the route is requested, not at startup or compile time. An optional parameter like `[HttpGet("catalog/items/{category?}")]` creates one template that matches both `/catalog/items` and `/catalog/items/toys` — it does not create two separate routes. To serve both a default list and a filtered list, either merge into one action with an optional `category` parameter, or use distinct templates such as `[HttpGet("catalog/items")]` and `[HttpGet("catalog/items/{category}")]`. Duplicate templates can also arise from multiple controllers having the same route prefix and the same action name.

---

#### Gotcha 3. Route constraints not matching causing 404 instead of 400

**Concepts**
- `{id:int}` constraint — fails the route match entirely; returns 404 for non-integer segments
- No constraint — route matches; model binder fails; `ModelState` has error, 400 is possible
- 404 vs 400 semantic — route mismatch is "no route found"; binding failure is "bad request"
- Constraint choice — use when only that type should reach the action; omit for validation messages

**Answer**

`[HttpGet("{id:int}")]` requires the route segment to parse as an integer at the routing layer. A request to `/Products/abc` does not match the route and returns a 404 — the client never reaches the action or model binding. Without the `:int` constraint, the route matches, the binder tries to convert "abc" to `int`, fails, adds a conversion error to `ModelState`, and the action receives `id = 0` with `ModelState.IsValid == false`. The behavioral difference matters: 404 signals a wrong URL (client should fix the request path) while 400 signals a bad request to a valid route. Choose constraints deliberately based on whether a non-integer should be treated as "no such route" or as "bad input to this route."

---

#### Gotcha 4. Route template on the controller conflicting with the route template on the action

**Concepts**
- `[Route("products")]` on controller — all action routes are relative to this prefix
- `[HttpGet("/absolute")]` — leading slash makes the route absolute, ignores controller prefix
- `[HttpGet("list")]` — relative, combined with controller prefix to produce `products/list`
- Double prefix — controller `[Route("api/products")]` + action `[HttpGet("api/products/list")]` duplicates the path

**Answer**

`[Route("products")]` on the controller sets a prefix; action routes without a leading `/` are appended to it. `[HttpGet("list")]` produces `products/list`. An action with `[HttpGet("/list")]` (leading slash) ignores the controller prefix and produces just `/list` — sometimes intended, often a mistake. Adding `[Route("api/products")]` on both the controller and the action produces `api/products/api/products/list`, doubling the prefix. The rule is: set the segment prefix once — either on the controller or on the action — and keep the other relative. Templates on actions should never repeat the controller prefix.

---

#### Gotcha 5. Area routes not registered before the default route

**Concepts**
- Area routes — must be registered with `MapAreaControllerRoute` before the default `MapControllerRoute`
- Default route matching area segment — `{controller=Home}` can match "Admin" as a controller name
- `{area:exists}` constraint — only matches area segment when a registered area with that name exists
- 404 for all area URLs — symptom of default route registered first

**Answer**

Area routing requires `app.MapAreaControllerRoute("admin_default", "Admin", "Admin/{controller=Dashboard}/{action=Index}/{id?}")` to be registered before `app.MapControllerRoute("default", ...)`. When the default route comes first, a request to `/Admin/Users/Index` matches `{controller=Admin}/{action=Users}/{id=Index}` — treating `Admin` as a controller name — and returns 404 because there is no root-level `AdminController` with a `Users` action. The `{area:exists}` constraint on the area route ensures it only matches the URL when "Admin" is a registered area name, preventing false matches. Every area route must precede the default catch-all in `Program.cs`.

---

#### Gotcha 6. `IActionConstraint` vs route constraint — different execution times and purposes

**Concepts**
- Route constraint — evaluated during route matching; filters candidate routes
- `IActionConstraint` — evaluated after route matching; filters among matched action candidates
- `[HttpGet]` / `[HttpPost]` — implemented as `IActionConstraint`, not route constraints
- Route constraint misuse — cannot access `HttpContext.User` or request headers

**Answer**

Route constraints like `{id:int}` or `{slug:regex(^[a-z]+$)}` execute during URL matching before any action is selected and have access only to the route template and URL segments — not to the HTTP request headers, body, or `HttpContext.User`. `IActionConstraint` implementations like HTTP verb attributes (`[HttpGet]`) execute after route matching, when the framework already has a set of candidate actions, and can access the full `HttpContext`. Trying to authorize based on a query string or user role inside a route constraint fails because `HttpContext` is not available at that stage. Authorization belongs in middleware, `IActionConstraint`, or action filters — not in route constraints.

---

#### Gotcha 7. Named routes used in `RedirectToRoute` but renamed after initial setup

**Concepts**
- Named routes — `name: "product_detail"` in `MapControllerRoute` or `[Route("...", Name="...")]`
- `RedirectToRoute("product_detail", ...)` — runtime exception if route name does not exist
- Route name not a compile-time constant — refactoring controller or action does not update string
- `RedirectToAction` — safer alternative that references action method directly

**Answer**

`RedirectToRoute("product_detail", new { id = 5 })` requires a route named `product_detail` to exist at runtime. If the route was renamed from `product_detail` to `products_show` in `Program.cs` or the attribute was changed, the `RedirectToRoute` call throws `InvalidOperationException` at runtime with no compile-time warning. `RedirectToAction(nameof(ProductsController.Details), "Products", new { id = 5 })` uses `nameof` which is checked at compile time — renaming the action method produces a build error, not a runtime crash. Use named routes only when necessary (for link generation across areas) and prefer `RedirectToAction` with `nameof` for redirects within the same application.

---

#### Gotcha 8. Optional route parameters behaving differently from default values

**Concepts**
- `{id?}` — optional segment; action receives null or default when omitted
- `{id=5}` — default value; route always provides 5 when the segment is omitted
- Null vs default — `{id?}` with `int id` receives 0 (not null) due to type default
- `int? id` vs `int id` — `int?` receives null when omitted; `int` receives 0

**Answer**

`[HttpGet("{id?}")]` with `public IActionResult Details(int id)` does not produce a null `id` when the segment is omitted — it produces `0` because `int` defaults to `0`, not null. A check of `if (id == 0)` is needed to detect the absent segment. Using `int? id` makes the absence explicit — `id` is null when the segment is omitted, `id` has a value when present. `{id=5}` is distinct: when the segment is omitted, the route populates `id = 5` as a default, so the action always receives a non-null value. Choose `int?` with `{id?}` when absence must be distinguishable from a legitimate zero value.

---

#### Gotcha 9. Ambiguous match between conventional route and attribute route on the same controller

**Concepts**
- Mixed routing — convention and attribute routes on the same controller
- Conventional route — matches `{controller}/{action}` pattern
- Attribute route — `[Route("custom-path")]` on the controller or action
- `AmbiguousMatchException` — when both routes match the same URL

**Answer**

Mixing conventional and attribute routing on the same controller can produce `AmbiguousMatchException`. When a controller has `[Route("api/orders")]` at the class level and `MapControllerRoute` also matches `api/Orders/Index`, both routes claim the request and the framework throws. MVC recommends that controllers either use fully attribute-routed actions or fully conventional routes, not both. The `[Route]` attribute on a controller disables conventional routing for all actions in that controller — any action without an explicit route attribute on a `[Route]`-decorated controller is unreachable via conventional routes. When converting a controller to attribute routing, add explicit route attributes to all actions before removing the conventional route.

---

#### Gotcha 10. Route data values leaking between requests in URL generation

**Concepts**
- Ambient route values — current request's route values used automatically in link generation
- `asp-action="Edit"` without `asp-id` — uses the current request's `id` from ambient values
- Unexpected URL — link on `/Products/Details/5` generates `/Products/Edit/5` even when editing a different id
- Explicit route values — always specify `asp-route-id` when the target id differs from current

**Answer**

Tag helpers and `Url.Action` use the current request's route values as "ambient" values when generating URLs. On a `/Products/Details/5` page, `<a asp-action="Edit">` generates `/Products/Edit/5` because it picks up `id=5` from the ambient route data. This is convenient when the target id matches — but on a page that iterates over a list, each iteration generates a link with the same ambient `id` rather than the row's id. The fix is to explicitly provide the id: `<a asp-action="Edit" asp-route-id="@item.Id">`. When in doubt about which route values will be ambient, always specify them explicitly rather than relying on ambient value inheritance.

---
## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- Route registration order — first-match wins
- Slug route `products/{id}` consuming the literal segment `sale` as a parameter value
- Attribute route specificity — literal segments ranked over bare parameters
- Conventional routing URL collision between slug pattern and named action

**Answer**

The issue is route registration order: the dedicated `products/{id}` route is registered before the default route, so `/products/sale` matches the slug pattern with `id = "sale"` and calls `Details("sale")` rather than the `Sale()` action. The conventional `{controller}/{action}` template would reach `Sale()` via `/products/sale`, but the slug route intercepts it first since routes are evaluated in registration order.

The cleanest fix is to switch to attribute routing and declare `[HttpGet("sale")]` on `Sale` and `[HttpGet("products/{slug}")]` on `Details` — attribute routing ranks literal segments over bare parameters, so `/products/sale` binds to the literal `Sale` action rather than treating `sale` as a slug value. For conventional-only apps, register a dedicated named route that maps the literal path `products/sale` to the `Sale` action, placing it before the generic slug route. Either way, add integration tests for `/products/sale`, `/products/details/sale`, and related permutations to catch regressions when routes change.

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

**Concepts**
- Inline route defaults supplying action name when segment is omitted
- Optional `{id?}` matching URLs with or without the third segment
- Route pattern hard-coding action name — rename without update causes 404
- Catalog route registered before default route — first-match priority

**Answer**

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

**Concepts**
- `:int` constraint — route should not match a non-integer segment
- Fallthrough to the next route when a constraint fails
- 404 expected from constraint failure, not 500 from `FormatException`
- Second unconstrained route likely catching the request after fallthrough

**Answer**

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

**Concepts**
- Catch-all `{*path}` registered first consuming every incoming request
- Route registration order — fallback must be the last entry
- Admin and default routes unreachable behind the catch-all
- 404 suppression — catch-all masks "no route matched" for all URLs

**Answer**

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

[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Index() => View();
}
```

Current request when bug occurs: `/shop/products` (Shop area).

**Concepts**
- Ambient area route value carrying the current area into link generation
- `asp-area` absent — Tag Helper inheriting ambient `shop` area
- Generated URL `/shop/users` instead of intended `/admin/users`
- Explicit `asp-area="Admin"` required for cross-area navigation

**Answer**

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

**Concepts**
- Controller-level `[Route]` prefix inherited by relative action templates
- Root-absolute action template ignoring the controller prefix
- `[controller]` token substituted with the controller name at startup
- Mixed MVC and API surface routing strategy

**Answer**

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

**Concepts**
- Catch-all `{*slug}` capturing the full remainder of the path
- Static files middleware short-circuiting before routing for certain paths
- Path segments containing dots potentially matching static file patterns
- Missing leading slash on `{*slug}` vs correct `docs/{*slug}` template

**Answer**

_Answer not found._

---

#### Q8. (M) For the same conventional route `{controller=Home}/{action=Index}/{id?}`, contrast the HTTP status and routing outcome for:

1. `GET /home/index` when `Index` exists and allows GET  
2. `POST /home/index` when only GET is implemented (no `[HttpPost]` on `Index`)  
3. `GET /homme/index` (typo in controller name)

Explain how endpoint routing differs from "no route matched" vs "route matched, method not allowed."

**Concepts**
- 200 OK from exact route and method match
- 405 Method Not Allowed — route matched the path, verb rejected
- 404 Not Found — typo controller segment matches no registered controller
- Endpoint routing distinguishing path-match from method-match at selection time

**Answer**

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

**Concepts**
- `LowercaseUrls` applying only to Tag Helper and `Url.Action` generated output
- Hard-coded `Redirect("/Account/Login")` bypassing the routing option
- Linux case-sensitive path matching behind reverse proxies
- `Url.Action` respecting `LowercaseUrls` — hard-coded strings do not

**Answer**

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

**Concepts**
- `UseMvcWithDefaultRoute` — legacy IRouter-based middleware pipeline
- `MapControllerRoute` — endpoint routing in ASP.NET Core 3+
- Running both causing double route registration and 500 errors
- Endpoint routing vs IRouter — action selection timing and middleware access differences

**Answer**

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

**Concepts**
- Attribute route on `Summary` — `/api/reports/summary` is the endpoint
- Conventional route on `Index` — reachable via `/Reports/Index`
- Mixed routing on one controller — attribute-routed actions not reachable via conventional route
- Tag Helper generating conventional URL for an attribute-routed action

**Answer**

_Answer not found._
