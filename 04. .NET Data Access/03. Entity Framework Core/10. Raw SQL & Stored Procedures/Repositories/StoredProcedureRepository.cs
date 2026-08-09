using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RawSqlAndStoredProcedures.Data;
using RawSqlAndStoredProcedures.Models;

namespace RawSqlAndStoredProcedures.Repositories;

/*
 * FILE ROLE: Stored procedures that return Product row shapes via DbSet.FromSqlRaw.
 *
 * SECTIONS IN THIS FILE:
 *   8. Stored procedures mapped to entities
 */

/*
 * SECTION 8: STORED PROCEDURES MAPPED TO ENTITIES
 *
 * When a procedure SELECTs columns that match a mapped entity, call FromSqlRaw on the DbSet:
 *
 *   context.Products.FromSqlRaw("EXEC dbo.usp_GetAllProducts")
 *
 * ADO.NET (ch.07): SqlCommand.CommandType = StoredProcedure; CommandText = "dbo.usp_...";
 * Dapper (ch.03): connection.Query<Product>("dbo.usp_...", commandType: StoredProcedure);
 *
 * --- Forms that work on SQL Server ---
 *
 *   EXEC dbo.usp_GetProductsByMinStock {0}
 *   EXEC dbo.usp_GetProductsByMinStock @MinStock={0}
 *
 * Both send @MinStock as a parameter — prefer explicit names for readability.
 *
 * Rules:
 *   • Procedure result columns must match entity property names/types.
 *   • OUTPUT / RETURN parameters are NOT supported on FromSqlRaw — use ExecuteSqlRaw with
 *     SqlParameter Direction Output (ADO.NET ch.07) or stay on Dapper DynamicParameters.
 *   • Multiple result sets: FromSqlRaw reads the FIRST result set only (same as Dapper Query).
 *
 * Pitfall: Do not embed user text in EXEC string — always pass {0} placeholders.
 */
public sealed class StoredProcedureRepository
{
    private readonly EfCoreTutorialDbContext _context;

    public StoredProcedureRepository(EfCoreTutorialDbContext context)
    {
        _context = context;
    }

    public List<Product> GetAllViaStoredProcedure()
    {
        return _context.Products
            .FromSqlRaw("EXEC dbo.usp_GetAllProducts")
            .AsNoTracking()
            .ToList();
    }

    public List<Product> GetByMinStock(int minStock)
    {
        return _context.Products
            .FromSqlRaw("EXEC dbo.usp_GetProductsByMinStock @MinStock={0}", minStock)
            .AsNoTracking()
            .ToList();
    }
}
