/*
 * =============================================================================
 * 09. JOINS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: INNER, LEFT, RIGHT, FULL OUTER, and CROSS JOIN with table aliases.
 *
 * WHY IT MATTERS:
 *   Relational data is split across tables. Joins connect related rows for
 *   reports, APIs, and ad-hoc queries.
 *
 * WHAT YOU WILL LEARN:
 *   1. INNER JOIN — matching rows only
 *   2. LEFT JOIN — all left rows preserved
 *   3. RIGHT JOIN — all right rows preserved
 *   4. FULL OUTER JOIN — all rows from both sides
 *   5. CROSS JOIN — Cartesian product
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: INNER JOIN
 * =========================================================================
 */
SELECT a.customer_id, b.order_id
FROM sales.customers AS a
INNER JOIN sales.orders AS b
    ON a.customer_id = b.customer_id;

SELECT
    a.product_name,
    b.category_name,
    a.list_price
FROM production.products AS a
INNER JOIN production.categories AS b
    ON a.category_id = b.category_id
ORDER BY a.list_price;
GO

/*
 * =========================================================================
 * SECTION 2: LEFT JOIN
 * =========================================================================
 */
SELECT
    CONCAT(a.first_name, ' ', a.last_name) AS Full_Name,
    b.store_name
FROM sales.staffs AS a
LEFT JOIN sales.stores AS b
    ON a.store_id = b.store_id
WHERE a.store_id = 1;

SELECT
    b.order_id,
    a.product_name,
    a.product_id
FROM production.products AS a
LEFT JOIN sales.order_items AS b
    ON a.product_id = b.product_id AND b.order_id = 100;
GO

/*
 * =========================================================================
 * SECTION 3: RIGHT JOIN
 * =========================================================================
 */
SELECT
    CONCAT(a.first_name, ' ', a.last_name) AS Full_Name,
    b.store_name
FROM sales.staffs AS a
RIGHT JOIN sales.stores AS b
    ON a.store_id = b.store_id;
GO

/*
 * =========================================================================
 * SECTION 4: FULL OUTER JOIN
 * =========================================================================
 */
SELECT
    CONCAT(a.first_name, ' ', a.last_name) AS Full_Name,
    b.store_name
FROM sales.staffs AS a
FULL JOIN sales.stores AS b
    ON a.store_id = b.store_id AND b.store_id = 1;
GO

/*
 * =========================================================================
 * SECTION 5: CROSS JOIN
 * =========================================================================
 */
SELECT
    CONCAT(a.first_name, ' ', a.last_name) AS Full_Name,
    b.store_name
FROM sales.staffs AS a
CROSS JOIN sales.stores AS b;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * INNER JOIN  — matches only
 * LEFT JOIN   — all left + matched right
 * RIGHT JOIN  — all right + matched left
 * FULL JOIN   — all from both sides
 * CROSS JOIN  — every row × every row (no ON)
 *
 * =============================================================================
 */
