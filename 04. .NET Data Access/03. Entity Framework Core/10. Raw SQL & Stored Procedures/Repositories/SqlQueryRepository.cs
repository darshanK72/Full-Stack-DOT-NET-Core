using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RawSqlAndStoredProcedures.Data;
using RawSqlAndStoredProcedures.Models;

namespace RawSqlAndStoredProcedures.Repositories;

/*
 * FILE ROLE: Database.SqlQuery and SqlQueryRaw (EF Core 8) — map arbitrary SELECT shapes to
 *            POCOs without DbSet registration.
 *
 * SECTIONS IN THIS FILE:
 *   9. SqlQuery and SqlQueryRaw (EF Core 8)
 */

/*
 * SECTION 9: SqlQuery AND SqlQueryRaw (EF CORE 8)
 *
 * New in EF Core 8: Database.SqlQuery<T> / SqlQueryRaw<T> replace many ad-hoc uses of
 * keyless entity types and raw ADO.NET for read-only projections.
 *
 *   | Need                              | EF Core API
 *   |-----------------------------------|----------------------------------
 *   | SELECT -> mapped entity + track   | DbSet.FromSqlRaw
 *   | SELECT -> mapped entity, read-only| DbSet.FromSqlRaw.AsNoTracking()
 *   | SELECT -> arbitrary POCO / DTO    | Database.SqlQueryRaw<T>  (EF Core 8+)
 *   | UPDATE/DELETE                     | Database.ExecuteSqlRaw
 *
 * ADO.NET path (ch.04): manual reader loop into InventorySummary fields.
 * Dapper path (ch.01): connection.Query<InventorySummary>("SELECT COUNT(*) ...").
 *
 * SqlQuery returns IQueryable<T> — you can append LINQ when SQL is composable (plain SELECT).
 * EXEC / stored procedures are NOT composable — call AsEnumerable() before client-side LINQ.
 *
 * --- Keyless entity alternative (preview) ---
 * Register modelBuilder.Entity<InventorySummary>().HasNoKey() and use DbSet — still valid
 * when you want the type in the same LINQ graph as entities. SqlQuery is lighter for one-off
 * reports.
 *
 * Requirements for T:
 *   • Public parameterless constructor
 *   • Readable/writable properties matching column names
 *   • Not registered as a tracked entity (no DbSet<InventorySummary>)
 */
public sealed class SqlQueryRepository
{
    private readonly EfCoreTutorialDbContext _context;

    public SqlQueryRepository(EfCoreTutorialDbContext context)
    {
        _context = context;
    }

    public InventorySummary GetInventorySummary()
    {
        // SqlQueryRaw — inline aggregate SELECT into InventorySummary (SECTION 2)
        return _context.Database
            .SqlQueryRaw<InventorySummary>(
                """
                SELECT
                    COUNT(*) AS ProductCount,
                    SUM(StockQuantity) AS TotalUnits,
                    AVG(UnitPrice) AS AveragePrice
                FROM dbo.Products
                WHERE DiscontinuedDate IS NULL
                """)
            .Single();
    }

    public List<LowStockRow> GetLowStockReport(int threshold)
    {
        // EXEC is non-composable SQL — OrderBy must run client-side via AsEnumerable()
        return _context.Database
            .SqlQueryRaw<LowStockRow>(
                "EXEC dbo.usp_GetLowStockReport @Threshold={0}",
                threshold)
            .AsEnumerable()
            .OrderBy(row => row.StockQuantity)
            .ToList();
    }

    public InventorySummary GetInventorySummaryInterpolated(decimal minPrice)
    {
        // SqlQuery with FormattableString — parameterized like FromSqlInterpolated
        FormattableString sql = $"""
            SELECT
                COUNT(*) AS ProductCount,
                SUM(StockQuantity) AS TotalUnits,
                AVG(UnitPrice) AS AveragePrice
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
              AND UnitPrice >= {minPrice}
            """;

        return _context.Database.SqlQuery<InventorySummary>(sql).Single();
    }
}
