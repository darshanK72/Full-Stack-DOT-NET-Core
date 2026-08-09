using System.Collections.Generic;

namespace RelationshipsAndNavigationProperties.Models;

/*
 * FILE ROLE:
 *   Principal entity in a one-to-many relationship with Order (SECTION 2).
 *
 * SECTIONS IN THIS FILE:
 *   1. Navigation properties  -  reference vs collection
 *   2. One-to-many principal  -  Customer with Orders collection
 */

/*
 * SECTION 1: NAVIGATION PROPERTIES  -  REFERENCE VS COLLECTION
 *
 * Navigation properties are CLR properties that represent associations between entities.
 * They are not stored as columns; EF Core uses them to discover relationships and to
 * traverse graphs in LINQ and change tracking.
 *
 *   Navigation kind   | Example on Customer     | Cardinality (this side)
 *   ------------------|-------------------------|-------------------------
 *   Reference         | (none on Customer here) | one Customer per Order
 *   Collection        | ICollection<Order>      | many Orders per Customer
 *
 * Pair each navigation with a foreign key on the dependent type (Order.CustomerId).
 * EF Core can infer the relationship from navigations + FK property name convention.
 *
 * Pitfall: uninitialized collection navigations throw on .Add  -  initialize to
 * new List<T>() (shown below) or use null-forgiving and let EF populate on load.
 * -------------------------------------------------------------------------
 */

/*
 * SECTION 2: ONE-TO-MANY PRINCIPAL  -  CUSTOMER WITH ORDERS COLLECTION
 *
 * Customer is the "one" side (principal). Order is the "many" side (dependent).
 *
 *   Customer (1) ----< Order (many)
 *
 * Convention discovery (no Fluent API required):
 *   - Order.CustomerId + Order.Customer navigation -> FK on Order
 *   - Customer.Orders collection -> inverse navigation
 *
 * Fluent configuration for delete behavior -> Data/RelationshipsDbContext.cs (SECTION 4).
 * -------------------------------------------------------------------------
 */
public sealed class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = new List<Order>(); // collection navigation  -  one customer, many orders

    public override string ToString() => $"{CustomerId}: {Name} <{Email}>";
}
