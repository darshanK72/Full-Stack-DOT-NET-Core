/*
 * =============================================================================
 * 12. VIEWS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: CREATE VIEW — saved SELECT queries queried like tables.
 *
 * WHY IT MATTERS:
 *   Views hide complex joins and aggregations behind a simple name. Applications
 *   and reports can SELECT from a view instead of repeating long queries.
 *
 * WHAT YOU WILL LEARN:
 *   1. Simple view with aggregation
 *   2. View with joins and grouped columns
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: SIMPLE AGGREGATE VIEW
 * =========================================================================
 */
CREATE VIEW sales.orders_items
AS
SELECT
    order_id,
    SUM(list_price) AS total_price
FROM sales.order_items
GROUP BY order_id;
GO

SELECT * FROM sales.orders_items;
GO

/*
 * =========================================================================
 * SECTION 2: VIEW WITH JOINS
 * =========================================================================
 */
CREATE VIEW sales.staff_sales
(
    first_name,
    last_name,
    [year],
    amount
)
AS
SELECT
    s.first_name,
    s.last_name,
    YEAR(o.order_date),
    SUM(i.list_price * i.quantity) AS amount
FROM sales.order_items AS i
INNER JOIN sales.orders AS o ON i.order_id = o.order_id
INNER JOIN sales.staffs AS s ON s.staff_id = o.staff_id
GROUP BY s.first_name, s.last_name, YEAR(o.order_date);
GO

SELECT * FROM sales.staff_sales;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * CREATE VIEW schema.name AS SELECT …;
 * SELECT * FROM schema.name;
 *
 * =============================================================================
 */
