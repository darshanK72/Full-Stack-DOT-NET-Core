/*
 * =============================================================================
 * 06. FUNCTIONS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Scalar functions — string, system, and date/time transformations.
 *
 * WHY IT MATTERS:
 *   Functions shape data row-by-row in SELECT lists and WHERE clauses —
 *   formatting names, parsing dates, and handling NULLs.
 *
 * WHAT YOU WILL LEARN:
 *   1. String functions — ASCII, CHAR, CHARINDEX, CONCAT, LEFT, RIGHT, LEN
 *   2. System functions — CAST, ISNULL, ISNUMERIC
 *   3. Date functions — GETDATE, DATENAME, DATEPART, DAY, MONTH, YEAR
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: ASCII AND CHAR
 * =========================================================================
 */
SELECT ASCII('a') AS AsciiValue;
SELECT CHAR(104)  AS CharValue;
GO

/*
 * =========================================================================
 * SECTION 2: CHARINDEX
 * =========================================================================
 */
SELECT CHARINDEX('hello', 'hello this is my world') AS IndexValue;
SELECT CHARINDEX('my', 'hello this is my world') AS IndexValue;
GO

/*
 * =========================================================================
 * SECTION 3: CONCAT AND CONCAT_WS
 * =========================================================================
 */
SELECT CONCAT(first_name, ' ', last_name) AS Full_Name
FROM sales.customers;

SELECT CONCAT_WS('&&', first_name, last_name) AS Full_Name
FROM sales.customers;
GO

/*
 * =========================================================================
 * SECTION 4: LEFT, RIGHT, LEN, LOWER, UPPER
 * =========================================================================
 */
SELECT LEFT('Hello World, this is Sparta', 10)  AS StartingString;
SELECT RIGHT('Hello World, This is Sparta', 7) AS EndString;
SELECT LEN('Hello World, this is Sparta')      AS LengthOfString;
SELECT LOWER('hHELLO THIS IS SPARTA')          AS LowerString;
SELECT UPPER('hello world this is sparta')     AS UpperString;
GO

/*
 * =========================================================================
 * SECTION 5: CAST
 * =========================================================================
 */
SELECT CAST(55.23 AS INT) AS CastedValue;
SELECT CAST('2000-07-12 03:45:45' AS DATETIME) AS CastedValue;
GO

/*
 * =========================================================================
 * SECTION 6: ISNULL AND ISNUMERIC
 * =========================================================================
 */
SELECT ISNULL(NULL, 'Hello') AS IsNullValue;
SELECT ISNUMERIC('523') AS IsNumericValue;
SELECT ISNUMERIC('abc') AS IsNumericValue;
GO

/*
 * =========================================================================
 * SECTION 7: DATE AND TIME FUNCTIONS
 * =========================================================================
 */
SELECT GETDATE() AS CurrentDate;
SELECT GETDATE() - 1 AS YesterdayDate;

SELECT DATENAME(YEAR, '2000-12-07') AS YearValue;
SELECT DATENAME(MONTH, '2000-12-07') AS MonthValue;
SELECT DATEPART(HOUR, '2000-12-07 03:45:23') AS HourValue;

SELECT DAY('2000-12-07') AS DayPart;
SELECT MONTH('2000-12-07') AS MonthPart;
SELECT YEAR('2000-12-07') AS YearPart;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * Strings: CONCAT, LEFT, RIGHT, LEN, LOWER, UPPER, CHARINDEX, ASCII, CHAR
 * System:  CAST(x AS type), ISNULL(x, default), ISNUMERIC(s)
 * Dates:   GETDATE(), DATENAME, DATEPART, DAY, MONTH, YEAR
 *
 * =============================================================================
 */
