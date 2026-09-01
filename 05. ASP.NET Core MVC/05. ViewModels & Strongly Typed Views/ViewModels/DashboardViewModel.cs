/*
 * FILE ROLE: Multi-entity composite ViewModel for a dashboard page. Shows how
 *            to compose statistics, recent items, and category summaries from
 *            multiple unrelated domain entities into a single typed VM.
 * SECTIONS IN THIS FILE:
 *  16. Multi-entity composite ViewModels — motivation and structure
 *  17. DashboardStats — nested statistics VM
 *  18. DashboardViewModel — composing unrelated entities into one page VM
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.ViewModels;

/*
 * SECTION 16: MULTI-ENTITY COMPOSITE VIEWMODELS
 * ─────────────────────────────────────────────────────────────────────────────
 * A dashboard page typically aggregates data from multiple domain entities that
 * have no natural parent-child relationship. Without a composite ViewModel, a
 * controller action would need multiple ViewBag entries or multiple ViewData
 * keys — all stringly-typed and invisible to the Razor compiler.
 *
 * ANTI-PATTERN — multiple ViewBag entries:
 *   ViewBag.TotalProducts  = repo.CountProducts();
 *   ViewBag.RecentProducts = repo.GetRecent(5);
 *   ViewBag.TopCategories  = repo.GetTopCategories(3);
 *   return View();   // @model is null — no strong typing at all
 *
 *   Problem: the view must cast every object (dynamic); misspelled property
 *   names or wrong cast types cause runtime exceptions, not compile errors.
 *
 * CORRECT PATTERN — composite ViewModel:
 *   var vm = new DashboardViewModel
 *   {
 *       Stats = new DashboardStats { TotalProducts = ..., ... },
 *       RecentProducts = products.Select(MapToSummary).ToList(),
 *       TopCategories  = categories.Select(MapToCategorySummary).ToList(),
 *   };
 *   return View(vm);   // @model DashboardViewModel — fully typed
 *
 *   Benefit: every property access in the view (@Model.Stats.TotalProducts)
 *   is verified by the Razor compiler and navigable via IntelliSense.
 *
 * STRUCTURE RULE:
 *   - Aggregate stats into a nested sub-VM (DashboardStats) — keeps the top
 *     level uncluttered and lets you pass DashboardStats alone to a partial view.
 *   - Reuse existing sub-VMs (ProductSummaryViewModel, CategorySummaryViewModel)
 *     rather than creating new duplicates. The same sub-VM can appear in multiple
 *     composite VMs without modification.
 */

/*
 * SECTION 17: DashboardStats — NESTED STATISTICS VM
 * ─────────────────────────────────────────────────────────────────────────────
 * Grouping related statistics into a nested class has two benefits:
 *   1. The dashboard view's stat cards section can receive just the Stats object
 *      (as a partial view parameter), keeping partial views self-contained.
 *   2. Adding a new stat field does not change the DashboardViewModel signature
 *      — only DashboardStats grows.
 *
 * [DisplayFormat(DataFormatString = "{0:C}")] on AveragePrice means that any
 * call to Html.DisplayFor(m => m.Stats.AveragePrice) renders the currency-
 * formatted value automatically, without the view hard-coding the format string.
 */
public class DashboardStats
{
    [Display(Name = "Total Products")]
    public int TotalProducts { get; set; }

    [Display(Name = "Active Products")]
    public int ActiveProducts { get; set; }

    [Display(Name = "Out of Stock")]
    public int OutOfStockProducts { get; set; }

    [Display(Name = "Total Categories")]
    public int TotalCategories { get; set; }

    [Display(Name = "Average Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal AveragePrice { get; set; }
}

/*
 * SECTION 18: DashboardViewModel — COMPOSING UNRELATED ENTITIES
 * ─────────────────────────────────────────────────────────────────────────────
 * DashboardViewModel is the @model for the (hypothetical) Dashboard view.
 * It combines three unrelated groups:
 *
 *   Stats            — scalar aggregates (counts, averages)
 *   RecentProducts   — last N products added (IEnumerable<ProductSummaryViewModel>)
 *   TopCategories    — categories sorted by product count (IEnumerable<CategorySummaryViewModel>)
 *
 * Both collection properties reuse sub-VMs defined in other files in this namespace:
 *   ProductSummaryViewModel  — defined in ProductListViewModel.cs (SECTION 8)
 *   CategorySummaryViewModel — defined in ProductDetailViewModel.cs (SECTION 11)
 *
 * This reuse is one of the key benefits of organizing ViewModels by namespace:
 * sub-VMs are shared across composite VMs without duplication.
 *
 * INITIALIZATION:
 *   Stats = new() initializes the nested object so the view can safely call
 *   @Model.Stats.TotalProducts without a null check.
 *
 *   Collection properties are initialized to empty lists so foreach loops in
 *   the view do not require null guards.
 */
public class DashboardViewModel
{
    // Nested stats sub-VM — initialized so @Model.Stats.X is always safe
    public DashboardStats Stats { get; set; } = new DashboardStats();

    // Last 5 products added — reuses the same sub-VM as the list page
    public IEnumerable<ProductSummaryViewModel> RecentProducts { get; set; }
        = new List<ProductSummaryViewModel>();

    // Top categories by product count — reuses the same sub-VM as the detail page
    public IEnumerable<CategorySummaryViewModel> TopCategories { get; set; }
        = new List<CategorySummaryViewModel>();
}
