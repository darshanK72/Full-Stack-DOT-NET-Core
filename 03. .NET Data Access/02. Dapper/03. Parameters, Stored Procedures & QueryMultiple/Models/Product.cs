using System;

namespace ParametersStoredProceduresAndQueryMultiple.Models;

/*
 * FILE ROLE: Product row type mapped by Dapper from dbo.Products and procedure result sets.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product row type — column name mapping
 */

/*
 * SECTION 1: PRODUCT ROW TYPE — COLUMN NAME MAPPING
 *
 * Dapper maps result columns to properties by name (case-insensitive by default).
 * SQL column ProductId maps to C# ProductId; ProductName to ProductName, etc.
 *
 *   SQL column (AdoNetTutorial) | C# property     | Notes
 *   -----------------------------|-----------------|----------------------------------
 *   ProductId                    | ProductId       | PK — matches @ProductId params
 *   ProductName                  | ProductName     | NVARCHAR(100)
 *   UnitPrice                    | UnitPrice       | DECIMAL(10,2)
 *   StockQuantity                | StockQuantity   | INT
 *   DiscontinuedDate             | DiscontinuedDate| DATE NULL → DateTime?
 *
 * If names differ, alias in SQL: SELECT ProductId AS Id ... and map to property Id,
 * or use custom column mapping (ch.04 Mapping, Multi-Mapping & Advanced Patterns).
 *
 * Parameter objects use the same naming rule: property ProductId binds @ProductId
 * in SQL (SECTION 2 in Repositories/AnonymousParameterRepository.cs).
 */
public sealed class Product
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int StockQuantity { get; init; }
    public DateTime? DiscontinuedDate { get; init; }
}
