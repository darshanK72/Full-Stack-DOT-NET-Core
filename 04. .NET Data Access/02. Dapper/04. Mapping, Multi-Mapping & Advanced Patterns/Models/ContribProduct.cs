using Dapper.Contrib.Extensions;

namespace DapperMappingAndAdvancedPatterns.Models;

/*
 * FILE ROLE:
 *   Entity type for Dapper.Contrib Insert/Get preview (SECTION 7).
 *
 * SECTIONS IN THIS FILE:
 *   7. Dapper.Contrib — Table/Key attributes and extension methods preview
 */

/*
 * SECTION 7: DAPPER.CONTRIB — INSERT / GET PREVIEW
 *
 * Maps to dbo.Products (AdoNetTutorial schema). ProductId is NOT IDENTITY — use [ExplicitKey]
 * so Contrib includes the assigned key in INSERT ( [Key] is for identity columns only ).
 *
 *   connection.Insert(entity);
 *   connection.Get<T>(id);
 * -------------------------------------------------------------------------
 */
[Table("Products")]
public sealed class ContribProduct
{
    [ExplicitKey]
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
