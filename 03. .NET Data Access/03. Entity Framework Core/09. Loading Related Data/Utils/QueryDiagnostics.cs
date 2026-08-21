using LoadingRelatedData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LoadingRelatedData.Utils;

/*
 * FILE ROLE:
 *   Counts SQL commands executed by EF Core for N+1 vs Include comparison in Program.cs.
 *
 * SECTIONS IN THIS FILE:
 *   6. Query diagnostics - LogTo command counter
 */

/*
 * SECTION 6: QUERY DIAGNOSTICS - COUNT SQL ROUND-TRIPS
 *
 * Attach to DbContextOptionsBuilder before creating ShopDbContext.
 * RelationalEventId.CommandExecuted fires once per SQL batch sent to SQL Server.
 *
 * Use during SECTION 6 demo to compare N+1 loop vs single Include query.
 * -------------------------------------------------------------------------
 */
public sealed class QueryDiagnostics
{
    private int _commandCount;

    public int CommandCount => _commandCount;

    public void Reset() => _commandCount = 0;

    public ShopDbContext CreateContext(string connectionString)
    {
        DbContextOptionsBuilder<ShopDbContext> builder = new DbContextOptionsBuilder<ShopDbContext>();
        builder.UseSqlServer(connectionString);
        builder.LogTo(
            _ => _commandCount++,
            new[] { RelationalEventId.CommandExecuted });
        return new ShopDbContext(builder.Options);
    }
}
