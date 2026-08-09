using System;

namespace RelationshipsAndNavigationProperties.Models;

/*
 * FILE ROLE:
 *   Explicit join entity for Product-Tag many-to-many with payload (SECTION 5).
 *
 * SECTIONS IN THIS FILE:
 *   5. Join entity  -  composite key, FK scalars, payload column
 */

/*
 * SECTION 5 (continued): JOIN ENTITY  -  COMPOSITE KEY AND PAYLOAD
 *
 * ProductTag maps to the link table dbo.ProductTags:
 *
 *   ProductId | TagId | AddedOn
 *   ----------|-------|--------
 *   (FK)      | (FK)  | payload column  -  why we use an entity instead of skip navigation
 *
 * Composite primary key: (ProductId, TagId)  -  configured in OnModelCreating.
 * Each FK scalar pairs with a reference navigation (Product, Tag).
 *
 * Insert patterns:
 *   1. Through Product: product.ProductTags.Add(new ProductTag { Tag = tag, AddedOn = ... });
 *   2. Direct: context.ProductTags.Add(new ProductTag { ProductId = 1, TagId = 2, ... });
 *
 * Pitfall: duplicate (ProductId, TagId) pairs violate the composite PK on SaveChanges.
 * -------------------------------------------------------------------------
 */
public sealed class ProductTag
{
    public int ProductId { get; set; }
    public int TagId { get; set; }
    public DateOnly AddedOn { get; set; }           // payload  -  reason for explicit join entity
    public Product Product { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
