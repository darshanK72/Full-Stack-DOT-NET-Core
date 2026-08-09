using System;

namespace RelationshipsAndNavigationProperties.Models;

/*
 * FILE ROLE:
 *   Dependent entity in the Customer-Order one-to-many relationship (SECTION 3).
 *
 * SECTIONS IN THIS FILE:
 *   3. Foreign key scalar + reference navigation on the many side
 */

/*
 * SECTION 3: FOREIGN KEY SCALAR + REFERENCE NAVIGATION ON THE MANY SIDE
 *
 * The dependent entity holds:
 *   1. FK scalar      -  CustomerId (maps to dbo.Orders.CustomerId column)
 *   2. Reference nav  -  Customer (points back to the principal row)
 *
 * EF Core pairs CustomerId with Customer by convention ({Principal}Id).
 *
 *   Column / property   | Role
 *   --------------------|--------------------------------------------------
 *   CustomerId (int)    | FK scalar  -  stored in the database
 *   Customer (Customer) | reference navigation  -  not a column; used in LINQ/graph ops
 *
 * Required relationship: non-nullable CustomerId + non-nullable Customer navigation
 * (null! tells the compiler EF will set it when materializing from the database).
 *
 * Insert via navigation graph (no need to set CustomerId manually):
 *   var customer = new Customer { Orders = { new Order { ... } } };
 *   context.Customers.Add(customer); // EF sets Order.CustomerId on SaveChanges
 *
 * Full Include / ThenInclude loading -> COVERED IN DETAIL LATER -> 09. Loading Related Data.
 * -------------------------------------------------------------------------
 */
public sealed class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }              // FK scalar  -  convention: matches Customer navigation name + Id
    public DateOnly OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer Customer { get; set; } = null!; // reference navigation  -  required side of one-to-many

    public override string ToString() =>
        $"Order {OrderId} on {OrderDate:d}  -  ${TotalAmount:F2} (CustomerId {CustomerId})";
}
