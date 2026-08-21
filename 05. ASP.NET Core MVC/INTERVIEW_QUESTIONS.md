# ASP.NET Core MVC — Interview Questions (Extended Reference Bank)

Organized by the curriculum chapters under `06. ASP.NET Core MVC`.  
Overlapping questions are deduplicated; each topic appears once in its best-fit chapter.  
**Gotchas** at the end are real interview traps — patterns candidates commonly miss.

> **Purpose:** Long-term reference bank — questions only (no answers). Coverage-driven; chapter count varies by topic depth.  
> **Prerequisite:** Generic ASP.NET Core hosting, middleware, and DI are covered in `05. ASP.NET Core`.

---

## Chapter 01. Introduction to MVC Pattern

1. What is the MVC pattern?
2. What is the role of the Model in ASP.NET Core MVC?
3. What is the role of the View in ASP.NET Core MVC?
4. What is the role of the Controller in ASP.NET Core MVC?
5. What is the difference between MVC and MVP?
6. What is the difference between MVC and MVVM?
7. What is the difference between ASP.NET Core MVC and Razor Pages?
8. What is the difference between ASP.NET Core MVC and a SPA + Minimal API approach?
9. What does "thin controller" mean and why is it preferred?
10. What is the "fat controller" anti-pattern?
11. Where should business logic live in an MVC application?
12. Where should data access logic live in an MVC application?
13. Where should input validation live in an MVC application?
14. How does a request flow through Model, View, and Controller?
15. What is the difference between a domain entity and a ViewModel?
16. What symptoms appear when business logic is placed in the View?
17. What symptoms appear when data access is placed in the Controller?
18. What is Post-Redirect-Get (PRG) and why is it used in MVC?

---

## Chapter 02. Controllers & Actions

1. What is a controller in ASP.NET Core MVC?
2. What is an action method?
3. What is `IActionResult` and why use it instead of returning raw objects?
4. What is the difference between `View()` and `Json()`?
5. What is the difference between `RedirectToAction` and `Redirect`?
6. Why should action methods return `Task<IActionResult>` instead of `async void`?
7. How does dependency injection work in MVC controllers?
8. What does the `Controller` base class provide?
9. What is `ModelState` and when should an action check it?
10. What are `[HttpGet]` and `[HttpPost]` used for?
11. What is `[ValidateAntiForgeryToken]` and when is it required?
12. What is the difference between `[FromBody]`, `[FromForm]`, and default binding in MVC actions?
13. How are controllers activated per request?
14. What is the difference between returning `NotFound()` and `BadRequest()`?
15. Why should controllers avoid a static service locator?
16. What is the purpose of constructor injection in controllers?
17. What is the difference between conventional routing and attribute routing on a controller?
18. Can you implement `IDisposable` on a controller to manage resources? Why or why not?

---

## Chapter 03. Views & Razor Syntax

1. What is a Razor view?
2. What is the `@model` directive?
3. What is the difference between `@` and `@@` in Razor?
4. How does Razor automatically encode output and why does it matter?
5. What is the difference between `@Html.Raw` and default Razor output?
6. What is a code block (`@{ }`) in Razor?
7. What is the difference between a strongly typed view and a dynamic view?
8. What logic should not belong in a Razor view?
9. What is `@inject` used for in Razor views?
10. What is the `@functions` block in Razor?
11. What is the difference between Razor runtime compilation and precompilation?
12. When would you enable `AddRazorRuntimeCompilation`?
13. What is a partial view and when do you use one?
14. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?
15. What is `ViewData` and how is it accessed in Razor?
16. What causes "The view 'X' was not found" errors?
17. What is `_ViewImports.cshtml` used for?
18. What is `_ViewStart.cshtml` used for?

---

## Chapter 04. Layouts, Sections & Partial Views

1. What is a layout in ASP.NET Core MVC?
2. What does `@RenderBody()` do in a layout?
3. What is a section in Razor (`@section`)?
4. What is the difference between `@RenderSection("Scripts", required: true)` and `required: false`?
5. What is `_ViewStart.cshtml` and how does it apply layouts?
6. What is the difference between a layout and a partial view?
7. How do nested layouts work?
8. How are `@section` definitions passed from a view to a layout?
9. Can a view define the same section name twice? What happens?
10. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?
11. How does Razor locate partial views?
12. How does partial view resolution differ in Areas?
13. What is the difference between `@RenderSection` and `@await Html.PartialAsync`?
14. Why pass a strongly typed model to a partial instead of `ViewBag`?
15. How does a child view override the layout assigned in `_ViewStart`?
16. What happens if a required section is not defined in a view?
17. What is the `Shared` folder under `Views` used for?
18. How does `_ViewStart` layout resolution work in Areas?

---

## Chapter 05. ViewModels & Strongly Typed Views

1. What is a ViewModel in ASP.NET Core MVC?
2. What is the difference between a domain entity and a ViewModel?
3. What is a strongly typed view?
4. Why should you not pass EF entities directly to Razor views?
5. What is over-posting (mass assignment) and how do ViewModels prevent it?
6. What is the difference between a Create ViewModel and an Edit ViewModel?
7. What is the purpose of a read-only/details ViewModel?
8. Why should sensitive fields (e.g., `IsAdmin`, internal margin) be excluded from ViewModels?
9. What is the difference between mapping in the controller vs using AutoMapper?
10. What problems occur when one DTO is shared between MVC views and REST APIs?
11. How should navigation properties be handled in ViewModels for partial views?
12. Where should validation attributes be placed — entity or ViewModel?
13. What is the difference between presentation logic and business logic in ViewModels?
14. How do ViewModels help with unit testing controllers?
15. What is a composite/page ViewModel and when might you split it?
16. Why can returning an entity to `PartialView` cause serialization errors?
17. What is `[BindNever]` and when is it used on ViewModels?
18. How do nullable reference types affect ViewModel design?

---

## Chapter 06. Model Binding in MVC

1. What is model binding in ASP.NET Core MVC?
2. How does model binding work for HTML form POSTs?
3. What is the difference between `[FromForm]` and `[FromBody]` in MVC?
4. Why does `[FromBody]` fail when posting a standard HTML form?
5. How are collection properties bound from form fields (`Lines[0].Sku`)?
6. What happens when collection indices are non-contiguous after deleting a row?
7. How does model binding handle nested objects (`Address.City`)?
8. How do partial views affect model binding prefix for nested properties?
9. What happens when optional nullable fields (`int?`, `DateTime?`) are left empty?
10. How does culture affect date and number binding from form fields?
11. What is `RequestLocalization` and how does it relate to model binding?
12. How does file upload binding work (`IFormFile`)?
13. What `enctype` is required for file upload forms?
14. What is over-posting during model binding and how is it prevented?
15. What are `[BindNever]` and `[Bind]` used for?
16. How do checkboxes bind to `bool` and `bool?` properties?
17. What is the hidden-field pattern for checkboxes?
18. What is a custom `IModelBinder` and when would you create one?

---

## Chapter 07. Data Annotations & Validation

1. What are Data Annotations in ASP.NET Core MVC?
2. What is `ModelState` and how does it relate to validation?
3. Why must server-side validation always be performed even with client validation?
4. What does `ModelState.IsValid` check?
5. What is the difference between `[Required]` and `[AllowNull]`?
6. Why does `[Required]` not work as expected on a `bool` checkbox?
7. What is `[Compare]` used for?
8. What are `[Range]` and `[StringLength]` used for?
9. What is `[RegularExpression]` used for?
10. What is `IValidatableObject` and when do you use it?
11. What is the difference between `IValidatableObject` and a custom `ValidationAttribute`?
12. What is `[Remote]` validation and how does it work?
13. What is `[ValidateNever]` and when is it applied?
14. Why should validation attributes not be placed on EF entities shared with MVC?
15. What are `asp-validation-for` and `asp-validation-summary`?
16. What is the difference between `ValidationSummary` `ModelOnly` and `All`?
17. What is unobtrusive client validation?
18. What is the difference between Data Annotations and FluentValidation?

---

## Chapter 08. Tag Helpers

1. What are Tag Helpers in ASP.NET Core MVC?
2. What is the difference between Tag Helpers and HTML Helpers?
3. What does `asp-for` do on an input element?
4. What do `asp-action` and `asp-controller` do on a form or anchor?
5. What is `asp-validation-for`?
6. What is `asp-validation-summary`?
7. How do Tag Helpers generate antiforgery tokens for forms?
8. What is `asp-route-*` used for?
9. What does `asp-append-version` do?
10. What is the `<environment>` tag helper used for?
11. How are Tag Helpers registered in `_ViewImports.cshtml`?
12. What are `@addTagHelper` and `@removeTagHelper`?
13. What is the difference between `<partial>` and `Html.PartialAsync` as a tag helper?
14. What is the `!` prefix (opt-out) on Tag Helpers?
15. How does a custom Tag Helper work (`TagHelper` base class)?
16. What is Tag Helper processing order and why does it matter?
17. How do you register Tag Helpers from a Razor Class Library?
18. What HTML attributes do Tag Helpers emit for client-side validation (`data-val-*`)?

---

## Chapter 09. Routing & Attribute Routing

1. What is conventional routing in ASP.NET Core MVC?
2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?
3. What is attribute routing in MVC controllers?
4. What is the difference between conventional routing and attribute routing?
5. What are route constraints (e.g., `:int`, `:exists`)?
6. Why does route registration order matter in `MapControllerRoute`?
7. How does the `{area:exists}` constraint work?
8. What is the difference between `[Route]` on a controller vs on an action?
9. What does a leading slash in `[HttpGet("/export/{year}")]` mean?
10. What is a catch-all route parameter (`{*slug}`)?
11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?
12. What happens when two actions match the same route?
13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?
14. Why must `area` be specified when generating links to area controllers from outside the area?
15. What is `LowercaseUrls` and how does it affect link generation?
16. What is the difference between "no route matched" and "405 Method Not Allowed"?
17. How do optional route parameters (`{id?}`) and defaults interact?
18. What is the areas route pattern and how does it differ from the default route?

---

## Chapter 10. Areas

1. What are Areas in ASP.NET Core MVC?
2. Why use Areas instead of controller name prefixes?
3. What folder structure is required for an Area?
4. What is the `[Area("Admin")]` attribute and why is it required?
5. How is area routing registered in `Program.cs`?
6. What is the standard areas route pattern?
7. Why does route registration order matter for Areas?
8. How do you generate links to area controllers using Tag Helpers?
9. What happens when `asp-controller` is used without `asp-area` from within an Area view?
10. What is the difference between root `Controllers` and `Areas/Admin/Controllers`?
11. Can two controllers have the same name in different Areas?
12. Where should shared partials used by multiple Areas live?
13. How does layout resolution work for Area views?
14. What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for?
15. How do `_ViewImports` files scope between root Views and Area Views?
16. How do you apply authorization to an entire Area?
17. How do you map `/Admin` to a default dashboard action in the Admin area?
18. When should you use Areas vs Razor Class Libraries vs separate applications?

---

## Chapter 11. Action Filters in MVC

1. What are action filters in ASP.NET Core MVC?
2. What is the MVC filter pipeline?
3. What are the filter stages (authorization, resource, action, exception, result)?
4. What is the difference between action filters and middleware?
5. What are `IActionFilter` and `IAsyncActionFilter`?
6. What is `IAuthorizationFilter`?
7. What is `IExceptionFilter`?
8. What is `IResultFilter`?
9. What is `IResourceFilter`?
10. What is the difference between global, controller-level, and action-level filters?
11. How does filter order (`IOrderedFilter`) work?
12. What is the difference between `[ServiceFilter]` and `[TypeFilter]`?
13. How do you register a global filter in `AddControllersWithViews`?
14. What is `[ValidateAntiForgeryToken]` as a filter?
15. What is `[AutoValidateAntiforgeryToken]`?
16. What is `[IgnoreAntiforgeryToken]`?
17. What is `[Authorize]` as an authorization filter?
18. When should you use a filter instead of middleware for MVC-specific concerns?

---

## Chapter 12. TempData, ViewData & ViewBag

1. What is `ViewBag` in ASP.NET Core MVC?
2. What is `ViewData` and how does it differ from `ViewBag`?
3. What is `TempData` and when is it used?
4. What is the difference between `ViewBag`, `ViewData`, and `TempData`?
5. Why is `TempData` used after `RedirectToAction`?
6. What is the Post-Redirect-Get (PRG) pattern?
7. Why doesn't `ModelState` survive a redirect?
8. How can validation errors survive a redirect?
9. What is the difference between cookie-based and session-based TempData?
10. What happens to TempData when it is read?
11. What is `TempData.Keep()` used for?
12. What is `TempData.Peek()` used for?
13. What are the size limits of cookie-based TempData?
14. Why does session-based TempData fail behind load balancers without sticky sessions?
15. When should you use `ViewBag`/`ViewData` instead of a ViewModel?
16. When should you not use `ViewBag` for layout data?
17. Does TempData work on AJAX partial responses the same as full page redirects?
18. What data should never be stored in TempData?

---

## Chapter 13. AJAX & Partial Page Updates

1. What is a partial page update in ASP.NET Core MVC?
2. What is `PartialView()` and what does it return?
3. What is the difference between returning `PartialView` and `Json` from an AJAX action?
4. How does model binding differ for AJAX POST with `FormData` vs JSON?
5. How do you include an antiforgery token in a `fetch`/AJAX request?
6. What is `RequestVerificationToken` and how is it validated on AJAX POSTs?
7. What is the difference between `[ValidateAntiForgeryToken]` and `[AutoValidateAntiforgeryToken]` for AJAX?
8. What is unobtrusive AJAX (`data-ajax="true"`)?
9. Why should AJAX partial endpoints check response status before injecting HTML?
10. What `Cache-Control` headers should dynamic partial views use?
11. What XSS risks exist when injecting server-rendered HTML via `innerHTML`?
12. Why do duplicate HTML `id` attributes break AJAX-loaded partials?
13. What is the difference between `Html.PartialAsync` returned from an action vs a full `View`?
14. How do you handle validation errors in AJAX form submissions?
15. What happens when `UseExceptionHandler` returns a full error page to a partial AJAX request?
16. What is `[FromBody]` vs form-urlencoded binding for AJAX filter endpoints?
17. What is the difference between jQuery unobtrusive AJAX and `fetch` + `innerHTML`?
18. How do you design separate actions for full-page POST vs AJAX POST?

---

## Chapter 14. Client-Side Validation

1. What is client-side validation in ASP.NET Core MVC?
2. What is unobtrusive validation?
3. What scripts are required for unobtrusive client validation?
4. What is `_ValidationScriptsPartial`?
5. What are `data-val-*` attributes and how are they generated?
6. What is the relationship between Data Annotations and client-side validation?
7. Why is client-side validation not sufficient for security?
8. What is `jquery.validate.unobtrusive.js` responsible for?
9. What is `asp-validation-for` used for?
10. What is the difference between `asp-validation-summary="All"` and `"ModelOnly"`?
11. How does `[Remote]` validation work on the client?
12. What is a client validation adapter for custom `ValidationAttribute`s?
13. What is `ClientValidationEnabled` on `ViewContext`?
14. Why does hand-written HTML input lose client-side validation?
15. How do you localize jQuery Validate error messages?
16. Why must server actions still check `ModelState.IsValid` when client validation is enabled?
17. Why doesn't client validation fire when using a button click handler instead of form submit?
18. What is the difference between MVC unobtrusive validation and SPA/API validation?

---

## Chapter 15. Real-Time UI with SignalR

1. What is SignalR and how does it relate to ASP.NET Core MVC?
2. What is a SignalR Hub?
3. What is the difference between a Hub and an MVC controller?
4. How do you map a Hub endpoint in `Program.cs`?
5. What is `IHubContext` and why is it used from MVC controllers?
6. Why should you not inject a Hub directly into a controller?
7. What is a SignalR backplane (e.g., Redis) and when is it needed?
8. What is the difference between sticky sessions and a SignalR backplane?
9. How does cookie authentication apply to SignalR connections?
10. What are SignalR Groups and how do clients join them?
11. What happens to group membership on SignalR reconnect?
12. What is the negotiate step in SignalR?
13. What CORS settings are required for cross-origin SignalR connections?
14. What is Azure SignalR Service and when would you use it?
15. What is `HubException` and how should hub errors be returned to clients?
16. How do you invoke hub methods from JavaScript in a Razor view?
17. What are `Clients.All`, `Clients.Caller`, and `Clients.Others`?
18. How does `PathBase` affect SignalR hub URLs behind a reverse proxy?

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

Pricing, discount, or authorization checks in `.cshtml` files bypass unit tests and duplicate rules already in services — views should only render data the controller prepared.

#### Gotcha 2. EF entities passed directly to views

Binding and displaying EF entities exposes navigation properties, causes over-posting on POST, and ties the UI to the database schema — use ViewModels instead.

#### Gotcha 3. `[FromBody]` on HTML form POST

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON — `[FromBody]` leaves the model empty while the action runs with default values.

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

Client-side validation is bypassable — attackers can POST directly without scripts; server-side validation is mandatory before any persist operation.

#### Gotcha 5. `return View()` after successful POST

Returning the same view after POST causes duplicate submission on refresh — use Post-Redirect-Get (`RedirectToAction`) instead.

#### Gotcha 6. `ModelState` after redirect

`ModelState` is request-scoped and does not survive `RedirectToAction` — rehydrate errors via TempData, a second GET validation pass, or redisplay the form without redirect on failure only.

#### Gotcha 7. TempData read twice in layout and view

TempData is consumed on first read — if the layout reads a flash message, the view sees nothing unless you use `Peek()` or `Keep()`.

#### Gotcha 8. Missing `[Area]` attribute on area controllers

Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong route.

#### Gotcha 9. Link generation without `asp-area`

Tag Helpers default to the current area context — links from a root view to an area controller need explicit `asp-area="Admin"` or they generate wrong URLs.

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

A missing checkbox posts nothing and binds as `false`, so `[Required]` never fails — use `bool?` or a hidden field plus server-side check for explicit consent.

#### Gotcha 11. Collection binding with gap indices

Deleting row 1 from a dynamic form leaving `Lines[0]` and `Lines[2]` breaks model binder alignment — reindex client-side or use a custom binder.

#### Gotcha 12. `@Html.Raw` with user content

Default Razor encoding prevents XSS — `@Html.Raw(Model.UserComment)` renders attacker script if the content is not sanitized server-side.

#### Gotcha 13. AJAX POST without antiforgery token

Form Tag Helpers emit tokens automatically; `fetch` and jQuery AJAX must send `RequestVerificationToken` header or field or POSTs fail with 400 antiforgery errors.

#### Gotcha 14. Injecting Hub into MVC controller

Hubs are not registered in DI for direct injection — use `IHubContext<THub>` from controllers to broadcast without coupling to connection lifecycle.

#### Gotcha 15. SignalR scale-out without backplane

Sticky sessions alone do not fan-out events across instances — multi-node deployments need Redis backplane or Azure SignalR Service.
