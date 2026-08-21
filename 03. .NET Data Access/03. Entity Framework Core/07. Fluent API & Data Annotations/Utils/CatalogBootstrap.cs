using System;
using System.Linq;
using FluentApiAndDataAnnotations.Data;
using FluentApiAndDataAnnotations.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FluentApiAndDataAnnotations.Utils;

/*
 * FILE ROLE: Creates EfCoreTutorial on LocalDB, validates ch07 table schema, and seeds
 *            sample rows when CatalogProducts is empty.
 *
 * SECTIONS IN THIS FILE:
 *   8b. Bootstrap — TryEnsureReady()
 */

/*
 * =========================================================================
 * SECTION 8b: BOOTSTRAP — TryEnsureReady()
 * =========================================================================
 *
 * EfCoreTutorial is shared across EF Core chapters. EnsureCreated() does not
 * alter tables when the database already exists with a different shape.
 *
 * Bootstrap steps:
 *   1. CREATE DATABASE EfCoreTutorial if missing (master connection)
 *   2. Detect whether CatalogProducts / Suppliers match this chapter's model
 *   3. EnsureDeleted + EnsureCreated when schema is incompatible
 *   4. Seed CatalogProducts and Suppliers when empty
 *
 * EnsureCreated vs Migrate:
 *   EnsureCreated — quick for reading tutorials (no __EFMigrationsHistory)
 *   Database.Migrate() — preferred once ch03 migrations exist in your solution
 * -------------------------------------------------------------------------
 */
public static class CatalogBootstrap
{
    public const string DatabaseName = "EfCoreTutorial";

    public static bool TryEnsureReady(DbContextOptions<CatalogDbContext> options, out string? failureReason)
    {
        failureReason = null;

        try
        {
            using (SqlConnection master = new SqlConnection(ConnectionHelper.MasterConnectionString))
            {
                master.Open();
                using SqlCommand createDb = new SqlCommand(
                    """
                    IF DB_ID(@dbName) IS NULL
                        CREATE DATABASE [EfCoreTutorial];
                    """,
                    master);
                createDb.Parameters.AddWithValue("@dbName", DatabaseName);
                createDb.ExecuteNonQuery();
            }

            using CatalogDbContext context = new CatalogDbContext(options);

            if (!SchemaMatchesChapterModel(context))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            else
            {
                context.Database.EnsureCreated();
            }

            SeedIfEmpty(context);
            return true;
        }
        catch (Exception ex)
        {
            failureReason = ex.Message;
            return false;
        }
    }

    private static bool SchemaMatchesChapterModel(CatalogDbContext context)
    {
        if (!context.Database.CanConnect())
        {
            return false;
        }

        try
        {
            int catalogProductColumns = context.Database
                .SqlQueryRaw<int>(
                    """
                    SELECT COUNT(*)
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    WHERE t.name = N'CatalogProducts'
                      AND t.schema_id = SCHEMA_ID(N'dbo')
                      AND c.name IN (N'CatalogProductId', N'Name', N'Description', N'Sku', N'UnitPrice');
                    """)
                .AsEnumerable()
                .FirstOrDefault();

            int supplierColumns = context.Database
                .SqlQueryRaw<int>(
                    """
                    SELECT COUNT(*)
                    FROM sys.columns c
                    INNER JOIN sys.tables t ON c.object_id = t.object_id
                    WHERE t.name = N'Suppliers'
                      AND t.schema_id = SCHEMA_ID(N'dbo')
                      AND c.name IN (N'SupplierId', N'CompanyName', N'SupplierCode', N'Address_Street', N'Address_City', N'Address_PostalCode');
                    """)
                .AsEnumerable()
                .FirstOrDefault();

            return catalogProductColumns == 5 && supplierColumns == 6;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private static void SeedIfEmpty(CatalogDbContext context)
    {
        if (context.CatalogProducts.Any())
        {
            return;
        }

        context.CatalogProducts.AddRange(
            new CatalogProduct { Name = "Wireless Mouse", Sku = "WM-001", UnitPrice = 29.99m, Description = "Ergonomic" },
            new CatalogProduct { Name = "USB-C Hub", Sku = "HUB-12", UnitPrice = 49.50m });

        context.Suppliers.Add(
            new Supplier
            {
                CompanyName = "Acme Parts",
                SupplierCode = "ACME",
                Address = new Address { Street = "100 Industrial Way", City = "Portland", PostalCode = "97201" }
            });

        context.SaveChanges();
    }
}
