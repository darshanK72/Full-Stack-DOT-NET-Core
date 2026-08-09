using System;
using Microsoft.EntityFrameworkCore;
using RawSqlAndStoredProcedures.Data;

namespace RawSqlAndStoredProcedures.Repositories;

/*
 * FILE ROLE: Database.ExecuteSqlRaw and ExecuteSqlInterpolated — non-query commands (UPDATE,
 *            INSERT, DELETE, EXEC) without materializing entities.
 *
 * SECTIONS IN THIS FILE:
 *   7. ExecuteSqlRaw and ExecuteSqlInterpolated
 */

/*
 * SECTION 7: ExecuteSqlRaw AND ExecuteSqlInterpolated
 *
 * Returns the number of rows affected (same as SqlCommand.ExecuteNonQuery in ADO.NET ch.03,
 * or Dapper Execute in ch.02). Does NOT return entity instances — use FromSqlRaw or
 * SqlQuery for SELECT mappings.
 *
 *   | Layer        | Non-query API              | Returns
 *   |--------------|----------------------------|----------
 *   | ADO.NET      | cmd.ExecuteNonQuery()      | int rows
 *   | Dapper       | connection.Execute(sql)    | int rows
 *   | EF Core      | context.Database.ExecuteSqlRaw | int rows
 *
 * ExecuteSqlRawAsync exists for async I/O — COVERED IN DETAIL LATER → ch.11.
 *
 * Parameter rules match FromSqlRaw: use {0} placeholders or ExecuteSqlInterpolated.
 *
 * Pitfall: ExecuteSqlRaw bypasses change tracker — already-loaded Product entities in the
 * same DbContext may be stale until you reload or use a fresh context.
 *
 * Pitfall: ExecuteSqlRaw on procs with SET NOCOUNT ON often returns -1 (not rows affected).
 *          Same behavior as ADO.NET ExecuteNonQuery with NOCOUNT ON (ch.07).
 */
public sealed class RawSqlExecuteRepository
{
    private readonly EfCoreTutorialDbContext _context;

    public RawSqlExecuteRepository(EfCoreTutorialDbContext context)
    {
        _context = context;
    }

    public int RestockProduct(int productId, int additionalUnits)
    {
        return _context.Database.ExecuteSqlRaw(
            """
            UPDATE dbo.Products
            SET StockQuantity = StockQuantity + {0}
            WHERE ProductId = {1}
            """,
            additionalUnits,
            productId);
    }

    public int ApplyBulkDiscountViaStoredProcedure(string categoryPrefix, decimal discountPercent)
    {
        // EXEC with parameters — same pattern as ADO.NET CommandType.StoredProcedure (ch.07)
        return _context.Database.ExecuteSqlRaw(
            "EXEC dbo.usp_ApplyBulkDiscount {0}, {1}",
            categoryPrefix,
            discountPercent);
    }

    public int MarkDiscontinued(int productId, DateTime discontinuedOn)
    {
        FormattableString sql = $"""
            UPDATE dbo.Products
            SET DiscontinuedDate = {discontinuedOn.Date}
            WHERE ProductId = {productId}
              AND DiscontinuedDate IS NULL
            """;

        return _context.Database.ExecuteSqlInterpolated(sql);
    }
}
