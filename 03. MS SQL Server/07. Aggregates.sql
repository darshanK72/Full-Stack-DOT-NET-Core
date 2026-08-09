/*
 * =============================================================================
 * 07. AGGREGATES IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Aggregate functions — AVG, SUM, COUNT, MIN, MAX, ROUND, DISTINCT.
 *
 * WHY IT MATTERS:
 *   Aggregates collapse many rows into summary values — totals, averages,
 *   counts — the basis of reporting and analytics.
 *
 * WHAT YOU WILL LEARN:
 *   1. AVG, SUM, COUNT, MIN, MAX
 *   2. DISTINCT inside aggregates
 *   3. ROUND and CAST on aggregate results
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: AVG AND DISTINCT AVG
 * =========================================================================
 */
SELECT AVG(list_price) AS AvgPrice
FROM production.products;

SELECT AVG(DISTINCT category_id) AS AvgCategoryVal
FROM production.products;
GO

/*
 * =========================================================================
 * SECTION 2: ROUND AND CAST ON AGGREGATES
 * =========================================================================
 */
SELECT ROUND(AVG(list_price), 2) AS AvgPriceRounded
FROM production.products;

SELECT CAST(ROUND(AVG(list_price), 2) AS DECIMAL(10, 2)) AS AvgPriceDecimal
FROM production.products;
GO

/*
 * =========================================================================
 * SECTION 3: SUM, COUNT, MIN, MAX
 * =========================================================================
 */
SELECT SUM(list_price) AS SumPrice
FROM production.products;

SELECT COUNT(*) AS RowCount
FROM production.products
WHERE category_id = 3;

SELECT MAX(list_price) AS MaxPrice,
       MIN(list_price) AS MinPrice
FROM production.products;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * AVG(col)  SUM(col)  COUNT(*)  COUNT(col)  MIN(col)  MAX(col)
 * AVG(DISTINCT col) — unique values only
 *
 * =============================================================================
 */
