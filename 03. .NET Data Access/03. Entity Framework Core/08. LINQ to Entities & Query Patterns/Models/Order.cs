using System;
using System.Collections.Generic;

namespace LinqToEntitiesAndQueryPatterns.Models;

/*
 * FILE ROLE:
 *   Order and OrderLine entities for join-style filtering and SelectMany projection demos.
 *
 * SECTIONS IN THIS FILE:
 *   1. Order entity
 *   2. OrderLine entity
 */

/*
 * SECTION 1: ORDER ENTITY
 *
 * Header row for customer orders. Lines collection enables nested projection
 * (order + line items) without Include  -  covered in depth in ch.09 Loading Related Data.
 * -------------------------------------------------------------------------
 */
public sealed class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

/*
 * SECTION 2: ORDER LINE ENTITY
 *
 * Child row referencing Order and Product by FK. Used when filtering orders
 * that contain a product above a price threshold (Any on navigation collection).
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
