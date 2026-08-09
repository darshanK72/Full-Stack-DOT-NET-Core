namespace RawSqlAndStoredProcedures.Models;

/*
 * FILE ROLE: Keyless result shape for Database.SqlQuery<T> — aggregate columns that do not
 *            map to a tracked entity or DbSet.
 *
 * SECTIONS IN THIS FILE:
 *   2. InventorySummary — SqlQuery result type (EF Core 8)
 */

/*
 * SECTION 2: InventorySummary — SqlQuery RESULT TYPE (EF CORE 8)
 *
 * EF Core 8 adds Database.SqlQuery<T> / SqlQueryRaw<T> for arbitrary SELECT shapes that are
 * NOT full entities. Before EF Core 8 you often used:
 *   • ADO.NET SqlDataReader + manual mapping (ch.04)
 *   • Dapper Query<T> on inline SQL (ch.01–02)
 *   • Keyless entity types registered in OnModelCreating (still valid — preview below)
 *
 * SqlQuery<T> requirements:
 *   • Public parameterless constructor (or EF cannot materialize)
 *   • Property names match result column names (case-insensitive)
 *   • Type is NOT added to DbSet<T> — no change tracking, no INSERT/UPDATE through EF
 *
 * Compare to FromSqlRaw:
 *   | API                    | Target type        | DbSet required | Change tracking
 *   |------------------------|--------------------|----------------|------------------
 *   | DbSet.FromSqlRaw       | Mapped entity      | Yes            | Yes (default)
 *   | Database.SqlQuery<T>   | Any POCO / record  | No             | No
 *
 * COVERED IN DETAIL LATER → 11. Change Tracking, Async & Transactions (AsNoTracking).
 */
public sealed class InventorySummary
{
    public int ProductCount { get; set; }
    public int TotalUnits { get; set; }
    public decimal AveragePrice { get; set; }
}
