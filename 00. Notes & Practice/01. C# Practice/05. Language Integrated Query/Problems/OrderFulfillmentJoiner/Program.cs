/*
 * PROBLEM: Order Fulfillment Joiner
 *
 * Customer service joins customers to orders and preserves customers who never ordered,
 * with optional shipment tracking on matched orders.
 *
 * This exercise covers:
 *   ch05 — Join, GroupJoin, DefaultIfEmpty, query syntax join equals
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderFulfillment
{
    record Customer(int CustomerId, string Name);
    record OrderHeader(int OrderId, int CustomerId, decimal OrderTotal);
    record Shipment(int OrderId, string TrackingNumber);
    record CustomerOrderRow(string CustomerName, int OrderId, decimal OrderTotal);
    record CustomerWithOptionalShipment(string CustomerName, int? OrderId, string? TrackingNumber);

    /*
     * Joins customer, order, and shipment sequences with inner and left-outer patterns.
     */
    class OrderFulfillmentJoiner
    {
        private readonly IEnumerable<Customer> _customers;
        private readonly IEnumerable<OrderHeader> _orders;
        private readonly IEnumerable<Shipment> _shipments;

        public OrderFulfillmentJoiner(
            IEnumerable<Customer> customers,
            IEnumerable<OrderHeader> orders,
            IEnumerable<Shipment> shipments)
        {
            _customers = customers;
            _orders = orders;
            _shipments = shipments;
        }

        /*
         * Inner join — customers without orders excluded.
         */
        public IEnumerable<CustomerOrderRow> InnerCustomerOrders()
        {
            // TODO: Join customers to orders on CustomerId
            throw new NotImplementedException();
        }

        /*
         * Left outer: all customers; null OrderId when no order.
         */
        public IEnumerable<CustomerWithOptionalShipment> LeftCustomersWithoutOrders()
        {
            // TODO: GroupJoin + DefaultIfEmpty + project nullable fields
            throw new NotImplementedException();
        }

        /*
         * Left outer customer/order with optional shipment tracking.
         */
        public IEnumerable<CustomerWithOptionalShipment> LeftOuterCustomerShipments()
        {
            // TODO: join pipeline including shipment with DefaultIfEmpty
            throw new NotImplementedException();
        }

        /*
         * Query syntax inner join equivalent to InnerCustomerOrders.
         */
        public IEnumerable<CustomerOrderRow> QuerySyntaxInnerJoin()
        {
            // TODO: from c join o on c.CustomerId equals o.CustomerId select ...
            throw new NotImplementedException();
        }

        /*
         * Count customers with no matching order.
         */
        public int CountCustomersWithoutOrders()
        {
            // TODO: LeftCustomersWithoutOrders where OrderId is null
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed customers including one without orders
            // TODO: inner join print — orphan customer absent
            // TODO: left outer with null OrderId
            // TODO: order without shipment → null TrackingNumber
            // TODO: query vs method join counts
            // TODO: CountCustomersWithoutOrders
            throw new NotImplementedException();
        }
    }
}
