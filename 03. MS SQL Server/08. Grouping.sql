/*
 * =============================================================================
 * 08. GROUPING IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: GROUP BY and HAVING — summarize rows per group and filter groups.
 *
 * WHY IT MATTERS:
 *   GROUP BY produces one row per category (city, order, product type).
 *   HAVING filters those summaries — e.g. orders with more than three items.
 *
 * WHAT YOU WILL LEARN:
 *   1. GROUP BY single and multiple columns
 *   2. Aggregates with GROUP BY
 *   3. HAVING vs WHERE
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: GROUP BY — ONE ROW PER GROUP
 * =========================================================================
 *
 * Every non-aggregated column in SELECT must appear in GROUP BY.
 * -------------------------------------------------------------------------
 */
SELECT category_id
FROM production.products
GROUP BY category_id;

SELECT city, COUNT(customer_id) AS TotalPeople
FROM sales.customers
GROUP BY city
ORDER BY city;

SELECT city, state, COUNT(customer_id) AS TotalPeople
FROM sales.customers
GROUP BY city, state
ORDER BY city;
GO

/*
 * =========================================================================
 * SECTION 2: MULTIPLE AGGREGATES PER GROUP
 * =========================================================================
 */
SELECT
    order_id,
    MAX(list_price) AS MaxValPrice,
    MIN(list_price) AS MinValPrice
FROM sales.order_items
GROUP BY order_id
ORDER BY order_id;
GO

/*
 * =========================================================================
 * SECTION 3: HAVING — FILTER GROUPS
 * =========================================================================
 *
 * WHERE filters rows before grouping; HAVING filters after aggregation.
 * -------------------------------------------------------------------------
 */
SELECT
    order_id,
    COUNT(product_id) AS TotalProducts
FROM sales.order_items
GROUP BY order_id
HAVING COUNT(product_id) > 3
ORDER BY TotalProducts;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * SELECT col, AGG(col2) FROM t GROUP BY col;
 * HAVING condition_on_aggregate   — after GROUP BY
 * WHERE condition_on_row        — before GROUP BY
 *
 * =============================================================================
 */
