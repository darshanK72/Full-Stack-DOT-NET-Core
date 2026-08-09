using System;

namespace IntroductionToEntityFrameworkCore.Models;

/*
 * FILE ROLE: Plain entity class that EF Core maps to a database table (or an
 *            In-Memory store in this chapter's preview demo).
 *
 * SECTIONS IN THIS FILE:
 *   7. Entity classes - POCOs EF Core maps to tables
 */

/*
 * =========================================================================
 * SECTION 7: ENTITY CLASSES - POCOs EF CORE MAPS TO TABLES
 * =========================================================================
 *
 * An entity is a .NET class whose instances represent rows in a table.
 * EF Core discovers entities through DbSet<T> properties on your DbContext
 * (see Data/AppDbContext.cs - preview only; full depth -> ch02 DbContext & DbSet).
 *
 * Minimum shape for this intro chapter:
 *
 *   - Public properties for columns you want persisted
 *   - Parameterless constructor (implicit or explicit)
 *   - Primary key property (ProductId) - EF Core convention: Id or {TypeName}Id
 *
 * Compare to Dapper ch01 Models/Product.cs:
 *
 *   | Concern              | Dapper POCO              | EF Core entity
 *   |----------------------|--------------------------|----------------------------------
 *   | Who creates SQL      | You write SELECT/INSERT  | EF Core generates SQL from LINQ
 *   | Mapping              | Column name -> property  | Same properties + change tracker
 *   | Schema ownership     | DBA / your scripts       | Code-First migrations or existing DB
 *
 * Data annotations ([Required], [MaxLength]) and Fluent API configuration ->
 * ch07 Fluent API & Data Annotations.
 * Relationships and navigation properties -> ch06.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public DateTime? DiscontinuedDate { get; set; }

    public override string ToString()
    {
        string status = DiscontinuedDate is null ? "available" : $"discontinued {DiscontinuedDate:yyyy-MM-dd}";
        return $"{ProductId,2} | {ProductName,-12} | ${UnitPrice,7:F2} | stock {StockQuantity,3} | {status}";
    }
}
