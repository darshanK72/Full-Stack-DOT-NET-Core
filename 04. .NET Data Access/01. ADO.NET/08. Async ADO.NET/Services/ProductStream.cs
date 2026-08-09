using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AsyncAdoNet.Models;
using Microsoft.Data.SqlClient;

namespace AsyncAdoNet.Services;

/*
 * FILE ROLE:
 *   Async streaming over SqlDataReader via IAsyncEnumerable and await foreach.
 *
 * SECTIONS IN THIS FILE:
 *   4. PREVIEW — IAsyncEnumerable over SqlDataReader
 */

/*
 * SECTION 4: PREVIEW -- IAsyncEnumerable OVER SqlDataReader
 *
 * IAsyncEnumerable<T> + await foreach streams rows without loading an entire
 * List<T> into memory. SqlDataReader is forward-only; combine ReadAsync with
 * yield return for pull-based async streaming.
 *
 * COVERED IN DETAIL LATER -> Advanced C# Features (async streams / IAsyncEnumerable).
 * -------------------------------------------------------------------------
 */
public static class ProductStream
{
    public static async IAsyncEnumerable<ProductRow> ReadProductsAsync(
        SqlDataReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return new ProductRow(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetInt32(3));
        }
    }
}
