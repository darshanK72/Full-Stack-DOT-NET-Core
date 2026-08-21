namespace LinqToEntitiesAndQueryPatterns.Models;

/*
 * FILE ROLE:
 *   Read-model DTO for projection demos  -  shape returned by Select, not a mapped table.
 *
 * SECTIONS IN THIS FILE:
 *   1. ProductListItem projection DTO
 */

/*
 * SECTION 1: PRODUCT LIST ITEM PROJECTION DTO
 *
 * Not registered as DbSet<T>  -  created in memory from SQL projection (SELECT columns only).
 * Prefer named DTOs over anonymous types when the result crosses method boundaries or API layers.
 *
 * PREREQUISITE: LINQ ch.08 Projection Operations  -  Select, anonymous types, deferred execution.
 * -------------------------------------------------------------------------
 */
public sealed class ProductListItem
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public bool InStock { get; init; }
}
