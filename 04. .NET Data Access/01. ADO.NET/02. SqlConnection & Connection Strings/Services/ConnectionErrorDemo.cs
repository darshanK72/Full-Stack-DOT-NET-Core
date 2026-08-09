using System;
using Microsoft.Data.SqlClient;

namespace SqlConnectionAndConnectionStrings.Services;

/*
 * FILE ROLE:
 *   Common connection string and SqlException error demonstrations.
 *
 * SECTIONS IN THIS FILE:
 *   9. Common errors — SqlException and invalid strings
 */

/*
 * =========================================================================
 * SECTION 9: COMMON ERRORS — SqlException AND INVALID STRINGS
 * =========================================================================
 *
 * Invalid connection string (malformed syntax):
 *   ArgumentException — e.g. empty string, bad quoting, invalid boolean
 *
 * Valid string but server unreachable / login failed:
 *   SqlException — Number property (e.g. 2 = instance not found, 18456 = login)
 *   InnerException may hold socket or SSL details
 *
 * Always catch SqlException around Open() in apps; log Number and Message.
 * Do not catch and swallow in libraries without rethrow or result type.
 * -------------------------------------------------------------------------
 */
public static class ConnectionErrorDemo
{
    public static void DemonstrateInvalidConnectionString()
    {
        try
        {
            _ = new SqlConnection("Not A Valid=Connection String=Without=Known=Keys;");
            Console.WriteLine("  Unexpected: invalid string was accepted.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  ArgumentException (bad string): {ex.Message}");
        }
    }

    public static void DemonstrateUnreachableServer()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "InvalidHostName.DoesNotExist.local",
            InitialCatalog = "NoDatabase",
            IntegratedSecurity = true,
            ConnectTimeout = 3,
            TrustServerCertificate = true
        };

        try
        {
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"  SqlException (unreachable host, Number={ex.Number}): {ex.Message}");
        }
    }
}
