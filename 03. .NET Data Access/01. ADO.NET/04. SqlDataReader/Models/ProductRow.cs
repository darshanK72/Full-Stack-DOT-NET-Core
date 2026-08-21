using System;

namespace SqlDataReader;

/*
 * FILE ROLE: Plain CLR object (POCO) that represents one Products row after manual mapping
 *            from SqlDataReader — the pattern Dapper and EF Core automate.
 *
 * SECTIONS IN THIS FILE:
 *   2. Domain type for manual mapping
 */

/*
 * =========================================================================
 * SECTION 2: DOMAIN TYPE FOR MANUAL MAPPING
 * =========================================================================
 *
 * SqlDataReader returns untyped columns. Production code maps each row to
 * a POCO (plain old CLR object). Dapper automates this; here you see the
 * manual pattern every ORM wraps.
 * -------------------------------------------------------------------------
 */
public sealed class ProductRow
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int StockQuantity { get; init; }
    public DateTime? DiscontinuedDate { get; init; }

    public override string ToString()
    {
        string discontinued = DiscontinuedDate.HasValue
            ? DiscontinuedDate.Value.ToString("yyyy-MM-dd")
            : "(active)";
        return $"{ProductId,2} | {ProductName,-10} | {UnitPrice,6:F2} | stock {StockQuantity,3} | {discontinued}";
    }
}
