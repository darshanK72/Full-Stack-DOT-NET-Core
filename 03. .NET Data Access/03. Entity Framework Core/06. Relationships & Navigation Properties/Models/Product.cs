using System.Collections.Generic;

namespace RelationshipsAndNavigationProperties.Models;

/*
 * FILE ROLE:
 *   Catalog product entity used in the many-to-many join demo (SECTION 5).
 *
 * SECTIONS IN THIS FILE:
 *   5. Many-to-many with explicit join entity  -  Product side
 */

/*
 * SECTION 5: MANY-TO-MANY WITH EXPLICIT JOIN ENTITY  -  PRODUCT SIDE
 *
 * Product and Tag have a many-to-many relationship:
 *
 *   Product (many) ----< ProductTag >---- (many) Tag
 *
 * ProductTag is the join entity (link table with payload  -  AddedOn date).
 * Each Product exposes ProductTags (collection of join rows), not Tags directly.
 *
 * Why a join entity instead of ICollection<Tag> Tags?
 *   - Extra columns on the link (AddedOn, SortOrder, AssignedBy, ...)
 *   - Explicit control over the join table name and keys
 *
 * EF Core 5+ also supports skip navigations (Product.Tags without ProductTag class)
 * when the link table has no payload  -  preview note in Data/RelationshipsDbContext.cs.
 *
 * Table name CatalogProducts (configured in OnModelCreating) keeps this chapter isolated
 * from dbo.Products potentially created by ch02/ch03 on the same EfCoreTutorial database.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>(); // join-entity collection  -  one row per product-tag link

    public override string ToString() => $"{ProductId}: {Name} @ ${UnitPrice:F2}";
}
