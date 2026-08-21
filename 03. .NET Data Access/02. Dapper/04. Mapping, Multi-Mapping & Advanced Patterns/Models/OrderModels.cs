using System;
using System.Collections.Generic;

namespace DapperMappingAndAdvancedPatterns.Models;

/*
 * FILE ROLE:
 *   Customer and Order types for multi-mapping demos (SECTIONS 3–5).
 *
 * SECTIONS IN THIS FILE:
 *   3. Multi-mapping types — Order + Customer (one-to-one join)
 *   5. One-to-many aggregate — Order with Lines collection
 */

/*
 * SECTION 3: MULTI-MAPPING TYPES — ORDER + CUSTOMER
 *
 * Query<Order, Customer, Order> splits a wide JOIN row into two objects.
 * Order.Customer is filled in the map delegate (see Repositories/MultiMapRepository.cs).
 * -------------------------------------------------------------------------
 */
public sealed class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer? Customer { get; set; }
    public IList<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

/*
 * SECTION 5: ONE-TO-MANY — ORDER LINE ROW TYPE
 *
 * OrderLines rows repeat the parent Order columns in a JOIN result set.
 * Multi-map + Dictionary<int, Order> collapses duplicate parent rows (SECTION 5).
 * -------------------------------------------------------------------------
 */
public sealed class OrderLine
{
    public int OrderLineId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
