using System;

namespace QueriesExecuteAndAsyncMethods.Models;

/*
 * FILE ROLE:
 *   Product row type mapped from dbo.Products - target of Dapper Query<T> calls.
 *
 * SECTIONS IN THIS FILE:
 *   1. Product row type and Dapper column mapping
 */

/*
 * =========================================================================
 * SECTION 1: PRODUCT ROW TYPE - DAPPER COLUMN MAPPING
 * =========================================================================
 *
 * Dapper maps result-set columns to properties by name (case-insensitive by
 * default). dbo.Products columns (AdoNetTutorial / ADO.NET ch.04):
 *
 *   ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
 *
 * Property names match column names - no custom mapping needed in this chapter.
 * Column aliases, splitOn, and custom type handlers are ch04 Mapping.
 *
 * init-only properties work: Dapper sets them via reflection during materialization.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int StockQuantity { get; init; }
    public DateTime? DiscontinuedDate { get; init; }
}
