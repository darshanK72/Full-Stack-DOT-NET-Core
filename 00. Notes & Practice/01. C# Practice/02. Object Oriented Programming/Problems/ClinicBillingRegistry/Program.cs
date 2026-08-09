/*
 * PROBLEM: Clinic Billing Registry
 *
 * A clinic maps CPT procedure codes to descriptions and assembles visit
 * invoices. Codes are fixed at startup; lines are indexed per visit.
 *
 * This exercise covers:
 *   ch02 — expression-bodied FormattedAmount property
 *   ch02 — get-only indexer on VisitInvoice
 *   ch03 — constructor chaining; copy constructor; private constructor
 *   ch04 — static class; static constructor; const vs static readonly
 *   ch07 — encapsulation via private line list and factory
 */

using System;
using System.Collections.Generic;

namespace HealthcareBilling
{
    enum ProcedureCategory
    {
        Consultation,
        Lab,
        Imaging,
        Procedure
    }

    /*
     * Static catalog of procedure codes loaded once at type initialization.
     */
    static class ProcedureCatalog
    {
        public const int MaxLinesPerVisit = 20;

        private static readonly Dictionary<string, string> _codes = new Dictionary<string, string>();

        static ProcedureCatalog()
        {
            // TODO: populate _codes with at least 6 CPT-style codes and descriptions
            throw new NotImplementedException();
        }

        public static int CodeCount => _codes.Count;

        public static bool TryGetDescription(string code, out string? description)
        {
            // TODO: lookup description
            throw new NotImplementedException();
        }
    }

    class BillingLine
    {
        public string Code { get; }
        public string Description { get; }
        public decimal Amount { get; }

        public string FormattedAmount => Amount.ToString("C");

        public BillingLine(string code, string description, decimal amount)
        {
            Code = code;
            Description = description;
            Amount = amount;
        }

        public BillingLine(BillingLine other)
        {
            // TODO: copy all fields
            throw new NotImplementedException();
        }
    }

    /*
     * One visit invoice. Only creatable via Create factory — default ctor is private.
     */
    class VisitInvoice
    {
        private readonly List<BillingLine> _lines = new List<BillingLine>();

        public int VisitId { get; }

        public int LineCount => _lines.Count;

        public BillingLine this[int index]
        {
            get
            {
                // TODO: throw if out of range
                throw new NotImplementedException();
            }
        }

        private VisitInvoice()
            : this(0)
        {
        }

        public VisitInvoice(int visitId)
        {
            VisitId = visitId;
        }

        public static VisitInvoice Create(int visitId)
        {
            // TODO: reject visitId <= 0
            throw new NotImplementedException();
        }

        public bool TryAddLine(string code, decimal amount)
        {
            // TODO: fail at MaxLinesPerVisit, unknown code, or amount <= 0
            throw new NotImplementedException();
        }

        public decimal GetTotal()
        {
            // TODO: sum line amounts
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: add lines to two visits; clone a line via copy constructor
            // TODO: print indexed lines and totals for each visit
            throw new NotImplementedException();
        }
    }
}
