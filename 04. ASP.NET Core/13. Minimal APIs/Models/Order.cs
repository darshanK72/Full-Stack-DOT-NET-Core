/*
 * FILE ROLE: Defines the Order entity and CreateOrderRequest DTO used by
 *            OrderEndpoints to demonstrate async handlers, [FromHeader] binding,
 *            and [FromServices] explicit injection in a second route group.
 * SECTIONS IN THIS FILE:
 *   5a. Order entity          — response model returned by order endpoints
 *   5b. CreateOrderRequest    — DTO for POST /orders (body + header bound)
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace MinimalApis.Models;

/*
 * SECTION 5a: ORDER ENTITY
 *
 * Represents a placed order referencing a Product by ProductId.
 * Status is a free-form string kept simple for tutorial purposes; a real system
 * would use an enum (Pending, Confirmed, Shipped, Delivered, Cancelled).
 *
 * DateTime.UtcNow as the default for CreatedAt is fine for an entity but would
 * be a test-unfriendly default in a service — inject IDateTimeProvider there.
 */
public sealed class Order
{
    public int Id { get; set; }                               // server-assigned
    public int ProductId { get; set; }                        // FK to Product
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }                   // computed at creation
    public string Status { get; set; } = "Pending";          // initialized default
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CustomerEmail { get; set; } = string.Empty;
}

/*
 * SECTION 5b: CREATEORDERREQUEST — POST /orders BODY DTO
 *
 * Used in OrderEndpoints to show two binding sources on the SAME endpoint:
 *   [FromBody]   — ProductId, Quantity, CustomerEmail come from the JSON body
 *   [FromHeader] — X-Customer-Id comes from a request header (shown separately)
 *
 * [EmailAddress] is a DataAnnotations attribute that validates RFC 5322 format.
 * It is recognized by ValidationEndpointFilter just like [Required] and [Range].
 */
public sealed class CreateOrderRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
    public int ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000.")]
    public int Quantity { get; set; }

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;
}
