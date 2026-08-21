using System;

namespace StoredProceduresAndOutputParameters.Services;

/*
 * FILE ROLE: Forward reference to async stored procedure execution in chapter 08.
 *
 * SECTIONS IN THIS FILE:
 *   8. Preview — ExecuteNonQueryAsync
 */

/*
 * SECTION 8: PREVIEW - ExecuteNonQueryAsync
 *
 * Async ADO.NET mirrors sync methods: open the connection and execute with
 * *Async suffixed methods. Output and ReturnValue parameters are still read
 * from .Value after await ExecuteNonQueryAsync() completes.
 *
 *   await connection.OpenAsync();
 *   await command.ExecuteNonQueryAsync();
 *   int id = (int)outputParam.Value;
 *
 * COVERED IN DETAIL LATER -> 08. Async ADO.NET
 *   (OpenAsync, ExecuteReaderAsync, cancellation tokens, ConfigureAwait)
 */
public static class AsyncPreview
{
    public static void ShowAsyncSignaturePreview()
    {
        Console.WriteLine();
        Console.WriteLine("=== Preview: async stored procedure execute ===");
        Console.WriteLine("  await command.ExecuteNonQueryAsync(cancellationToken);");
        Console.WriteLine("  Read Output/ReturnValue from SqlParameter.Value after await completes.");
        Console.WriteLine("  Full async patterns -> chapter 08. Async ADO.NET");
    }
}
