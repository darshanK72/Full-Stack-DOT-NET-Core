/*
 * =============================================================================
 * 10. SUBQUERIES IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Nested queries with IN, EXISTS, ANY, and ALL.
 *
 * WHY IT MATTERS:
 *   Subqueries embed one SELECT inside another for filtering, existence checks,
 *   and comparisons against a derived set of values.
 *
 * WHAT YOU WILL LEARN:
 *   1. Subquery with IN
 *   2. Correlated subquery with EXISTS
 *   3. ANY and ALL comparisons
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: SUBQUERY WITH IN
 * =========================================================================
 */
SELECT order_id, customer_id, order_date
FROM sales.orders
WHERE customer_id IN (
    SELECT customer_id
    FROM sales.customers
    WHERE city = 'Hempstead'
);
GO

/*
 * =========================================================================
 * SECTION 2: SUBQUERY WITH EXISTS
 * =========================================================================
 */
SELECT customer_id, first_name
FROM sales.customers AS a
WHERE EXISTS (
    SELECT COUNT(*)
    FROM sales.orders
    WHERE customer_id = a.customer_id
    GROUP BY customer_id
    HAVING COUNT(*) > 2
);
GO

/*
 * =========================================================================
 * SECTION 3: SUBQUERY WITH ANY
 * =========================================================================
 */
SELECT product_id, product_name
FROM production.products
WHERE product_id = ANY (
    SELECT product_id
    FROM sales.order_items
    WHERE quantity = 2
);
GO

/*
 * =========================================================================
 * SECTION 4: SUBQUERY WITH ALL
 * =========================================================================
 */
SELECT product_id, product_name
FROM production.products
WHERE list_price > ALL (
    SELECT AVG(list_price)
    FROM sales.order_items
    GROUP BY order_id
    HAVING AVG(list_price) < 200
);
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * IN (subquery)      — value in result set
 * EXISTS (subquery)  — at least one row (often correlated)
 * = ANY (subquery)   — equal to any value
 * > ALL (subquery)   — greater than every value
 *
 * =============================================================================
 */
