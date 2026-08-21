using System.Collections.Generic;

namespace RelationshipsAndNavigationProperties.Models;

/*
 * FILE ROLE:
 *   Tag entity  -  other end of the many-to-many relationship through ProductTag (SECTION 5).
 *
 * SECTIONS IN THIS FILE:
 *   5. Many-to-many with explicit join entity  -  Tag side
 */

/*
 * SECTION 5 (continued): MANY-TO-MANY  -  TAG SIDE
 *
 * Tag mirrors Product: it navigates to ProductTag rows, not directly to Product instances.
 * Queries that need "all tags for a product" go through ProductTags then .Tag, or use
 * SelectMany after Include  -  full eager-loading patterns -> ch09 Loading Related Data.
 * -------------------------------------------------------------------------
 */
public sealed class Tag
{
    public int TagId { get; set; }
    public string Label { get; set; } = string.Empty;
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

    public override string ToString() => $"Tag {TagId}: {Label}";
}
