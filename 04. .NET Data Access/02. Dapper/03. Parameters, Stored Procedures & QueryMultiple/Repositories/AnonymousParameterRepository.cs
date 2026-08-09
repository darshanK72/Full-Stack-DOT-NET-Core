using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ParametersStoredProceduresAndQueryMultiple.Models;
using ParametersStoredProceduresAndQueryMultiple.Utils;

namespace ParametersStoredProceduresAndQueryMultiple.Repositories;

/*
 * FILE ROLE: Anonymous object parameters and @ naming conventions for Dapper SQL.
 *
 * SECTIONS IN THIS FILE:
 *   2. Parameter naming — @ conventions
 *   3. Anonymous object parameters
 */

/*
 * SECTION 2: PARAMETER NAMING — @ CONVENTIONS
 *
 * Dapper binds parameters by matching names between SQL tokens and the param object.
 *
 *   SQL token in command text     | Param object member   | Bound as
 *   ------------------------------|-----------------------|---------------------------
 *   @ProductId                    | ProductId = 1         | @ProductId (Input)
 *   @productId (same, case-insensitive) | productId = 1   | @ProductId
 *   @MinPrice                     | MinPrice = 10m        | @MinPrice
 *
 * Rules:
 *   • SQL uses @ParameterName — T-SQL and Dapper convention (same as ADO.NET ch.03).
 *   • Anonymous/type properties omit @ — Dapper adds it when building SqlParameter.
 *   • Names must match (case-insensitive default). @Id in SQL needs property Id, not ProductId.
 *   • DynamicParameters uses explicit names: dp.Add("@ProductId", 1) — @ optional in Add
 *     but the stored name should include @ for clarity (SECTION 4).
 *
 * Pitfall: Property MinPrice but SQL @MinimumPrice — silent mismatch or runtime error
 * depending on provider; always align names or alias in SQL.
 *
 * COVERED IN ADO.NET ch.03 → SqlCommand & Parameters (SqlParameter names and types).
 */
public sealed class AnonymousParameterRepository
{
    private readonly string _connectionString;

    public AnonymousParameterRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /*
     * SECTION 3: ANONYMOUS OBJECT PARAMETERS
     *
     * Pass parameters as an anonymous type: new { ProductId = 1, MinPrice = 10m }
     *
     *   | Approach              | Example                                      | When to use
     *   |-----------------------|----------------------------------------------|-------------
     *   | Anonymous object      | new { Id = 1, Name = "x" }                   | Ad-hoc queries, few params
     *   | Strongly typed object | new ProductFilter { MinPrice = 10m }         | Reusable filter DTOs
     *   | DynamicParameters     | dp.Add("@Id", 1); dp.Add(..., direction: ...) | Output, ReturnValue, TVPs
     *
     * Dapper reflects public properties/getters on the object and creates parameters.
     * Works with Query, Execute, ExecuteScalar, QueryMultiple, and CommandDefinition.
     *
     * --- 3a. Multiple parameters in one object ---
     *
     * One anonymous object can supply every @ token in the SQL string — no Add() calls.
     *
     * --- 3b. Single-property shorthand ---
     *
     * Query<T>(sql, new { ProductId = id }) — one property binds one @ProductId.
     */
    public Product? GetById(int productId)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductId = @ProductId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        // Property ProductId matches @ProductId in SQL — Dapper builds the parameter
        return connection.QuerySingleOrDefault<Product>(sql, new { ProductId = productId });
    }

    public IEnumerable<Product> GetByPriceRange(decimal minPrice, decimal maxPrice)
    {
        const string sql = """
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE UnitPrice >= @MinPrice AND UnitPrice <= @MaxPrice
            ORDER BY UnitPrice;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        // Two properties bind two @ tokens in one round trip
        return connection.Query<Product>(sql, new { MinPrice = minPrice, MaxPrice = maxPrice });
    }

    public int UpdatePrice(int productId, decimal newPrice)
    {
        const string sql = """
            UPDATE dbo.Products
            SET UnitPrice = @NewPrice
            WHERE ProductId = @ProductId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);
        // Execute uses the same param object as Query — anonymous object is not Query-only
        return connection.Execute(sql, new { ProductId = productId, NewPrice = newPrice });
    }

    public void ShowAnonymousBindingConcept()
    {
        Console.WriteLine("=== Anonymous parameters (no database) ===");
        object parameters = new { ProductId = 2, MinPrice = 10m, MaxPrice = 50m };
        Console.WriteLine("  Param object type: {0}", parameters.GetType().Name);
        Console.WriteLine("  Binds @ProductId, @MinPrice, @MaxPrice from property names");
    }
}
