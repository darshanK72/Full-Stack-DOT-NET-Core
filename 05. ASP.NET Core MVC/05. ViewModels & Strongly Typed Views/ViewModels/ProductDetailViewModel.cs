/*
 * FILE ROLE: Composite ViewModel for the Products/Detail page. Combines product
 *            details, an embedded category summary, and a list of related products
 *            in a single typed object — demonstrating composite and nested VMs.
 * SECTIONS IN THIS FILE:
 *  10. Composite ViewModels — why and when to combine entities
 *  11. CategorySummaryViewModel — nested VM component
 *  12. ProductDetailViewModel — composite VM with nested + collection
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.ViewModels;

/*
 * SECTION 10: COMPOSITE VIEWMODELS — COMBINING MULTIPLE ENTITIES
 * ─────────────────────────────────────────────────────────────────────────────
 * A detail page typically shows data from more than one domain entity. Instead
 * of passing multiple entities to the view (or using ViewBag for the extras),
 * a COMPOSITE VIEWMODEL merges everything into one typed object.
 *
 * EXAMPLES:
 *   ProductDetailViewModel   — product + its category + related products
 *   OrderSummaryViewModel    — order + customer + line items
 *   DashboardViewModel       — stats + recent orders + low-stock alerts
 *
 * DESIGN PRINCIPLES:
 *
 *   1. Shape for the view, not for the domain.
 *      Include only what the detail page renders. A product detail page may
 *      show CategoryName but not CategoryId, InternalCost, or CreatedBy.
 *
 *   2. Nest ViewModels for sub-sections.
 *      CategorySummaryViewModel is a separate class used as a property on
 *      ProductDetailViewModel. This keeps each sub-section reusable and testable.
 *
 *   3. Initialize collection properties.
 *      RelatedProducts = new List<ProductSummaryViewModel>() avoids null
 *      reference exceptions in the view's foreach loop.
 *
 *   4. No navigation properties.
 *      CategorySummaryViewModel is NOT Category with extra attributes. It is a
 *      new class with only the fields the view needs, preventing lazy-load calls.
 *
 * NESTED VIEWMODEL PATTERN:
 *   public class OrderViewModel
 *   {
 *       public CustomerSummaryViewModel Customer { get; set; } = new();  // nested
 *       public IEnumerable<LineItemViewModel> Lines { get; set; } = …;   // collection
 *   }
 *   → View: @Model.Customer.Name / @foreach (var line in Model.Lines)
 */

/*
 * SECTION 11: CategorySummaryViewModel — NESTED VM COMPONENT
 * ─────────────────────────────────────────────────────────────────────────────
 * This is a "sub-ViewModel" used as a property on ProductDetailViewModel.
 * It carries only the category fields the detail page displays — not the full
 * Category domain entity and not its Products collection.
 *
 * Reuse: CategorySummaryViewModel is also used in DashboardViewModel (Section 17)
 * and the controller's Dashboard action (Section 23). Defining it here in
 * ViewModels.ViewModels means it is available to all files in the same namespace.
 *
 * = new() initializer on the property in ProductDetailViewModel guarantees the
 * nested object is never null, so the view can safely write @Model.Category.Name
 * without a null check.
 */
public class CategorySummaryViewModel
{
    [Display(Name = "Category")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Category Description")]
    public string? Description { get; set; }       // nullable: category may lack a description

    [Display(Name = "Total Products")]
    public int ProductCount { get; set; }
}

/*
 * SECTION 12: ProductDetailViewModel — COMPOSITE VM
 * ─────────────────────────────────────────────────────────────────────────────
 * This single ViewModel carries everything the detail page needs:
 *
 *   Product fields            (Name, Price, Stock, Active, CreatedAt)
 *   Nested CategorySummaryVM  (Name, Description, ProductCount)
 *   Related products list     (IEnumerable<ProductSummaryViewModel>)
 *
 * The controller (Section 21) populates all three groups before calling View(vm).
 *
 * [DisplayFormat] on CreatedAt:
 *   DataFormatString = "{0:d}"  → short date (e.g. "1/10/2024" en-US)
 *   ApplyFormatInEditMode = false → raw DateTime in edit form; formatted in display
 *
 * [DisplayFormat] on Price:
 *   DataFormatString = "{0:C}"  → currency (e.g. "$1,299.99" en-US)
 *
 * NULLABLE vs NON-NULLABLE PROPERTIES:
 *   string Name — non-nullable (every product has a name)
 *   string? Description — nullable (optional; view uses ?? to show fallback text)
 *   CategorySummaryViewModel Category = new() — non-null (always populated by controller)
 *   IEnumerable<ProductSummaryViewModel> RelatedProducts — non-null (may be empty)
 */
public class ProductDetailViewModel
{
    public int ProductId { get; set; }

    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }        // nullable: shown as "No description" in view

    [Display(Name = "Unit Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }

    [Display(Name = "Stock Quantity")]
    [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
    public int StockQuantity { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Listed On")]
    [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
    public DateTime CreatedAt { get; set; }

    // Nested ViewModel — the controller pre-fetches the Category and maps it here
    public CategorySummaryViewModel Category { get; set; } = new();

    // Related products in the same category — may be empty, never null
    public IEnumerable<ProductSummaryViewModel> RelatedProducts { get; set; }
        = new List<ProductSummaryViewModel>();
}
