namespace DapperMappingAndAdvancedPatterns.Models;

/*
 * FILE ROLE:
 *   Row types for default column-to-property mapping (SECTION 1).
 *
 * SECTIONS IN THIS FILE:
 *   1. Default column-to-property mapping
 */

/*
 * SECTION 1: DEFAULT COLUMN-TO-PROPERTY MAPPING
 *
 * Dapper maps result set columns to public settable properties by name match.
 *
 * | Behavior              | Detail                                              |
 * |-----------------------|-----------------------------------------------------|
 * | Name match            | Case-insensitive — ProductId, productid, PRODUCTID  |
 * | Extra columns         | Ignored if no matching property                     |
 * | Missing columns       | Property stays default (0, null, false)             |
 * | Constructor           | Parameterless ctor required (or record with init)     |
 *
 * Column names follow the AdoNetTutorial Products table from ADO.NET ch.04:
 * ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate.
 * -------------------------------------------------------------------------
 */
public sealed class ProductRow
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
