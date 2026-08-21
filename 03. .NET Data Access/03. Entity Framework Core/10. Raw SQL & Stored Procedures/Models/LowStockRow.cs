namespace RawSqlAndStoredProcedures.Models;

/*
 * FILE ROLE: Narrow report row returned by a stored procedure — demonstrates SqlQueryRaw<T>
 *            with parameters on a shape that is not Product.
 *
 * SECTIONS IN THIS FILE:
 *   3. LowStockRow — stored-procedure report via SqlQuery
 */

/*
 * SECTION 3: LowStockRow — STORED-PROCEDURE REPORT VIA SqlQuery
 *
 * usp_GetLowStockReport returns ProductId, ProductName, StockQuantity, and a computed
 * Status column. This is a read-only projection — not a table-backed entity.
 *
 * In Dapper you would write:
 *   connection.Query<LowStockRow>("dbo.usp_GetLowStockReport", new { Threshold = 50 },
 *       commandType: CommandType.StoredProcedure);
 *
 * In ADO.NET you would SqlDataReader.Read() and assign fields manually (ch.04).
 *
 * EF Core 8 equivalent:
 *   context.Database.SqlQueryRaw<LowStockRow>(
 *       "EXEC dbo.usp_GetLowStockReport @Threshold", threshold);
 */
public sealed class LowStockRow
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
}
