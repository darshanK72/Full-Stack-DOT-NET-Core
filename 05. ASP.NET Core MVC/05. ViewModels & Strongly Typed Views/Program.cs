/*
 * TOPIC: ViewModels & Strongly Typed Views
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   A domain entity (Product, Order, Customer) is shaped for the database and
 *   business rules — it is NOT shaped for what a view needs to display. Passing
 *   a domain entity directly into a view creates tight coupling, leaks internal
 *   fields, and makes testing and maintenance harder. ViewModels are the bridge:
 *   plain C# classes (or records) shaped for ONE specific view, carrying exactly
 *   the data that view needs, nothing more.
 *
 *   Strong typing (@model directive) lets the Razor compiler verify every
 *   property reference at build time, enables IntelliSense in the editor, and
 *   unlocks the expression-based HTML helpers (Html.DisplayFor, Html.EditorFor)
 *   that read [Display] / [DisplayFormat] metadata automatically.
 *
 * WHAT YOU WILL LEARN:
 *    1. MVC services registration (AddControllersWithViews)
 *    2. MVC middleware and default route setup
 *    3. Why ViewModels exist — domain model vs view concerns
 *    4. Domain entity shape (Product) and what the view should NOT see
 *    5. Related domain entity (Category) and navigation properties
 *    6. ViewModels vs DTOs vs domain entities — comparison and when to use each
 *    7. ProductSummaryViewModel — per-row list item with [Display] and [DisplayFormat]
 *    8. ProductListViewModel — IEnumerable<T> as model, filter state, naming convention
 *    9. Composite ViewModels — embedding multiple entity shapes in one VM
 *   10. CategorySummaryViewModel — nested VM component
 *   11. ProductDetailViewModel — composite VM combining product + category + related list
 *   12. Form ViewModels — separate from display VMs; hold input + dropdown data
 *   13. CategoryOption — lightweight DTO keeping ViewModels framework-independent
 *   14. ProductFormViewModel — [Display], [DisplayFormat], [Required], [Range], nullable types
 *   15. Multi-entity composite ViewModels for dashboards
 *   16. DashboardStats — nested stats VM
 *   17. DashboardViewModel — composing unrelated entities
 *   18. Manual domain → ViewModel mapping (PREVIEW: AutoMapper)
 *   19. Index action — building and passing ProductListViewModel
 *   20. Detail action — building ProductDetailViewModel
 *   21. Form GET + POST — building and consuming ProductFormViewModel
 *   22. Dashboard action — building DashboardViewModel
 *   23. ViewBag/ViewData alongside @model (anti-pattern explanation)
 *   24. @model directive, Html.DisplayNameFor, Html.DisplayFor, Html.EditorFor in views
 *
 * CHAPTER MAP (open files in this order):
 *   SECTION  1  → Program.cs                              MVC services registration
 *   SECTION  2  → Program.cs                              Middleware pipeline
 *   SECTION  3  → Program.cs                              Default route
 *   SECTION  4  → Domain/Product.cs                       Domain entity vs ViewModel
 *   SECTION  5  → Domain/Product.cs                       Product domain entity
 *   SECTION  6  → Domain/Category.cs                      Category domain entity
 *   SECTION  7  → ViewModels/ProductListViewModel.cs       ViewModels vs DTOs vs entities
 *   SECTION  8  → ViewModels/ProductListViewModel.cs       ProductSummaryViewModel
 *   SECTION  9  → ViewModels/ProductListViewModel.cs       ProductListViewModel
 *   SECTION 10  → ViewModels/ProductDetailViewModel.cs     Composite ViewModels
 *   SECTION 11  → ViewModels/ProductDetailViewModel.cs     CategorySummaryViewModel
 *   SECTION 12  → ViewModels/ProductDetailViewModel.cs     ProductDetailViewModel
 *   SECTION 13  → ViewModels/ProductFormViewModel.cs       Form ViewModels
 *   SECTION 14  → ViewModels/ProductFormViewModel.cs       CategoryOption
 *   SECTION 15  → ViewModels/ProductFormViewModel.cs       ProductFormViewModel
 *   SECTION 16  → ViewModels/DashboardViewModel.cs         Multi-entity composite ViewModels
 *   SECTION 17  → ViewModels/DashboardViewModel.cs         DashboardStats
 *   SECTION 18  → ViewModels/DashboardViewModel.cs         DashboardViewModel
 *   SECTION 19  → Controllers/ProductsController.cs        Manual domain→ViewModel mapping
 *   SECTION 20  → Controllers/ProductsController.cs        Index action
 *   SECTION 21  → Controllers/ProductsController.cs        Detail action
 *   SECTION 22  → Controllers/ProductsController.cs        Form GET + POST
 *   SECTION 23  → Controllers/ProductsController.cs        Dashboard action
 *   SECTION 24  → Controllers/ProductsController.cs        ViewBag/ViewData anti-pattern
 *       Views   → Views/Products/Index.cshtml              @model, DisplayNameFor, DisplayFor
 *       Views   → Views/Products/Detail.cshtml             Composite VM display
 *       Views   → Views/Products/Form.cshtml               EditorFor, LabelFor, DropDownListFor
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 * SECTION 1: MVC SERVICES REGISTRATION
 * ─────────────────────────────────────────────────────────────────────────────
 * AddControllersWithViews() registers the full MVC stack into the DI container:
 *
 *   What it registers                          Purpose
 *   ─────────────────────────────────────────  ──────────────────────────────────
 *   Controller discovery + activation          Finds controller classes, resolves them
 *   IViewEngine (Razor)                        Compiles and renders .cshtml files
 *   IHtmlGenerator                             Generates Html.* helper output
 *   IModelMetadataProvider                     Reads [Display], [DisplayFormat] attributes
 *   IModelBinderFactory                        Binds HTTP form data → ViewModel properties
 *   IObjectModelValidator                      Runs [Required], [Range] etc. on POST
 *   TempData + ViewData infrastructure         Allows ViewBag/TempData alongside @model
 *   Antiforgery token services                 CSRF protection for form POSTs
 *
 * Compare to alternatives:
 *   AddControllers()            — Web API only; no Razor views
 *   AddRazorPages()             — Razor Pages only; no MVC controllers
 *   AddControllersWithViews()   — MVC: controllers + Razor views (this chapter)
 *   AddMvc()                    — controllers + views + Razor Pages (all three)
 *
 * COVERED IN DETAIL LATER → 02. Controllers & Actions
 */
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(); // registers MVC + Razor view engine

/*
 * SECTION 2: MIDDLEWARE PIPELINE FOR MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * An MVC application uses a standard middleware pipeline. Order matters:
 *
 *   Middleware                   Purpose
 *   ─────────────────────────── ─────────────────────────────────────────────
 *   UseHttpsRedirection()        Redirect HTTP → HTTPS (security)
 *   UseStaticFiles()             Serve wwwroot/ files before routing (short-circuit)
 *   UseRouting()                 Match URL to controller/action endpoint
 *   UseAuthorization()           Check [Authorize] attributes on matched endpoint
 *   UseEndpoints / MapController Map conventional MVC routes
 *
 * In .NET 6+ minimal hosting, UseRouting and UseEndpoints are implied when you
 * call MapControllerRoute(), so you only need to call them explicitly if you
 * need middleware to run AFTER routing but BEFORE endpoint execution (e.g.,
 * UseAuthorization which requires the endpoint to be matched first).
 */
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // detailed error page; disable in production
}
else
{
    app.UseExceptionHandler("/Products/Error"); // production error handling
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // serve wwwroot/css, wwwroot/js, wwwroot/lib

/*
 * SECTION 3: DEFAULT MVC ROUTE
 * ─────────────────────────────────────────────────────────────────────────────
 * Conventional routing maps URL segments to controller/action/id:
 *
 *   Pattern:  {controller}/{action}/{id?}
 *
 *   URL                         Controller   Action   id
 *   ──────────────────────────  ───────────  ───────  ──────
 *   /Products                   Products     Index    (none)
 *   /Products/Detail/3          Products     Detail   3
 *   /Products/Form              Products     Form     (none)
 *   /Products/Form/3            Products     Form     3
 *   /Products/Dashboard         Products     Dashboard (none)
 *   /                           Products     Index    (none) ← defaults applied
 *
 * The {id?} segment is optional (the ? makes it nullable). When present, the
 * framework binds it to any action parameter named `id` (int?, string, etc.).
 *
 * Alternatives:
 *   Attribute routing   [Route("products/{id}")]  on controller/action
 *   Razor Pages         /Pages/Products/Index.cshtml maps to /Products
 *   Minimal APIs        app.MapGet("/products/{id}", handler)
 *
 * COVERED IN DETAIL LATER → 09. Routing & Attribute Routing
 */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}"); // Products/Index is the home page

app.Run();

/*
 * QUICK REFERENCE — ViewModels & Strongly Typed Views
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * 1. DECLARE A VIEWMODEL
 *    public class ProductDetailViewModel
 *    {
 *        [Display(Name = "Product Name")]
 *        public string Name { get; set; } = string.Empty;
 *
 *        [DisplayFormat(DataFormatString = "{0:C}")]
 *        public decimal Price { get; set; }
 *    }
 *
 * 2. PASS VM FROM CONTROLLER
 *    public IActionResult Detail(int id)
 *    {
 *        Product product = GetById(id);           // get from domain / service
 *        var vm = new ProductDetailViewModel      // MAP domain → VM
 *        {
 *            Name  = product.Name,
 *            Price = product.Price,
 *        };
 *        return View(vm);                         // View(viewModel) passes it to the view
 *    }
 *
 * 3. DECLARE MODEL IN VIEW
 *    @model ProductDetailViewModel
 *    <h1>@Model.Name</h1>
 *    @Html.DisplayFor(m => m.Price)               // respects [DisplayFormat]
 *    @Html.DisplayNameFor(m => m.Price)           // reads [Display(Name = "...")]
 *
 * 4. EXPRESSION-BASED HTML HELPERS
 *    Html.DisplayFor(m => m.Prop)                 // render value using [DisplayFormat]
 *    Html.DisplayNameFor(m => m.Prop)             // render label using [Display(Name)]
 *    Html.EditorFor(m => m.Prop)                  // render input using [DataType], [UIHint]
 *    Html.LabelFor(m => m.Prop)                   // <label> with [Display(Name)]
 *    Html.ValidationMessageFor(m => m.Prop)       // inline validation error span
 *    Html.DropDownListFor(m => m.Id, selectList)  // <select> bound to a property
 *
 * 5. VIEWMODEL NAMING CONVENTIONS
 *    {Controller}ViewModel        ProductsViewModel     — shared across actions
 *    {Action}ViewModel            IndexViewModel        — action-specific
 *    {Feature}ViewModel           DashboardViewModel    — feature/page-specific (most common)
 *
 * 6. VIEWMODELS vs DTOs vs DOMAIN ENTITIES
 *    Domain entity — database shape; has navigation properties; ORM-tracked
 *    DTO           — data transfer over the wire (API request/response bodies)
 *    ViewModel     — shaped for ONE view; has display metadata; may be composite
 *
 * 7. VIEWBAG / VIEWDATA — USE SPARINGLY
 *    ViewBag.Title = "My Page";                   // dynamic; string Title in ViewData
 *    @ViewData["Title"]                           // read in view
 *    ANTI-PATTERN: using ViewBag to pass model data when @model can carry it.
 *    ACCEPTABLE:   ViewData["Title"] for layout page metadata (standard MVC convention).
 *
 * 8. NULLABLE REFERENCE TYPES IN VIEWMODELS
 *    string Name { get; set; } = string.Empty;   // required; non-null; no CS8618 warning
 *    string? Description { get; set; }            // optional; explicitly nullable
 *
 * NEXT CHAPTERS:
 *   06. Model Binding in MVC     — how POST data maps into ViewModel properties
 *   07. Data Annotations         — [Required], [Range], [EmailAddress] validation depth
 *   08. Tag Helpers              — modern alternative to Html.* expression helpers
 */
