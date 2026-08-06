-- Index
-- Views
-- Stored Procedures
-- Triggers

-- INDEX
USE BikeStores;

SELECT * FROM production.products;

SELECT product_name FROM production.products;

-- Clustered Index
CREATE TABLE sales.teams(
	teamId int,
	teamName varchar(50)

);

CREATE CLUSTERED INDEX teamNameIndex
ON sales.teams(teamName);

INSERT INTO sales.teams(teamId,teamName) VALUES(1,'Team A');
INSERT INTO sales.teams(teamId,teamName) VALUES(1,'Team A');

SELECT * FROM sales.teams;

-- Unclustred Index
CREATE INDEX pnameIndex
ON production.products(product_name);

CREATE INDEX nameIndex
ON sales.customers(first_name,last_name)

SELECT customer_id,first_name,last_name FROM sales.customers;

-- UNIQUE INDEX
DELETE FROM sales.teams WHERE teamId IN(SELECT teamId FROM sales.teams GROUP BY teamId HAVING COUNT(teamId) > 1);

CREATE UNIQUE INDEX uniqId
ON sales.teams(teamId);

INSERT INTO sales.teams(teamId,teamName) VALUES(1,'Team A');
INSERT INTO sales.teams(teamId,teamName) VALUES(1,'Team A'); -- cannot be inserted because of unique index
INSERT INTO sales.teams(teamId,teamName) VALUES(2,'Team B');
INSERT INTO sales.teams(teamId,teamName) VALUES(3,'Team C');

SELECT * FROM sales.teams;


-- DISABLE INDEX
ALTER INDEX nameIndex
ON sales.customers
DISABLE;

ALTER INDEX ALL 
ON production.products
DISABLE;

ALTER INDEX ALL
ON production.products
REBUILD;


-- VIEWS
SELECT * FROM sales.orders;
SELECT * FROM sales.order_items;

CREATE VIEW sales.orders_items(
	order_id,
	total_price
)
AS
SELECT order_id,SUM(list_price)
FROM sales.order_items
GROUP BY order_id;

SELECT * FROM sales.orders_items;

CREATE VIEW sales.staff_sales (
        first_name, 
        last_name,
        year, 
        amount
)
AS 
    SELECT 
        first_name,
        last_name,
        YEAR(order_date),
        SUM(list_price * quantity) amount
    FROM
        sales.order_items i
    INNER JOIN sales.orders o
        ON i.order_id = o.order_id
    INNER JOIN sales.staffs s
        ON s.staff_id = o.staff_id
    GROUP BY 
        first_name, 
        last_name, 
        YEAR(order_date);

SELECT * FROM sales.staff_sales;

-- Stored Procedures

-- CREATE PROCEDUCE
CREATE PROCEDURE myProcedure
AS
BEGIN
	SELECT * FROM sales.order_items
	WHERE product_id IN (2,3,4);
END

EXECUTE myProcedure;

DROP PROCEDURE myProcedure;

-- PARAMETER IN STORED PROCEDUCE

CREATE PROCEDURE averageOrderPrice(@orde_id AS INT)
AS
BEGIN
	SELECT order_id,AVG(list_price) AS Average_Price FROM sales.order_items
	GROUP BY order_id HAVING order_id = @orde_id;
END

DROP PROCEDURE averageOrderPrice;

EXEC averageOrderPrice 2;

-- MULTIPLE PARAMETERS

CREATE PROCEDURE uspFindProducts(
    @min_list_price AS DECIMAL
    ,@max_list_price AS DECIMAL
)
AS
BEGIN
    SELECT product_name,list_price
    FROM production.products
    WHERE list_price >= @min_list_price AND list_price <= @max_list_price
    ORDER BY list_price;
END;

EXEC uspFindProducts 200.0,900.0;


-- VARIABLES

DECLARE @model_year SMALLINT;

SET @model_year = 2018;

SELECT
    product_name,
    model_year,
    list_price 
FROM 
    production.products
WHERE 
    model_year = @model_year
ORDER BY
    product_name;
