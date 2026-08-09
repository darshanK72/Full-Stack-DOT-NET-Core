using System;
using SqlClientReader = Microsoft.Data.SqlClient.SqlDataReader;

namespace SqlDataReader;

/*
 * FILE ROLE: Maps one SqlDataReader row to ProductRow using cached ordinals, typed accessors,
 *            and IsDBNull for nullable columns.
 *
 * SECTIONS IN THIS FILE:
 *   3. Manual row mapper — typed accessors + IsDBNull
 */

/*
 * =========================================================================
 * SECTION 3: MANUAL ROW MAPPER — TYPED ACCESSORS + IsDBNull
 * =========================================================================
 *
 * Best practice inside a hot Read() loop:
 *   1. Resolve ordinals once with GetOrdinal (by column name)
 *   2. Read values with GetInt32/GetString/GetDecimal on those ordinals
 *   3. Call IsDBNull before nullable columns — never call GetDateTime on NULL
 *
 * Indexer reader["col"] boxes to object — fine for ad-hoc reads; prefer typed
 * accessors in performance-sensitive mapping.
 * -------------------------------------------------------------------------
 */
public static class ProductMapper
{
    public static ProductRow MapFromReader(SqlClientReader reader)
    {
        int ordId = reader.GetOrdinal("ProductId");
        int ordName = reader.GetOrdinal("ProductName");
        int ordPrice = reader.GetOrdinal("UnitPrice");
        int ordStock = reader.GetOrdinal("StockQuantity");
        int ordDisc = reader.GetOrdinal("DiscontinuedDate");

        DateTime? discontinued = reader.IsDBNull(ordDisc)
            ? null
            : reader.GetDateTime(ordDisc);

        return new ProductRow
        {
            ProductId = reader.GetInt32(ordId),
            ProductName = reader.GetString(ordName),
            UnitPrice = reader.GetDecimal(ordPrice),
            StockQuantity = reader.GetInt32(ordStock),
            DiscontinuedDate = discontinued
        };
    }
}
