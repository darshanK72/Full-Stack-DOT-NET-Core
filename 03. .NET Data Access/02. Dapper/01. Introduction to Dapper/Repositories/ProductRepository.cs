using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using IntroductionToDapper.Models;
using Microsoft.Data.SqlClient;

namespace IntroductionToDapper.Repositories;

/*
 * FILE ROLE: Minimal product repository showing Dapper Query<T> and a thin
 *            data-access layer over IDbConnection.
 *
 * SECTIONS IN THIS FILE:
 *   3. First Query<T> — map rows to a POCO
 *   6. Minimal repository pattern with Dapper
 */

public sealed class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /*
     * =========================================================================
     * SECTION 3: FIRST Query<T> — MAP ROWS TO A POCO
     * =========================================================================
     *
     * Query<T> executes SQL and maps each row to T using column-to-property
     * name matching (see Models/Product.cs SECTION 7).
     *
     * ADO.NET equivalent (ch04 SqlDataReader) — manual loop:
     *
     *   using var reader = cmd.ExecuteReader();
     *   while (reader.Read())
     *   {
     *       list.Add(new Product
     *       {
     *           ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
     *           ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
     *           …
     *       });
     *   }
     *
     * Dapper replaces that boilerplate with one line. You still own the SQL
     * and the connection lifecycle.
     *
     * Query returns IEnumerable<T> — by default buffered (all rows read before
     * return). Materialize with ToList() before disposing the connection.
     *
     * QueryFirst, QuerySingle, Execute, async variants → ch02.
     * Parameters (@name, anonymous objects) → ch03.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<Product> GetActiveProducts()
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE DiscontinuedDate IS NULL
            ORDER BY ProductId;
            """;

        using IDbConnection connection = new SqlConnection(_connectionString);
        return connection.Query<Product>(sql).ToList(); // buffered read; safe after using disposes connection
    }

    /*
     * =========================================================================
     * SECTION 6: MINIMAL REPOSITORY PATTERN WITH DAPPER
     * =========================================================================
     *
     * A repository encapsulates SQL and connection creation so controllers or
     * services depend on methods, not raw SQL strings scattered through the app.
     *
     * Pattern shown here:
     *   • Constructor stores connection string (or accept IDbConnection for tests)
     *   • One method per use case with explicit SQL
     *   • using IDbConnection + Dapper extension per call (simple, pool-friendly)
     *
     * Larger apps may inject IDbConnection from DI, share connections inside
     * transactions (ch06 ADO.NET), or add async methods (ch02).
     * -------------------------------------------------------------------------
     */
    public Product? GetById(int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductId = @productId;
            """;

        using IDbConnection connection = new SqlConnection(_connectionString);
        // Anonymous object binds @productId — full parameter patterns → ch03
        return connection.QueryFirstOrDefault<Product>(sql, new { productId });
    }

    public int CountActiveProducts()
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Products WHERE DiscontinuedDate IS NULL;";

        using IDbConnection connection = new SqlConnection(_connectionString);
        return connection.ExecuteScalar<int>(sql); // ExecuteScalar family preview → ch02
    }
}
