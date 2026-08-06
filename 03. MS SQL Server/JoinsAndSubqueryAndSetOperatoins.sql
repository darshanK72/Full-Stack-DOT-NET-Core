-- SQL JOINS
-- SQL SUBQUERIES

USE BikeStores;

-- Joins

-- Inner Join -- Gives common recoards

SELECT * FROM sales.customers;
SELECT * FROM sales.orders

SELECT a.customer_id,b.order_id FROM sales.customers as a 
INNER JOIN sales.orders AS b ON a.customer_id = b.customer_id;

SELECT * FROM production.products;
SELECT * FROM production.categories;

-- Inner Join (returns rows which are common in both tables)
SELECT a.product_name,b.category_name,a.list_price
FROM production.products AS a INNER JOIN production.categories AS b
ON a.category_id = b.category_id ORDER BY a.list_price;

-- Left Join (returns rows from left table and joins columns of both table)
SELECT * FROM sales.stores;
SELECT * FROM sales.staffs;

SELECT CONCAT(a.first_name,' ',a.last_name) AS Full_Name, b.store_name FROM sales.staffs AS a
LEFT JOIN sales.stores AS b
ON a.store_id = b.store_id
WHERE a.store_id = 1;

SELECT * FROM production.products;
SELECT * FROM sales.order_items;

SELECT b.order_id,a.product_name,a.product_id FROM production.products AS a
LEFT JOIN sales.order_items AS b
ON a.product_id = b.product_id AND b.order_id = 100;

-- Right Join (returns rows from right table and joins columns of both table)
SELECT CONCAT(a.first_name,' ',a.last_name) AS Full_Name, b.store_name FROM sales.staffs AS a
RIGHT JOIN sales.stores AS b
ON a.store_id = b.store_id;

-- Outer Join (returns all rows from both table by joining columns)
SELECT CONCAT(a.first_name,' ',a.last_name) AS Full_Name, b.store_name FROM sales.staffs AS a
FULL JOIN sales.stores AS b
ON a.store_id = b.store_id AND b.store_id = 1 ;


-- Cross Join (Returns n1 * n2 rows where n1 is number of rows in left table, and n2 is number of tows in right table)
SELECT CONCAT(a.first_name,' ',a.last_name) AS Full_Name, b.store_name FROM sales.staffs AS a
CROSS JOIN sales.stores AS b;


--- Subqueries

SELECT * FROM sales.customers;
SELECT * FROM sales.orders;

-- IN
SELECT order_id,customer_id,order_date FROM sales.orders
WHERE customer_id IN (SELECT customer_id FROM sales.customers WHERE city = 'Hempstead');


-- EXISTS
SELECT customer_id,first_name FROM sales.customers AS a
WHERE EXISTS (SELECT Count(*) 
FROM sales.orders WHERE customer_id = a.customer_id
GROUP BY customer_id 
HAVING COUNT(*) > 2);

-- ANY
SELECT product_id,product_name FROM production.products WHERE product_id = ANY
(SELECT product_id FROM sales.order_items WHERE quantity = 2);

-- ALL
SELECT product_id,product_name FROM production.products WHERE list_price > ALL
(SELECT AVG(list_price) FROM sales.order_items GROUP BY order_id HAVING AVG(list_price) < 200);

SELECT * FROM sales.order_items;
SELECT order_id,AVG(list_price) FROM sales.order_items GROUP BY order_id HAVING AVG(list_price) < 200;
