using System;

namespace IntroductionToDapper.Models;

/*
 * FILE ROLE: Plain CLR object (POCO) that Dapper maps from dbo.Products query rows.
 *
 * SECTIONS IN THIS FILE:
 *   7. Column-to-property mapping by convention
 */

/*
 * =========================================================================
 * SECTION 7: COLUMN-TO-PROPERTY MAPPING BY CONVENTION
 * =========================================================================
 *
 * Dapper maps each result column to a public property or field on T when
 * names match (case-insensitive by default):
 *
 *   SQL column (AdoNetTutorial) | Product property
 *   ---------------------------|------------------
 *   ProductId                   | ProductId
 *   ProductName                 | ProductName
 *   UnitPrice                   | UnitPrice
 *   StockQuantity               | StockQuantity
 *   DiscontinuedDate            | DiscontinuedDate
 *
 * Requirements for automatic mapping:
 *   - Parameterless constructor (or a constructor Dapper can use)
 *   - Public setters or init-only properties on mapped members
 *   - Column names align with property names (underscore rules deferred -> ch04)
 *
 * Compare to ADO.NET ch04 SqlDataReader: you manually call GetInt32,
 * GetString, and IsDBNull inside a Read() loop. Dapper performs that mapping
 * in one Query<T> call.
 *
 * When names differ (alias in SQL vs property name), use AS in SQL or custom
 * mapping - COVERED IN DETAIL LATER -> 04. Mapping, Multi-Mapping & Advanced Patterns.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int StockQuantity { get; init; }
    public DateTime? DiscontinuedDate { get; init; }

    public override string ToString()
    {
        string status = DiscontinuedDate is null ? "available" : $"discontinued {DiscontinuedDate:yyyy-MM-dd}";
        return $"{ProductId,2} | {ProductName,-12} | ${UnitPrice,7:F2} | stock {StockQuantity,3} | {status}";
    }
}
