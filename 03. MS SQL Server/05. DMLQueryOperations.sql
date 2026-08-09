/*
 * =============================================================================
 * 05. DML QUERY OPERATIONS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: SELECT, WHERE, ORDER BY, TOP, OFFSET/FETCH, DISTINCT, predicates,
 *        and column aliases.
 *
 * WHY IT MATTERS:
 *   SELECT is how applications read data. Filtering and sorting control which
 *   rows return and in what order.
 *
 * WHAT YOU WILL LEARN:
 *   1. Basic SELECT and column lists
 *   2. WHERE with AND, OR, IN, BETWEEN, LIKE
 *   3. IS NULL / IS NOT NULL
 *   4. ORDER BY, TOP, OFFSET/FETCH, DISTINCT
 *   5. Column aliases
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: BASIC SELECT
 * =========================================================================
 */
SELECT * FROM production.products;

SELECT product_id, product_name
FROM production.products;
GO

/*
 * =========================================================================
 * SECTION 2: WHERE — FILTER ROWS
 * =========================================================================
 */
SELECT *
FROM production.products
WHERE brand_id = 8 AND model_year = 2016;
GO

/*
 * =========================================================================
 * SECTION 3: ORDER BY
 * =========================================================================
 */
SELECT product_id, product_name, brand_id
FROM production.products
ORDER BY brand_id;

SELECT product_id, product_name, brand_id
FROM production.products
ORDER BY brand_id DESC;

SELECT product_id, product_name, brand_id
FROM production.products
ORDER BY brand_id, product_id;

SELECT product_id, product_name, brand_id
FROM production.products
ORDER BY LEN(product_name);
GO

/*
 * =========================================================================
 * SECTION 4: OFFSET AND FETCH — PAGING
 * =========================================================================
 */
SELECT *
FROM production.products
ORDER BY product_id
OFFSET 10 ROWS
FETCH NEXT 10 ROWS ONLY;

SELECT *
FROM production.products
ORDER BY product_id
OFFSET 5 ROWS;
GO

/*
 * =========================================================================
 * SECTION 5: TOP
 * =========================================================================
 */
SELECT TOP 10 *
FROM production.products;

SELECT TOP 5 PERCENT *
FROM production.products;

SELECT TOP 1 *
FROM production.products
ORDER BY list_price DESC;
GO

/*
 * =========================================================================
 * SECTION 6: DISTINCT
 * =========================================================================
 */
SELECT DISTINCT brand_id
FROM production.products;

SELECT DISTINCT category_id
FROM production.products;
GO

/*
 * =========================================================================
 * SECTION 7: NULL CHECKS
 * =========================================================================
 */
SELECT *
FROM sales.customers
WHERE phone IS NULL;

SELECT *
FROM sales.customers
WHERE phone IS NOT NULL;
GO

/*
 * =========================================================================
 * SECTION 8: AND, OR, PARENTHESES
 * =========================================================================
 */
SELECT *
FROM production.products
WHERE brand_id > 3 AND category_id > 3;

SELECT *
FROM production.products
WHERE brand_id = 2 OR brand_id = 8;

SELECT *
FROM production.products
WHERE (brand_id = 9 AND category_id = 6) OR list_price > 24000;
GO

/*
 * =========================================================================
 * SECTION 9: IN
 * =========================================================================
 */
SELECT *
FROM production.products
WHERE category_id IN (2, 3, 7, 9);
GO

/*
 * =========================================================================
 * SECTION 10: BETWEEN
 * =========================================================================
 */
SELECT *
FROM production.products
WHERE list_price BETWEEN 400 AND 900;
GO

/*
 * =========================================================================
 * SECTION 11: LIKE — PATTERN MATCHING
 * =========================================================================
 */
SELECT *
FROM sales.customers
WHERE first_name LIKE '%m%';

SELECT *
FROM sales.customers
WHERE first_name LIKE 'M%';

SELECT *
FROM sales.customers
WHERE first_name LIKE '%m';

SELECT *
FROM sales.customers
WHERE last_name LIKE '%p_';

SELECT *
FROM sales.customers
WHERE first_name LIKE '[PST]%';

SELECT *
FROM sales.customers
WHERE first_name LIKE '[J-N]%';

SELECT *
FROM sales.customers
WHERE first_name NOT LIKE '%a%' AND last_name NOT LIKE '%a%';
GO

/*
 * =========================================================================
 * SECTION 12: COLUMN ALIASES
 * =========================================================================
 */
SELECT
    first_name AS [Name],
    email      AS Email_id
FROM sales.customers;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * SELECT cols FROM t WHERE … ORDER BY col;
 * TOP n | OFFSET n ROWS FETCH NEXT m ROWS ONLY
 * DISTINCT | IS NULL | IN | BETWEEN | LIKE
 *
 * =============================================================================
 */
