using System;
using System.Collections.Generic;

namespace LoadingRelatedData.Models;

/*
 * FILE ROLE:
 *   Order aggregate entities with navigation properties for eager, explicit, and filtered loading demos.
 *
 * SECTIONS IN THIS FILE:
 *   1. Customer - many orders (one-to-many parent)
 *   2. Order - Customer reference + Lines collection
 *   3. OrderLine - child row; Product navigation for ThenInclude chain
 *   4. Product - minimal type for ThenInclude (maps to dbo.Products)
 */

/*
 * SECTION 1: CUSTOMER - ONE-TO-MANY PARENT
 *
 * Customer.Orders is the inverse navigation (optional for loading demos).
 * FK lives on Order.CustomerId (configured in Data/ShopDbContext.cs).
 *
 * Relationship basics: COVERED IN DETAIL -> 06. Relationships & Navigation Properties
 * -------------------------------------------------------------------------
 */
public sealed class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

/*
 * SECTION 2: ORDER - NAVIGATION TO CUSTOMER AND LINES
 *
 * Order.Customer  - reference navigation (many-to-one)
 * Order.Lines     - collection navigation (one-to-many)
 *
 * Without Include / explicit Load / lazy loading, these navigations are null or empty
 * when the entity is materialized from a query that only selected dbo.Orders columns.
 * -------------------------------------------------------------------------
 */
public sealed class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

/*
 * SECTION 3: ORDER LINE - CHILD ENTITY + PRODUCT NAVIGATION
 *
 * OrderLine.Product enables ThenInclude(o => o.Lines).ThenInclude(l => l.Product).
 * ProductId FK column matches dbo.OrderLines.ProductId (no FK constraint in seed DB).
 * -------------------------------------------------------------------------
 */
public sealed class OrderLine
{
    public int OrderLineId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}

/*
 * SECTION 4: PRODUCT - MINIMAL ENTITY FOR ThenInclude
 *
 * Only fields needed to prove the nested Include chain; full catalog modeling is out of scope.
 * -------------------------------------------------------------------------
 */
public sealed class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
