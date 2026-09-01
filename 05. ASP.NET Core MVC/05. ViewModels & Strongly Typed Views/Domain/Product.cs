/*
 * FILE ROLE: Domain entity representing a product as stored in the database /
 *            returned by the repository layer. Teaches WHY this shape must not
 *            be passed directly to views and how it differs from a ViewModel.
 * SECTIONS IN THIS FILE:
 *   4. Domain Entity vs ViewModel — motivation and comparison table
 *   5. Product domain entity — the shape that comes from persistence
 */

using System;

namespace ViewModels.Domain;

/*
 * SECTION 4: DOMAIN ENTITY VS VIEWMODEL — WHY THE SPLIT
 * ─────────────────────────────────────────────────────────────────────────────
 * A domain entity is shaped for the DATABASE and business rules, not for a view:
 *
 *   Concern                  Domain Entity          ViewModel
 *   ───────────────────────  ─────────────────────  ──────────────────────────────
 *   Navigation properties    Yes (Category, Orders)  No — avoided (lazy-load risk)
 *   Internal audit fields    Yes (CreatedBy, RowVer)  No — hidden from the view
 *   Display labels           No                      Yes — [Display(Name = "...")]
 *   Display formatting       No                      Yes — [DisplayFormat(...)]
 *   Validation attributes    Sometimes               Yes — [Required], [Range], etc.
 *   Dropdown data            No                      Yes — CategoryOptions list
 *   Computed display props   No                      Yes — FullDescription, BadgeText
 *   ORM change-tracking      Yes (EF Core tracked)   No — plain POCO, not tracked
 *   Shape                    One per table/aggregate  One per view/page/action
 *
 * WHAT GOES WRONG WHEN YOU PASS A DOMAIN ENTITY DIRECTLY TO A VIEW:
 *
 *   1. OVER-POSTING / MASS ASSIGNMENT
 *      If Product is the @model for a form, a malicious POST could set
 *      IsDeleted, InternalCost, or other sensitive fields the form never showed.
 *      A ViewModel exposes ONLY the fields the form intentionally edits.
 *
 *   2. LAZY-LOAD N+1 IN THE VIEW
 *      product.Category.Name in a view triggers a lazy-load database call per
 *      row. A ViewModel receives the pre-fetched string CategoryName — no ORM.
 *
 *   3. SERIALIZATION CYCLES
 *      Product.Category.Products.Category... — circular navigation properties
 *      cause stack overflows in JSON serializers and view rendering.
 *
 *   4. TIGHT VIEW-DOMAIN COUPLING
 *      Renaming a domain property breaks the view. With a ViewModel, the mapping
 *      in the controller absorbs the rename; the view stays untouched.
 *
 *   5. MISSING DISPLAY METADATA
 *      Domain entities rarely carry [Display] / [DisplayFormat] attributes
 *      because those are UI concerns. ViewModels are the right place for them.
 *
 * RULE: Controllers receive domain entities from the service/repository layer,
 * MAP them to ViewModels, and pass the ViewModel to the view — never the entity.
 */

/*
 * SECTION 5: PRODUCT DOMAIN ENTITY
 * ─────────────────────────────────────────────────────────────────────────────
 * This is what a repository or EF Core DbContext returns. Notice what is here
 * that a view should NOT receive directly:
 *   - CategoryId (foreign key — expose Category.Name in the VM instead)
 *   - Category (navigation property — triggers lazy-load if not pre-fetched)
 *   - CreatedAt (raw timestamp — formatted differently per view: "3 days ago" vs "2024-01-10")
 *   - InternalCost (internal field — never exposed to the view at all)
 *
 * The controller's mapping method cherry-picks from here and shapes the ViewModel.
 */
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // non-nullable: every product has a name

    // Navigation property — do NOT pass to views; fetch Category.Name in the controller instead
    public string? Description { get; set; }         // nullable: description is optional

    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foreign key — kept in the entity for persistence; mapped to a name string in the VM
    public int CategoryId { get; set; }

    // Navigation property — loaded by the repository; the VM carries only what the view needs
    public Category? Category { get; set; }

    // Internal field — never exposed via a ViewModel; filtered out during mapping
    public decimal InternalCost { get; set; }
}
