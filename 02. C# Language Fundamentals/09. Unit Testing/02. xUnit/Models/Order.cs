using System.Collections.Generic;

namespace XUnit.Models;

/*
 * SECTION 2: ORDER — HEADER PLUS LINE COLLECTION
 *
 * Order groups an OrderId with a read-only list of lines. IReadOnlyList<T>
 * prevents callers from mutating the collection after construction — tests can
 * still pass List<OrderLine> because List<T> implements IReadOnlyList<T>.
 */
public sealed class Order
{
    public required string OrderId { get; init; }

    public required IReadOnlyList<OrderLine> Lines { get; init; }
}
