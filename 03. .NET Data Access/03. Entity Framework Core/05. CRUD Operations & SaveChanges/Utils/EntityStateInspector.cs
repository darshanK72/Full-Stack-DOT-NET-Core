using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CrudOperationsAndSaveChanges.Utils;

/*
 * FILE ROLE: Reads EntityEntry.State from the change tracker so the demo can
 *            print Added, Modified, Deleted, and Unchanged while CRUD runs.
 *
 * SECTIONS IN THIS FILE:
 *   8. Entity states — Added, Modified, Deleted, Unchanged
 */

/*
 * =========================================================================
 * SECTION 8: ENTITY STATES — Added, Modified, Deleted, Unchanged
 * =========================================================================
 *
 * EF Core tracks each entity instance attached to a DbContext in one of these
 * states (enum EntityState):
 *
 *   State      | Meaning                         | Typical cause
 *   -----------|---------------------------------|----------------------------------
 *   Detached   | Not tracked by this context     | new object(), after AsNoTracking
 *   Unchanged  | Matches last known DB snapshot  | after query, after SaveChanges
 *   Added      | Pending INSERT                  | DbSet.Add / AddRange
 *   Modified   | Pending UPDATE                  | property change on tracked entity
 *              |                                 | or DbSet.Update (detached graph)
 *   Deleted    | Pending DELETE                  | DbSet.Remove / RemoveRange
 *
 * Inspect state:
 *   EntityEntry entry = context.Entry(entity);
 *   EntityState state = entry.State;
 *
 *   foreach (EntityEntry e in context.ChangeTracker.Entries())
 *       Console.WriteLine($"{e.Entity.GetType().Name}: {e.State}");
 *
 * Nothing hits the database until SaveChanges (SECTION 7). States describe
 * what SQL EF will generate on the next flush.
 *
 * Advanced attach/update patterns, AsNoTracking, and SaveChangesAsync ->
 * COVERED IN DETAIL LATER -> 11. Change Tracking, Async & Transactions.
 * -------------------------------------------------------------------------
 */
public static class EntityStateInspector
{
    public static string Describe<TEntity>(DbContext context, TEntity entity)
        where TEntity : class
    {
        EntityEntry entry = context.Entry(entity);
        return $"{typeof(TEntity).Name} [{entry.State}]";
    }

    public static string DescribeAllTracked(DbContext context)
    {
        EntityEntry[] entries = context.ChangeTracker.Entries().ToArray();
        if (entries.Length == 0)
        {
            return "  (no tracked entities)";
        }

        var builder = new StringBuilder();
        foreach (EntityEntry entry in entries)
        {
            builder.Append("  ");
            builder.Append(entry.Entity.GetType().Name);
            builder.Append(" -> ");
            builder.AppendLine(entry.State.ToString());
        }

        return builder.ToString().TrimEnd();
    }
}
