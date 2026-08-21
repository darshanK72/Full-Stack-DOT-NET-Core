using System.Collections.Generic;

namespace LinqToEntitiesAndQueryPatterns.Models;

/*
 * FILE ROLE:
 *   Category entity for one-to-many Product relationship and filtering demos.
 *
 * SECTIONS IN THIS FILE:
 *   1. Category entity
 */

/*
 * SECTION 1: CATEGORY ENTITY
 *
 * Parent in one-to-many: Category (1) -> Products (many).
 * CategoryId is the PK; Products hold CategoryId as FK.
 *
 * Navigation collection Products is used in projection demos (category name + product list).
 * Full relationship configuration lives in Data/StoreDbContext.cs (Fluent API from ch.06-07).
 * -------------------------------------------------------------------------
 */
public sealed class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>(); // one-to-many nav
}
