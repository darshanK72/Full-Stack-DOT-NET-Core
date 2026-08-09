using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperMappingAndAdvancedPatterns.Models;
using Microsoft.Data.SqlClient;

namespace DapperMappingAndAdvancedPatterns.Repositories;

/*
 * FILE ROLE:
 *   Dapper column mapping demos — default matching, SQL aliases, and [Column] TypeMap.
 *
 * SECTIONS IN THIS FILE:
 *   1. Default column-to-property mapping
 *   2. SQL column aliases and [Column] attribute mapping
 */

public sealed class MappingRepository
{
    private readonly string _connectionString;

    public MappingRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /*
     * SECTION 1: DEFAULT COLUMN-TO-PROPERTY MAPPING
     *
     * Query<T> materializes each row into T. Column names from the result set are matched
     * to property names case-insensitively — no configuration for simple 1:1 shapes.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<ProductRow> GetProductsDefaultMapping()
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity
            FROM dbo.Products
            ORDER BY ProductId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        IEnumerable<ProductRow> rows = connection.Query<ProductRow>(sql); // maps columns → properties by name
        return rows.ToList();
    }

    /*
     * SECTION 2: SQL COLUMN ALIASES AND [Column] ATTRIBUTE
     *
     * --- 2a. SQL AS aliases ---
     * Alias mismatched columns in SELECT; Dapper binds alias names to properties.
     *
     * --- 2b. [Column] attribute ---
     * Requires ColumnAttributeTypeMap.RegisterOnce() before this query.
     * SELECT uses real column names (UnitPrice); TypeMap maps UnitPrice → Price property.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<ProductWithAlias> GetProductsWithSqlAliases()
    {
        const string sql = """
            SELECT
                ProductId AS Id,
                ProductName AS Title,
                UnitPrice AS Price
            FROM dbo.Products
            ORDER BY ProductId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        return connection.Query<ProductWithAlias>(sql).ToList();
    }

    public ColumnMappedProduct? GetProductWithColumnAttribute(int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice
            FROM dbo.Products
            WHERE ProductId = @productId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        return connection.QuerySingleOrDefault<ColumnMappedProduct>(sql, new { productId });
    }
}
