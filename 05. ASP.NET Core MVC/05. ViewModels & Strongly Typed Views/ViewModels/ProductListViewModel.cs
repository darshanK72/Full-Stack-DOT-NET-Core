/*
 * FILE ROLE: List ViewModels for the Products/Index page — a per-row summary VM
 *            (ProductSummaryViewModel) and the page-level list VM that wraps the
 *            collection and carries filter state. Demonstrates IEnumerable<T> as
 *            the @model type and ViewModel naming conventions.
 * SECTIONS IN THIS FILE:
 *   7. ViewModels vs DTOs vs domain entities (comparison)
 *   8. ProductSummaryViewModel — per-row item with [Display] and [DisplayFormat]
 *   9. ProductListViewModel — IEnumerable<T> wrapper with filter state
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.ViewModels;

/*
 * SECTION 7: VIEWMODELS vs DTOs vs DOMAIN ENTITIES
 * ─────────────────────────────────────────────────────────────────────────────
 * These three types look similar (plain C# classes) but serve different roles:
 *
 *   Type            Purpose                       Lives In            Has
 *   ─────────────── ─────────────────────────────  ─────────────────  ────────────────────────
 *   Domain Entity   Persistence / business logic  Domain/ or Models/  Nav props, ORM tracking
 *   DTO             Data transfer over the wire   DTOs/ or Contracts/ JSON-friendly; no logic
 *   ViewModel       Shape for ONE specific view   ViewModels/         [Display], composite data
 *
 * WHEN TO USE WHICH:
 *
 *   Use a domain entity when:
 *     - Inside the service/repository layer passing between business methods.
 *     - EF Core tracking changes before SaveChanges().
 *
 *   Use a DTO when:
 *     - Sending data over an API boundary (Minimal API, Web API controller).
 *     - Deserializing incoming JSON request bodies.
 *     - Projecting from a domain entity for a REST response.
 *
 *   Use a ViewModel when:
 *     - Passing data from a controller action to a Razor view.
 *     - The view needs display metadata ([Display], [DisplayFormat]).
 *     - The view combines data from multiple entities (composite VM).
 *     - A form page needs input fields + dropdown options in one object.
 *
 * OVERLAP:
 *   ViewModels and DTOs are both plain classes — the distinction is PURPOSE.
 *   A ViewModel is for Razor views; a DTO is for serialization / wire transfer.
 *   In small applications these sometimes merge; in larger codebases keeping
 *   them separate prevents view concerns from polluting API contracts.
 *
 * VIEWMODEL NAMING CONVENTIONS:
 *   {Feature}ViewModel       ProductListViewModel     — feature / page name (most common)
 *   {Action}ViewModel        IndexViewModel           — action-specific
 *   {Controller}ViewModel    ProductsViewModel        — controller-wide shared VM
 *
 *   All of the above are valid. Pick one convention and apply it consistently.
 *   This chapter uses {Feature}ViewModel throughout.
 */

/*
 * SECTION 8: ProductSummaryViewModel — PER-ROW ITEM VM
 * ─────────────────────────────────────────────────────────────────────────────
 * In a list view, each row is a small "summary" of the full entity. The summary
 * ViewModel exposes only the columns the table needs and adds [Display] /
 * [DisplayFormat] metadata so Razor helpers render labels and values correctly.
 *
 * [Display(Name = "...")] — used by Html.DisplayNameFor and Html.LabelFor
 *   to render the column header or form label from an expression:
 *     @Html.DisplayNameFor(m => m.Products.FirstOrDefault()!.Price)
 *     → renders "Unit Price"
 *
 * [DisplayFormat(DataFormatString = "{0:C}")] — used by Html.DisplayFor
 *   to format the value using the specified format string before rendering:
 *     @Html.DisplayFor(modelItem => item.Price)
 *     → renders "$1,299.99" (culture-aware currency format)
 *
 * ApplyFormatInEditMode = true — extends the format to EditorFor; false by
 *   default because raw numbers are easier to edit than formatted strings.
 *
 * NULLABLE REFERENCE TYPE PATTERN:
 *   string Name { get; set; } = string.Empty;   // non-null; = init value silences CS8618
 *   string? CategoryName { get; set; }          // explicitly nullable (may be unknown)
 *
 * The controller fills every property before passing to the view, so in practice
 * CategoryName will never be null — but declaring it nullable signals that
 * absence is theoretically possible and the view must handle it (e.g., with ??)
 */
public class ProductSummaryViewModel
{
    public int Id { get; set; } // passed to asp-route-id for action links

    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public string? CategoryName { get; set; } // nullable: category may not be loaded

    [Display(Name = "Unit Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }

    [Display(Name = "In Stock")]
    public bool IsActive { get; set; }
}

/*
 * SECTION 9: ProductListViewModel — IEnumerable<T> WRAPPER WITH FILTER STATE
 * ─────────────────────────────────────────────────────────────────────────────
 * A LIST VIEWMODEL wraps an IEnumerable<RowViewModel> plus any page-level data
 * the view needs: filter values, totals, pagination state, dropdown options.
 *
 * WHY WRAP INSTEAD OF USING IEnumerable<T> DIRECTLY AS @model?
 *
 *   Option A: @model IEnumerable<ProductSummaryViewModel>
 *     Pro:  Simple, no wrapper class required.
 *     Con:  Cannot carry filter state, total count, or page metadata alongside
 *           the list. ViewBag would be needed for the extras — which is an
 *           anti-pattern when @model already handles the data contract.
 *
 *   Option B: @model ProductListViewModel  (this class)
 *     Pro:  All view data in one typed object: list + filters + totals.
 *           Strong typing for everything; no ViewBag workarounds.
 *     Con:  One extra class per list page.
 *
 * In real applications Option B scales far better; use Option A only for the
 * simplest read-only lists with no filter or pagination requirements.
 *
 * IEnumerable<T> vs IList<T> vs List<T> in ViewModels:
 *   IEnumerable<T>  — widest interface; sufficient for foreach in the view.
 *   IList<T>        — adds Count and index access; use if the view needs Count.
 *   List<T>         — concrete; fine, but IEnumerable<T> is more testable.
 *
 * Initialization: = new List<ProductSummaryViewModel>() ensures the property is
 * never null — avoids null checks in the view's foreach loop.
 *
 * FILTER STATE in the ViewModel:
 *   Returning filter values to the view lets it pre-populate the filter form,
 *   so the user sees what search they just ran. The controller fills these from
 *   the incoming query string / form values before passing the VM to the view.
 *
 * TotalCount: the controller sets this to the count BEFORE pagination so the
 *   view can display "Showing 1–20 of 47 results" without re-querying.
 */
public class ProductListViewModel
{
    // The collection of per-row VMs — the main content of the list view
    public IEnumerable<ProductSummaryViewModel> Products { get; set; }
        = new List<ProductSummaryViewModel>();

    // Filter state — returned to the view so the search form shows current values
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Category")]
    public int? CategoryFilter { get; set; }

    // Page metadata
    [Display(Name = "Total Products")]
    public int TotalCount { get; set; }

    // Category dropdown options — filled by controller; NOT domain Category entities
    public IEnumerable<CategoryOption> CategoryOptions { get; set; }
        = new List<CategoryOption>();
}
