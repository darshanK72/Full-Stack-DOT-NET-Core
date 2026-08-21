using System;

namespace SqlConnectionAndConnectionStrings.Services;

/*
 * FILE ROLE:
 *   Forward references to connection pooling and SqlCommand (next chapters).
 *
 * SECTIONS IN THIS FILE:
 *  10. Preview — connection pooling (ch.06)
 *  11. Preview — SqlCommand (ch.03)
 */

/*
 * =========================================================================
 * SECTION 10: PREVIEW — CONNECTION POOLING (ch.06)
 * =========================================================================
 *
 * SqlClient pools open connections by connection string signature. Dispose()
 * returns the socket to the pool instead of tearing down TCP every time.
 * Pool size, clearing pools, and transaction interaction are taught in:
 *
 *   COVERED IN DETAIL LATER → 06. Transactions & Connection Pooling
 * -------------------------------------------------------------------------
 */
public static class PoolingPreview
{
    public static void PrintPoolingNote()
    {
        Console.WriteLine("--- Connection pooling (preview) ---");
        Console.WriteLine("  Dispose() returns connections to the pool — do not hold SqlConnection open.");
        Console.WriteLine("  COVERED IN DETAIL LATER → 06. Transactions & Connection Pooling");
    }
}

/*
 * =========================================================================
 * SECTION 11: PREVIEW — SqlCommand (ch.03)
 * =========================================================================
 *
 * After Open(), you run SQL with SqlCommand.ExecuteNonQuery / ExecuteScalar /
 * ExecuteReader. Parameters prevent SQL injection. Not shown here — next chapter:
 *
 *   COVERED IN DETAIL LATER → 03. SqlCommand & Parameters
 * -------------------------------------------------------------------------
 */
public static class CommandPreview
{
    public static void PrintCommandNote()
    {
        Console.WriteLine("--- SqlCommand (preview) ---");
        Console.WriteLine("  Open SqlConnection first, then attach SqlCommand with CommandText + parameters.");
        Console.WriteLine("  COVERED IN DETAIL LATER → 03. SqlCommand & Parameters");
    }
}
