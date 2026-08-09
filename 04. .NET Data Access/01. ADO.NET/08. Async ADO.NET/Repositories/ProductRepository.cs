using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AsyncAdoNet.Repositories;

/*
 * FILE ROLE:
 *   Async repository helpers showing ConfigureAwait(false) in library-style code.
 *
 * SECTIONS IN THIS FILE:
 *   3. Library code — ConfigureAwait(false)
 */

/*
 * SECTION 3: LIBRARY CODE -- ConfigureAwait(false)
 *
 * Application code (ASP.NET Minimal API, WinForms event handler) usually omits
 * ConfigureAwait because you often WANT to resume on the request/UI context.
 *
 * Library/repository code that has no UI or HTTP context should call
 * ConfigureAwait(false) on every await so continuations do not marshal back
 * to a captured SynchronizationContext unnecessarily.
 *
 *   await connection.OpenAsync(ct).ConfigureAwait(false);
 *
 * In this console app there is no SynchronizationContext, so behavior is the
 * same -- the pattern is shown so repository helpers are copy-ready for services.
 * -------------------------------------------------------------------------
 */
public static class ProductRepository
{
    public static async Task<int> GetProductCountAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM dbo.Products;";

        object? scalar = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(scalar);
    }

    public static async Task<int> InsertProductAsync(
        string connectionString,
        string name,
        decimal unitPrice,
        int stock,
        CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO dbo.Products (Name, UnitPrice, Stock)
            VALUES (@Name, @UnitPrice, @Stock);
            """;
        command.Parameters.Add(new SqlParameter("@Name", name));
        command.Parameters.Add(new SqlParameter("@UnitPrice", unitPrice));
        command.Parameters.Add(new SqlParameter("@Stock", stock));

        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
