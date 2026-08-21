using System;
using Dapper;
using Dapper.Contrib.Extensions;
using DapperMappingAndAdvancedPatterns.Models;
using Microsoft.Data.SqlClient;

namespace DapperMappingAndAdvancedPatterns.Repositories;

/*
 * FILE ROLE:
 *   Minimal Dapper.Contrib Insert and Get demonstration (SECTION 7 preview).
 *
 * SECTIONS IN THIS FILE:
 *   7. Dapper.Contrib — Insert / Get extensions
 */

public sealed class ContribPreviewRepository
{
    private readonly string _connectionString;

    public ContribPreviewRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /*
     * SECTION 7: DAPPER.CONTRIB — INSERT / GET PREVIEW
     *
     * AdoNetTutorial.Products uses manual ProductId — [ExplicitKey] includes it in INSERT.
     * Assign MAX(ProductId)+1 before Insert; Get<T>(id) selects by that key.
     * -------------------------------------------------------------------------
     */
    public ContribProduct InsertAndGetSampleProduct()
    {
        ContribProduct newProduct = new ContribProduct
        {
            ProductName = "Contrib Demo Part",
            UnitPrice = 4.25m,
            StockQuantity = 40,
        };

        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();

        newProduct.ProductId = connection.ExecuteScalar<int>(
            "SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products;");

        connection.Insert(newProduct); // INSERT all columns including assigned ProductId
        ContribProduct? loaded = connection.Get<ContribProduct>(newProduct.ProductId);

        if (loaded is null)
        {
            throw new InvalidOperationException("Contrib Get returned null after Insert.");
        }

        return loaded;
    }
}
