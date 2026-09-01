/*
 * FILE ROLE: ProductsController is the bridge between domain entities and
 *            ViewModels. Every action fetches domain data, maps it to a
 *            ViewModel, and passes the ViewModel to the view. No domain entity
 *            ever touches the view layer directly — the controller absorbs all
 *            mapping responsibility.
 * SECTIONS IN THIS FILE:
 *  19. Manual domain → ViewModel mapping (static mapper methods)
 *  20. Index action — building and passing ProductListViewModel
 *  21. Detail action — building ProductDetailViewModel
 *  22. Form GET + POST — building and consuming ProductFormViewModel
 *  23. Dashboard action — building DashboardViewModel
 *  24. ViewBag/ViewData anti-pattern explanation
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Domain;
using ViewModels.ViewModels;

namespace ViewModels.Controllers;

public class ProductsController : Controller
{
    // ─── SEED DATA (simulates a repository / database) ───────────────────────
    // In a real application these would be injected services (IProductRepository,
    // ICategoryRepository) resolved from the DI container via constructor injection.
    // Static lists let this tutorial compile and demonstrate mapping without a database.

    private static readonly List<Category> _categories = new List<Category>
    {
        new Category { Id = 1, Name = "Electronics",  Description = "Gadgets and devices" },
        new Category { Id = 2, Name = "Books",        Description = "Printed and digital books" },
        new Category { Id = 3, Name = "Clothing",     Description = null },
    };

    private static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Laptop Pro",      Description = "High-performance laptop",                Price = 1299.99m, StockQuantity = 15,  IsActive = true,  CreatedAt = new DateTime(2024, 1, 10), CategoryId = 1, InternalCost = 800m  },
        new Product { Id = 2, Name = "Wireless Mouse",  Description = null,                                     Price = 29.99m,   StockQuantity = 0,   IsActive = false, CreatedAt = new DateTime(2024, 2,  5), CategoryId = 1, InternalCost = 12m   },
        new Product { Id = 3, Name = "Clean Code",      Description = "Handbook of agile software craftsmanship", Price = 39.99m,  StockQuantity = 42,  IsActive = true,  CreatedAt = new DateTime(2024, 3,  1), CategoryId = 2, InternalCost = 15m   },
        new Product { Id = 4, Name = "T-Shirt Classic", Description = null,                                     Price = 19.99m,   StockQuantity = 100, IsActive = true,  CreatedAt = new DateTime(2024, 4, 12), CategoryId = 3, InternalCost = 5m    },
        new Product { Id = 5, Name = "C# in Depth",     Description = "Advanced C# programming guide",          Price = 49.99m,   StockQuantity = 28,  IsActive = true,  CreatedAt = new DateTime(2024, 5, 20), CategoryId = 2, InternalCost = 20m   },
    };

    // ─────────────────────────────────────────────────────────────────────────

    /*
     * SECTION 19: MANUAL DOMAIN → VIEWMODEL MAPPING
     * ─────────────────────────────────────────────────────────────────────────
     * The controller is responsible for ALL domain-to-ViewModel translation.
     * Mapping is done in private static methods, keeping the action methods clean.
     *
     * MANUAL MAPPING (this file):
     *   Pro: No dependencies. Explicit — you can see every field that is included or
     *        omitted. Easy to step through in a debugger.
     *   Con: Verbose for large entities. New domain fields are silently missed if you
     *        forget to add them to the mapper.
     *
     * AUTOMAPPER (PREVIEW):
     *   var config = new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductSummaryViewModel>());
     *   var mapper = config.CreateMapper();
     *   ProductSummaryViewModel vm = mapper.Map<ProductSummaryViewModel>(product);
     *   Pro: Convention-based (same-named properties map automatically); less boilerplate.
     *   Con: Magic mapping hides mismatches; runtime errors instead of compile errors.
     *   COVERED IN DETAIL LATER → AutoMapper chapter.
     *
     * MAPSTER (PREVIEW):
     *   A popular alternative mapper with better performance than AutoMapper.
     *
     * RECOMMENDATION: Use manual mapping in small projects and tutorials.
     * Introduce a mapper library only when the mapping surface is large enough
     * to justify the dependency.
     *
     * KEY DECISIONS MADE IN THESE MAPPERS:
     *   InternalCost     → OMITTED entirely (sensitive; the form VM never exposes it)
     *   Category nav prop → RESOLVED to a string name (prevents lazy-load side effects)
     *   CreatedAt        → passed as raw DateTime; [DisplayFormat] on the VM formats it
     *   Description null → flows through as string? — the view shows a fallback string
     */

    // Map one Product domain entity to the per-row summary VM used in the list view.
    private static ProductSummaryViewModel MapToSummary(Product p)
    {
        string? categoryName = _categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name; // null if not found
        return new ProductSummaryViewModel
        {
            Id           = p.Id,
            Name         = p.Name,
            CategoryName = categoryName,  // nullable string — view uses ?? to show fallback
            Price        = p.Price,
            IsActive     = p.IsActive,
        };
    }

    // Map one Product + its related products to the composite detail VM.
    private static ProductDetailViewModel MapToDetail(Product p, IEnumerable<Product> relatedProducts)
    {
        Category? category = _categories.FirstOrDefault(c => c.Id == p.CategoryId); // nullable
        int productCountInCategory = _products.Count(x => x.CategoryId == p.CategoryId);

        return new ProductDetailViewModel
        {
            ProductId     = p.Id,
            Name          = p.Name,
            Description   = p.Description,       // nullable — may be null; view handles it
            Price         = p.Price,
            StockQuantity = p.StockQuantity,
            IsActive      = p.IsActive,
            CreatedAt     = p.CreatedAt,
            Category = new CategorySummaryViewModel  // map domain Category → nested VM
            {
                Name         = category?.Name        ?? "Unknown",  // non-null fallback
                Description  = category?.Description,               // nullable is fine
                ProductCount = productCountInCategory,
            },
            RelatedProducts = relatedProducts.Select(MapToSummary).ToList(), // reuses summary mapper
        };
    }

    // Map one Product domain entity to the create/edit form VM.
    private static ProductFormViewModel MapToForm(Product p)
    {
        return new ProductFormViewModel
        {
            Id            = p.Id,           // non-null for edit; the form uses this to determine Create vs Edit
            Name          = p.Name,
            Description   = p.Description,
            Price         = p.Price,
            StockQuantity = p.StockQuantity,
            IsActive      = p.IsActive,
            CategoryId    = p.CategoryId,
            // CategoryOptions is intentionally left empty here.
            // The calling action (Form GET / failed Form POST) populates it separately.
        };
    }

    // Convert domain Category entities to the lightweight dropdown DTO.
    private static IEnumerable<CategoryOption> GetCategoryOptions()
    {
        return _categories
            .Select(c => new CategoryOption { Id = c.Id, Name = c.Name })
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────

    /*
     * SECTION 20: INDEX ACTION — BUILDING ProductListViewModel
     * ─────────────────────────────────────────────────────────────────────────
     * Demonstrates:
     *   1. Simple-type parameter binding from the query string (automatic for
     *      string? and int? — no [FromQuery] attribute needed for primitive types)
     *   2. LINQ filtering over domain entities before mapping
     *   3. Mapping filtered results to per-row summary VMs
     *   4. Wrapping the collection + filter state in a list wrapper VM
     *   5. Passing the typed VM to the view via return View(vm)
     *
     * ROUTING (matched by the default route {controller}/{action}/{id?}):
     *   GET /Products                          → Index(null, null)       all products
     *   GET /Products?searchTerm=laptop        → Index("laptop", null)   filtered by name
     *   GET /Products?categoryFilter=1         → Index(null, 1)          filtered by category
     *   GET /Products?searchTerm=c%23&categoryFilter=2 → both filters applied
     *
     * RETURNING FILTER STATE TO THE VIEW:
     *   vm.SearchTerm = searchTerm tells the view what search is currently active so
     *   it can pre-populate the search input. Without this, submitting the filter
     *   clears the input, confusing the user about what was searched.
     */
    public IActionResult Index(string? searchTerm = null, int? categoryFilter = null)
    {
        IEnumerable<Product> query = _products; // start with all; filter lazily

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        if (categoryFilter.HasValue)
            query = query.Where(p => p.CategoryId == categoryFilter.Value);

        List<Product> filtered = query.ToList(); // materialize once; reuse Count + Select

        var vm = new ProductListViewModel
        {
            Products        = filtered.Select(MapToSummary).ToList(), // map domain → per-row VMs
            SearchTerm      = searchTerm,       // return filter state so the view can show it
            CategoryFilter  = categoryFilter,
            TotalCount      = filtered.Count,   // count after filtering (before pagination in real apps)
            CategoryOptions = GetCategoryOptions(), // dropdown options for the filter form
        };

        ViewData["Title"] = "Products"; // acceptable ViewBag use — layout page title (see SECTION 24)
        return View(vm);                // convention: looks for Views/Products/Index.cshtml
    }

    /*
     * SECTION 21: DETAIL ACTION — BUILDING ProductDetailViewModel
     * ─────────────────────────────────────────────────────────────────────────
     * Demonstrates:
     *   1. Route parameter binding (id from /Products/Detail/3)
     *   2. Null check with NotFound() for missing resources
     *   3. Building a composite ViewModel from multiple domain queries
     *   4. Passing a nested VM (CategorySummaryViewModel) inside the detail VM
     *   5. Passing a collection (RelatedProducts) inside the detail VM
     *
     * The controller fetches ALL needed data before mapping. The view receives
     * a completely flat, pre-computed ViewModel — no navigation properties,
     * no lazy-loading, no ORM tracking objects.
     *
     * IActionResult (vs ViewResult):
     *   IActionResult is the standard return type because the action can return
     *   either View(vm) (a ViewResult) or NotFound() (a NotFoundResult). Both
     *   implement IActionResult. Using the concrete ViewResult would prevent the
     *   NotFound() return.
     */
    public IActionResult Detail(int id)
    {
        Product? product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound(); // 404 — product does not exist; do not attempt to render a view

        // Fetch related products: same category, different product, limit to 3
        IEnumerable<Product> related = _products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
            .Take(3);

        ProductDetailViewModel vm = MapToDetail(product, related);
        ViewData["Title"] = vm.Name;     // page title uses the product name
        return View(vm);                 // convention: looks for Views/Products/Detail.cshtml
    }

    /*
     * SECTION 22: FORM GET — BUILDING ProductFormViewModel
     * ─────────────────────────────────────────────────────────────────────────
     * The Form action handles both Create and Edit with one method pair.
     * The nullable int? id parameter signals which mode is active:
     *
     *   GET /Products/Form          → id is null → Create (blank form)
     *   GET /Products/Form/3        → id = 3     → Edit (pre-filled form)
     *
     * CRITICAL: CategoryOptions MUST be populated before returning the view.
     * This is a frequent omission bug in real applications. If CategoryOptions
     * is null when the view calls @Html.DropDownListFor, a NullReferenceException
     * is thrown at runtime. The fix: always call GetCategoryOptions() on every
     * GET and on every failed POST before returning the view.
     *
     * [HttpGet] is the default for controller actions (no annotation = GET).
     * Adding it explicitly avoids confusion when the POST version is right below.
     */
    [HttpGet]
    public IActionResult Form(int? id = null)
    {
        ProductFormViewModel vm;

        if (id.HasValue)
        {
            // Edit mode: find the existing product, map it to the form VM
            Product? product = _products.FirstOrDefault(p => p.Id == id.Value);
            if (product is null)
                return NotFound();
            vm = MapToForm(product);
        }
        else
        {
            // Create mode: start with an empty VM (IsActive defaults to true in the class)
            vm = new ProductFormViewModel();
        }

        vm.CategoryOptions = GetCategoryOptions(); // always populate dropdown before rendering
        ViewData["Title"] = id.HasValue ? "Edit Product" : "Add Product";
        return View(vm); // convention: looks for Views/Products/Form.cshtml
    }

    /*
     * SECTION 22 (continued): FORM POST — CONSUMING ProductFormViewModel
     * ─────────────────────────────────────────────────────────────────────────
     * On form submit the MVC MODEL BINDER maps HTTP form fields → VM properties
     * by matching field name to property name (case-insensitive):
     *
     *   <input name="Name"         value="Laptop" />  → vm.Name = "Laptop"
     *   <input name="Price"        value="79.99"  />  → vm.Price = 79.99m  (parsed by binder)
     *   <input name="CategoryId"   value="1"      />  → vm.CategoryId = 1
     *   <input name="IsActive"     value="true"   />  → vm.IsActive = true
     *
     * CategoryOptions is NOT submitted (complex collections not in standard form encoding).
     * The controller must re-populate it on a failed POST before returning View(vm).
     *
     * [ValidateAntiForgeryToken]:
     *   Verifies the hidden __RequestVerificationToken field injected by
     *   @Html.AntiForgeryToken() in the view. Prevents CSRF attacks where a
     *   malicious page submits a form to your endpoint using the user's session.
     *   AddControllersWithViews() registers the required antiforgery services.
     *
     * POST-REDIRECT-GET (PRG) PATTERN:
     *   After a successful POST, redirect to a GET action (RedirectToAction).
     *   This prevents the browser from re-submitting the form on page refresh.
     *   Skipping PRG causes "Confirm Form Resubmission?" dialogs and duplicate
     *   records when the user refreshes the success page.
     */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Form(ProductFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = GetCategoryOptions(); // re-populate before re-rendering (critical)
            return View(vm); // return the form — validation errors shown by @Html.ValidationMessageFor
        }

        if (vm.Id.HasValue)
        {
            // Edit: find and update the existing domain entity
            Product? existing = _products.FirstOrDefault(p => p.Id == vm.Id.Value);
            if (existing is null)
                return NotFound();

            // Map ViewModel → domain entity (only the editable fields)
            existing.Name          = vm.Name;
            existing.Description   = vm.Description;
            existing.Price         = vm.Price;
            existing.StockQuantity = vm.StockQuantity;
            existing.IsActive      = vm.IsActive;
            existing.CategoryId    = vm.CategoryId;
            // InternalCost is NOT updated — the form VM never exposed it (over-posting protection)

            return RedirectToAction(nameof(Detail), new { id = existing.Id }); // PRG: redirect to GET
        }
        else
        {
            // Create: add a new domain entity to the in-memory list
            int newId = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
            var newProduct = new Product
            {
                Id            = newId,
                Name          = vm.Name,
                Description   = vm.Description,
                Price         = vm.Price,
                StockQuantity = vm.StockQuantity,
                IsActive      = vm.IsActive,
                CategoryId    = vm.CategoryId,
                CreatedAt     = DateTime.UtcNow,
                InternalCost  = 0m, // not set by the form — set by a separate internal process
            };
            _products.Add(newProduct);
            return RedirectToAction(nameof(Detail), new { id = newProduct.Id }); // PRG: redirect to GET
        }
    }

    /*
     * SECTION 23: DASHBOARD ACTION — BUILDING DashboardViewModel
     * ─────────────────────────────────────────────────────────────────────────
     * The dashboard aggregates data from multiple domain collections into a
     * single composite ViewModel. Every property the view accesses is pre-computed
     * in the controller — the view never queries data or performs business logic.
     *
     * This is the controller-as-orchestrator pattern in its purest form:
     *   1. Query each data source independently
     *   2. Project/aggregate results into sub-VMs
     *   3. Assemble sub-VMs into the top-level composite VM
     *   4. Pass to the view via return View(vm)
     *
     * The view (not created in this chapter — see note below) would declare:
     *   @model DashboardViewModel
     * and navigate: @Model.Stats.TotalProducts, @foreach (var p in Model.RecentProducts)
     *
     * NOTE: Views/Products/Dashboard.cshtml is not created in this chapter because
     * the Index, Detail, and Form views cover all the Razor techniques shown in the
     * chapter map. The Dashboard pattern is identical — a different @model on the
     * same @Html.DisplayFor / @Html.DisplayNameFor helpers.
     */
    public IActionResult Dashboard()
    {
        var stats = new DashboardStats
        {
            TotalProducts      = _products.Count,
            ActiveProducts     = _products.Count(p => p.IsActive),
            OutOfStockProducts = _products.Count(p => p.StockQuantity == 0),
            TotalCategories    = _categories.Count,
            AveragePrice       = _products.Any() ? _products.Average(p => p.Price) : 0m,
        };

        IEnumerable<ProductSummaryViewModel> recent = _products
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(MapToSummary)
            .ToList();

        IEnumerable<CategorySummaryViewModel> topCategories = _categories
            .Select(c => new CategorySummaryViewModel
            {
                Name         = c.Name,
                Description  = c.Description,
                ProductCount = _products.Count(p => p.CategoryId == c.Id),
            })
            .OrderByDescending(c => c.ProductCount)
            .Take(3)
            .ToList();

        var vm = new DashboardViewModel
        {
            Stats          = stats,
            RecentProducts = recent,
            TopCategories  = topCategories,
        };

        ViewData["Title"] = "Dashboard";
        return View(vm); // looks for Views/Products/Dashboard.cshtml (not in this chapter's scope)
    }

    /*
     * SECTION 24: VIEWBAG / VIEWDATA — ANTI-PATTERN EXPLANATION
     * ─────────────────────────────────────────────────────────────────────────
     * ViewBag and ViewData pass extra data from a controller to a view alongside
     * the typed @model. They look convenient but introduce stringly-typed coupling.
     *
     * VIEWDATA — Dictionary<string, object?>:
     *   Controller:  ViewData["Title"] = "My Products";
     *   View:        @ViewData["Title"]                → string-keyed lookup; typo = null, not an error
     *                (string)ViewData["SomeObject"]    → cast required; no compile-time check
     *
     * VIEWBAG — dynamic wrapper over ViewData (same underlying dictionary):
     *   Controller:  ViewBag.ProductCount = 42;
     *   View:        @ViewBag.ProductCount             → dynamic; no cast; no IntelliSense
     *                @ViewBag.ProducCount              → TYPO — compiles; returns null at runtime
     *
     * THE ONE ACCEPTED CONVENTION — page title:
     *   ViewData["Title"] = "Products";   // set in the action (see Index, Detail above)
     *   @ViewData["Title"]                // read in _Layout.cshtml <title> element
     *
     *   This is the standard MVC convention for the browser tab title. It is acceptable
     *   because it is a well-understood layout concern, not business data. All actions
     *   in this controller follow this convention.
     *
     * ANTI-PATTERNS TO AVOID:
     *
     *   BAD — passing model data through ViewBag:
     *     ViewBag.CategoryOptions = GetCategoryOptions();
     *     return View();   // @model is missing or null
     *     // View must cast: ((IEnumerable<CategoryOption>)ViewBag.CategoryOptions)
     *     // A typo ("CategoryOption" vs "CategoryOptions") returns null silently
     *
     *   GOOD — model data belongs in the ViewModel:
     *     vm.CategoryOptions = GetCategoryOptions();
     *     return View(vm); // @model ProductFormViewModel — typed, IntelliSense, compiler-checked
     *
     *   BAD — mixing @model with ViewBag for parallel model data:
     *     ViewBag.RelatedProducts = GetRelated(id);
     *     return View(productVm);  // some data typed, some dynamic — inconsistent contract
     *
     *   GOOD — composite ViewModel carries everything:
     *     productVm.RelatedProducts = GetRelated(id).Select(MapToSummary).ToList();
     *     return View(productVm);  // one @model, all data typed and compiler-verified
     *
     * RULE: If data goes to the view, it goes in the ViewModel. ViewData["Title"]
     * is the only well-established exception, and only for layout metadata.
     */
    // (No runnable code for this section — the teaching is in the comment above.
    //  Every action above demonstrates the correct approach, making the anti-pattern
    //  visible by contrast.)
}
