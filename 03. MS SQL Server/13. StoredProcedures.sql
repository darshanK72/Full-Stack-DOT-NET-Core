/*
 * =============================================================================
 * 13. STORED PROCEDURES IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: CREATE PROCEDURE with parameters, EXEC, and T-SQL batch variables.
 *
 * WHY IT MATTERS:
 *   Stored procedures bundle server-side logic — reusable, parameterised, and
 *   often easier to permission than raw ad-hoc SQL from clients.
 *
 * WHAT YOU WILL LEARN:
 *   1. Procedure with no parameters
 *   2. Single and multiple parameters
 *   3. DECLARE and SET for batch variables
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: PROCEDURE WITH NO PARAMETERS
 * =========================================================================
 */
CREATE PROCEDURE myProcedure
AS
BEGIN
    SELECT *
    FROM sales.order_items
    WHERE product_id IN (2, 3, 4);
END;
GO

EXECUTE myProcedure;
GO

DROP PROCEDURE myProcedure;
GO

/*
 * =========================================================================
 * SECTION 2: SINGLE PARAMETER
 * =========================================================================
 */
CREATE PROCEDURE averageOrderPrice (@order_id AS INT)
AS
BEGIN
    SELECT
        order_id,
        AVG(list_price) AS Average_Price
    FROM sales.order_items
    GROUP BY order_id
    HAVING order_id = @order_id;
END;
GO

EXEC averageOrderPrice @order_id = 2;
GO

DROP PROCEDURE averageOrderPrice;
GO

/*
 * =========================================================================
 * SECTION 3: MULTIPLE PARAMETERS
 * =========================================================================
 */
CREATE PROCEDURE uspFindProducts
    @min_list_price AS DECIMAL(10, 2),
    @max_list_price AS DECIMAL(10, 2)
AS
BEGIN
    SELECT product_name, list_price
    FROM production.products
    WHERE list_price >= @min_list_price
      AND list_price <= @max_list_price
    ORDER BY list_price;
END;
GO

EXEC uspFindProducts @min_list_price = 200.0, @max_list_price = 900.0;
GO

/*
 * =========================================================================
 * SECTION 4: T-SQL VARIABLES IN A BATCH
 * =========================================================================
 *
 * DECLARE / SET variables live for one batch — unlike procedure parameters.
 * -------------------------------------------------------------------------
 */
DECLARE @model_year SMALLINT;

SET @model_year = 2018;

SELECT product_name, model_year, list_price
FROM production.products
WHERE model_year = @model_year
ORDER BY product_name;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * CREATE PROCEDURE name @p type AS BEGIN … END;
 * EXEC name @p = value;
 * DROP PROCEDURE name;
 * DECLARE @v type; SET @v = expression;
 *
 * =============================================================================
 */
