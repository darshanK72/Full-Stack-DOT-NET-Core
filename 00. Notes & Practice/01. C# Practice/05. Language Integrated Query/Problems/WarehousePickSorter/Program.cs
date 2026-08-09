/*
 * PROBLEM: Warehouse Pick Sorter
 *
 * Fulfillment pick lists sorted by zone then priority. Zone labels like A-12 and A-3
 * must sort numerically within the same letter prefix.
 *
 * This exercise covers:
 *   ch03 — OrderBy, ThenBy, OrderByDescending, Reverse, IComparer, stable sort
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseFulfillment
{
    record PickTicket(int TicketId, string Zone, int Priority, string Sku);

    /*
     * Compares zone strings Letter-Number with numeric suffix ordering.
     */
    class ZoneComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            // TODO: parse hyphenated zones; fallback to OrdinalIgnoreCase
            throw new NotImplementedException();
        }
    }

    /*
     * Sorts pick tickets with LINQ ordering operators.
     */
    class WarehousePickSorter
    {
        private readonly IEnumerable<PickTicket> _tickets;

        public WarehousePickSorter(IEnumerable<PickTicket> tickets)
        {
            _tickets = tickets;
        }

        /*
         * OrderBy Zone (ZoneComparer), ThenBy Priority ascending, ThenBy Sku.
         */
        public IEnumerable<PickTicket> ByZoneThenPriority()
        {
            // TODO: OrderBy + ThenBy chain — not second OrderBy
            throw new NotImplementedException();
        }

        /*
         * Highest priority first (lower Priority number = higher priority → OrderByDescending Priority).
         */
        public IEnumerable<PickTicket> ByPriorityDescending()
        {
            // TODO: OrderByDescending Priority
            throw new NotImplementedException();
        }

        /*
         * Reverses source enumeration order — not the same as OrderByDescending.
         */
        public IEnumerable<PickTicket> ReverseOriginal()
        {
            // TODO: Reverse()
            throw new NotImplementedException();
        }

        /*
         * Query syntax sort by Zone, Priority, Sku (zone comparer may require method syntax for OrderBy only).
         */
        public IEnumerable<PickTicket> QuerySyntaxSort()
        {
            // TODO: from t in _tickets orderby ... select t
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed tickets including A-3 and A-12
            // TODO: print ByZoneThenPriority ids — A-3 before A-12
            // TODO: ByPriorityDescending first id
            // TODO: ReverseOriginal vs source order
            throw new NotImplementedException();
        }
    }
}
