-- SQL Data Manipulation Language (DML)
-- Operations
-- Functions
-- Group By & Having


USE BikeStores;

-- SELECT 
SELECT * FROM production.products;
SELECT product_id,product_name FROM production.products;

-- WHERE
SELECT * FROM production.products
WHERE brand_id = 8 AND model_year = 2016;

-- ORDER BY
SELECT product_id,product_name,brand_id FROM production.products
ORDER BY brand_id; -- Order by brand id Ascending

SELECT product_id,product_name,brand_id FROM production.products
ORDER BY brand_id DESC; -- Orderby  brand id Descending

SELECT product_id,product_name,brand_id FROM production.products
ORDER BY brand_id,product_id; -- Order by both brand_id & product id

SELECT product_id,product_name,brand_id FROM production.products
ORDER BY LEN(product_name); -- Order by length of product name

-- FETCH & OFFSET

SELECT * FROM production.products
ORDER BY product_id
OFFSET 10 ROWS -- OFFSET IS COMPULSERY
FETCH NEXT 10 ROWS ONLY; -- FETCH IS OPTIONAL

SELECT * FROM production.products
ORDER BY product_id
OFFSET 5 ROWS;

-- TOP & PERCENT
SELECT TOP 10 * FROM production.products;

SELECT TOP 5 PERCENT * FROM production.products;

SELECT TOP 1 * FROM production.products
ORDER BY list_price DESC;

-- DISTINCT
SELECT DISTINCT brand_id FROM production.products;

SELECT DISTINCT category_id FROM production.products;


-- Operations

-- IS NULL,IS NOT NULL
SELECT * FROM sales.customers WHERE phone IS NULL;

SELECT * FROM sales.customers WHERE phone IS NOT NULL;

-- AND
SELECT * FROM production.products
WHERE brand_id > 3 AND category_id > 3;

-- OR
SELECT * FROM production.products
WHERE brand_id = 2 OR brand_id = 8;

SELECT * FROM production.products
WHERE (brand_id = 9 AND category_id = 6) OR list_price > 24000;

-- IN
SELECT * FROM production.products
WHERE category_id IN (2,3,7,9);

-- BETWEEN
SELECT * FROM production.products
WHERE list_price BETWEEN 400 AND 900;

-- LIKE & NOT LIKE
-- Wild Card : 
-- 1) % - any length of string
-- 2) _ - single character
-- 3) [LIST OF CHARACTER] - optional characters
-- 4) [CHARACTER - CHARACTER] - range of character

SELECT * FROM sales.customers
WHERE first_name LIKE '%m%'; -- m in between

SELECT * FROM sales.customers
WHERE first_name LIKE 'M%'; -- starts with M

SELECT * FROM sales.customers
WHERE first_name LIKE '%m'; -- ends with m

SELECT * FROM sales.customers
WHERE last_name LIKE '%p_'; -- last second character is p

SELECT * FROM sales.customers
WHERE first_name LIKE '[PST]%'; -- starts with P or S or T

SELECT * FROM sales.customers
WHERE first_name LIKE '[J-N]%'; -- starts with range of character from J to N

SELECT * FROM sales.customers
WHERE first_name NOT LIKE '%a%' AND last_name NOT LIKE '%a%'; -- not have a in first name and last name

-- Alias
SELECT first_name AS [Name] , email AS Email_id FROM sales.customers; -- alias for columns


-- String Functions

-- ASCII(CHAR) & CHAR(VLAUE)
SELECT ASCII('a') AS AsciiValue;
SELECT ASCII('m') AS AsciiValue;
SELECT ASCII('J') AS AsciiValue;
SELECT ASCII('3') AS AsciiValue;
SELECT ASCII('*') AS AsciiValue;

SELECT CHAR(104) AS CharValue;

-- CHARINDEX(SUBSTR,STR,INDEX)
SELECT CHARINDEX('hello','hello this is my world') AS IndexValue;
SELECT CHARINDEX('my','hello this is my world') AS IndexValue;
SELECT CHARINDEX('hello','hello this is my world, again hello',6) AS IndexValue;

-- CONCAT(STR1,STR2) & CONCAT_WS(CHAR,STR1,STR2)
SELECT CONCAT(first_name,' ',last_name) AS Full_Name FROM sales.customers;
SELECT CONCAT_WS('&&',first_name,last_name) AS Full_Name FROM sales.customers;

-- LEFT(STR,LENGTH) & RIGHT(STR,LENGTH)
SELECT LEFT('Hello World, this is Sparta',10) AS StartingString;
SELECT RIGHT('Hello World, This is Sparta',7) AS EndString;


-- LEN(STR)
SELECT LEN('Hello World, this is Sparta') AS LengthOfString;

-- LOWER(STR) & UPPER(STR)
SELECT LOWER('hHELLO THIS IS SPARTA') AS LowerString;
SELECT UPPER('hello world this is sparta') AS UpperString;

-- System Functions

-- CAST(VALUE AS DATATYPE)
SELECT CAST(55.23 AS INT) AS CastedValue;
SELECT CAST('2000-07-12 03:45:45' AS DATETIME) AS CastedValue;

-- ISNULL(VAL1,VAL2)
SELECT ISNULL(NULL,'Hello') AS IsNullValue;
SELECT ISNULL('FIRST VALUE','SECOND VALUE') AS IsNullValue;

-- ISNUMERIC(STR)
SELECT ISNUMERIC('523') AS IsNumericValue;
SELECT ISNUMERIC('abc') AS IsNumericValue;


-- Date Functions

-- GETDATE()
SELECT GETDATE() AS CurentDate;
SELECT GETDATE()-1 AS YesterdayDate;
SELECT GETDATE()+1 AS TomarrowDate;

-- DATENAME() & DATEPART()
SELECT DATENAME(year,'2000-12-07') AS YearValue;
SELECT DATENAME(day,'2000-12-07') AS DayValue;
SELECT DATENAME(month,'2000-12-07') AS MonthValue;
SELECT DATENAME(dayofyear,'2000-12-07') AS DayOfYearValue;
SELECT DATENAME(week,'2000-12-07') AS WeekValue;
SELECT DATENAME(weekday,'2000-12-07') AS WeekDayValue;
SELECT DATEPART(hour,'2000-12-07 03:45:23') AS HourValue;
SELECT DATEPART(minute,'2000-12-07 03:45:23') AS MinuteValue;
SELECT DATEPART(second,'2000-12-07 03:45:23') AS SecondValue;

-- DAY(), MONTH() & YEAR()
SELECT DAY('2000-12-07') AS DateValue;
SELECT MONTH('2000-12-07') AS MonthValue;
SELECT YEAR('2000-12-07') AS YearValue;

-- Aggregate Functions

-- AVG()
SELECT AVG(list_price) AS AvgPrice FROM production.products;
SELECT AVG(DISTINCT category_id) AS AvgCategoryVal FROM production.products;

-- ROUND()
SELECT ROUND(AVG(list_price),2) AS AvgPrice FROM production.products;
SELECT CAST(ROUND(AVG(list_price),2) AS DECIMAL(10,2)) AS AvgPrice FROM production.products;

-- SUM()
SELECT SUM(category_id) AS SumPrice FROM production.products;
SELECT SUM(list_price) AS SumPrice FROM production.products;

-- COUNT()
SELECT COUNT(*) AS CountCategory FROM production.products WHERE category_id = 3;

-- MIN() & MAX()
SELECT MAX(list_price) AS MaxPrice FROM production.products;
SELECT MIN(list_price) AS MinPrice FROM production.products;

-- GROUP BY & HAVING
SELECT category_id FROM production.products GROUP BY category_id;
SELECT product_id,category_id FROM production.products GROUP BY product_id,category_id;

SELECT * FROM sales.customers;
SELECT city FROM SALES.customers GROUP BY city;
SELECT city,COUNT(customer_id) AS TotalPeople FROM sales.customers GROUP BY city ORDER BY city;
SELECT city,state,COUNT(customer_id) AS TotalPeople FROM sales.customers GROUP BY city,state ORDER BY city;

SELECT * FROM sales.order_items;
SELECT order_id,MAX(list_price) AS MaxValPrice,MIN(list_price) AS MinValPrice FROM sales.order_items
GROUP BY order_id ORDER BY order_id;

SELECT order_id, COUNT(product_id) AS TotalProducts FROM sales.order_items
GROUP BY order_id HAVING COUNT(product_id) > 3 ORDER BY TotalProducts;