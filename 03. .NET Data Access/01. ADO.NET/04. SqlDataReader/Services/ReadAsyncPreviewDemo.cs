using System;

namespace SqlDataReader;

/*
 * FILE ROLE: Points forward to async reader APIs covered in chapter 08.
 *
 * SECTIONS IN THIS FILE:
 *   11. ReadAsync — preview (ch.08 Async ADO.NET)
 */

/*
 * =========================================================================
 * SECTION 11: ReadAsync — PREVIEW (ch.08 Async ADO.NET)
 * =========================================================================
 *
 * Async overloads avoid blocking thread-pool threads during network I/O:
 *
 *   await using var reader = await cmd.ExecuteReaderAsync();
 *   while (await reader.ReadAsync()) { … }
 *
 * Requires async Main or an async helper. Full patterns, cancellation
 * tokens, and OpenAsync → COVERED IN DETAIL LATER → 08. Async ADO.NET
 * -------------------------------------------------------------------------
 */
public static class ReadAsyncPreviewDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 11: ReadAsync preview ---");
        Console.WriteLine("  Sync Read() used in this chapter.");
        Console.WriteLine("  Async: await reader.ReadAsync() → see 08. Async ADO.NET");
    }
}
