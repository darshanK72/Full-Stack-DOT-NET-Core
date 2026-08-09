/*
 * PROBLEM: Customer Import Validator
 *
 * Support operations validate CRM import rows with compiled regex patterns,
 * extract order references from free-text notes, and redact phone numbers.
 *
 * This exercise covers:
 *   ch03 — static readonly Regex with Compiled option
 *   ch03 — IsMatch, Match, named groups
 *   ch03 — Replace for redaction
 *   ch03 — batch validation report
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CustomerSupport
{
    /*
     * One imported customer row from the legacy CRM export.
     */
    class ImportRow
    {
        public ImportRow(string rowId, string email, string phone, string productCode, string notes)
        {
            RowId = rowId;
            Email = email;
            Phone = phone;
            ProductCode = productCode;
            Notes = notes;
        }

        public string RowId { get; }
        public string Email { get; }
        public string Phone { get; }
        public string ProductCode { get; }
        public string Notes { get; }
    }

    /*
     * Single validation failure for one field on one row.
     */
    class ValidationIssue
    {
        public ValidationIssue(string rowId, string field, string message)
        {
            RowId = rowId;
            Field = field;
            Message = message;
        }

        public string RowId { get; }
        public string Field { get; }
        public string Message { get; }
    }

    /*
     * Regex-based import gate. Patterns compiled once as static fields.
     * No Console I/O in this class.
     */
    class ImportValidationEngine
    {
        // TODO: declare private static readonly Regex fields per PROBLEM.md patterns

        /*
         * Returns false for null, empty, or malformed email.
         */
        public bool IsValidEmail(string email)
        {
            // TODO: IsMatch with Email pattern
            throw new NotImplementedException();
        }

        /*
         * Returns false for null, empty, or malformed product code (ABC-1234).
         */
        public bool IsValidProductCode(string code)
        {
            // TODO: IsMatch with ProductCode pattern
            throw new NotImplementedException();
        }

        /*
         * Finds all "order {n}" mentions; returns parsed integers in match order.
         *
         * Only read group "id" when match.Success is true.
         */
        public IReadOnlyList<int> ExtractOrderIds(string notes)
        {
            // TODO: iterate matches, read named group id
            throw new NotImplementedException();
        }

        /*
         * Replaces US-style phone patterns (###-###-####) with [REDACTED].
         */
        public string RedactPhones(string notes)
        {
            // TODO: Regex.Replace
            throw new NotImplementedException();
        }

        /*
         * Validates every row; emits separate issues for Email and ProductCode failures.
         */
        public IReadOnlyList<ValidationIssue> ValidateBatch(IReadOnlyList<ImportRow> rows)
        {
            // TODO: loop rows, collect issues
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed 5+ ImportRow samples (valid and invalid)
            // TODO: print validation issues
            // TODO: extract and print order ids from notes
            // TODO: print original vs redacted notes for phone row
            throw new NotImplementedException();
        }
    }
}
