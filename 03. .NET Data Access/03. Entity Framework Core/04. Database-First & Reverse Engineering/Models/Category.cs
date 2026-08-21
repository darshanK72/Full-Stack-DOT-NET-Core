using System.Collections.Generic;

namespace DatabaseFirstAndReverseEngineering.Models;

/*
 * FILE ROLE:
 *   Scaffolded-style entity for dbo.Categories (hand-written to match reverse-engineering output).
 *
 * SECTIONS IN THIS FILE:
 *   4. Generated Category entity
 */

/*
 * SECTION 4: GENERATED CATEGORY ENTITY
 *
 * Reverse engineering emits one partial class per table. Typical traits:
 *
 * | Scaffold output              | Meaning                                      |
 * |------------------------------|----------------------------------------------|
 * | partial class                | Allows *.Partial.cs files you own            |
 * | virtual DbSet / navigation   | Lazy-loading hook (off by default in EF Core)|
 * | Nullable reference types     | NULL columns become string? etc.             |
 * | No data annotations          | Fluent config lives in DbContext (section 5) |
 *
 * WARNING: Re-scaffold replaces this file. Do not add custom methods here;
 *   put them in Category.Partial.cs (section 7).
 * -------------------------------------------------------------------------
 */
public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
