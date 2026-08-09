namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: Forward reference to async transaction patterns in chapter 08.
 *
 * SECTIONS IN THIS FILE:
 *  12. Async transactions — preview
 */

/*
 * SECTION 12: ASYNC TRANSACTIONS - PREVIEW
 *
 * COVERED IN DETAIL LATER -> 08. Async ADO.NET
 *
 * Async counterparts mirror the synchronous API:
 *   await connection.OpenAsync(cancellationToken);
 *   await command.ExecuteNonQueryAsync(cancellationToken);
 *
 * BeginTransaction() still returns SqlTransaction synchronously; commands inside
 * the transaction use *Async execute methods. Full patterns in chapter 08.
 */
internal static class AsyncTransactionPreview
{
    public static string ForwardReference()
    {
        return "Async ADO.NET (OpenAsync, ExecuteNonQueryAsync, cancellation) -> chapter 08.";
    }
}
