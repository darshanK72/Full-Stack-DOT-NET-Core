using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChangeTrackingAsyncAndTransactions.Data;

/*
 * FILE ROLE:
 *   Preview of EF Core interceptors and SaveChanges events — registration pattern only.
 *
 * SECTIONS IN THIS FILE:
 *   9. Interceptors and SaveChanges events (preview)
 */

/*
 * SECTION 9: INTERCEPTORS AND SAVECHANGES EVENTS (PREVIEW)
 *
 * Interceptors implement IInterceptor and plug into EF Core pipeline methods
 * (connection, command, SaveChanges). SaveChangesInterceptor is the common hook
 * for auditing, soft-delete, or tenant filters applied at save time.
 *
 * | Hook                         | When it runs                          |
 * |------------------------------|---------------------------------------|
 * | SavingChanges / Async        | Before SQL is generated for save      |
 * | SavedChanges / Async         | After database accepts the transaction|
 * | DbContext.SavingChanges      | Legacy event on DbContext (avoid mix) |
 *
 * Register: optionsBuilder.AddInterceptors(new AuditSaveChangesInterceptor());
 *           or services.AddDbContext<T>(o => o.AddInterceptors(...)) in ASP.NET.
 *
 * COVERED IN DETAIL LATER -> dedicated advanced EF Core topic (not in this track).
 * This file shows a minimal SavingChangesAsync override you can register in
 * InventoryDbContext.OnConfiguring when experimenting.
 * -------------------------------------------------------------------------
 */
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;
        if (context is not null)
        {
            int trackedCount = context.ChangeTracker.Entries().Count(e => e.State != EntityState.Unchanged);
            Console.WriteLine($"  [Interceptor preview] SavingChangesAsync — {trackedCount} tracked change(s).");
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
