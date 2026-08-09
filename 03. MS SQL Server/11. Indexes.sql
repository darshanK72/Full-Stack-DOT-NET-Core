/*
 * =============================================================================
 * 11. INDEXES IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Clustered, nonclustered, and unique indexes; disable and rebuild.
 *
 * WHY IT MATTERS:
 *   Indexes speed up seeks and joins on large tables at the cost of extra
 *   storage and slower writes on indexed columns.
 *
 * WHAT YOU WILL LEARN:
 *   1. Clustered vs nonclustered indexes
 *   2. Unique indexes
 *   3. DISABLE and REBUILD
 *
 * Sample database: BikeStores.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: CLUSTERED INDEX
 * =========================================================================
 */
CREATE TABLE sales.teams
(
    teamId   INT,
    teamName VARCHAR(50)
);
GO

CREATE CLUSTERED INDEX teamNameIndex
ON sales.teams (teamName);
GO

INSERT INTO sales.teams (teamId, teamName) VALUES (1, 'Team A');
INSERT INTO sales.teams (teamId, teamName) VALUES (2, 'Team B');
GO

SELECT * FROM sales.teams;
GO

/*
 * =========================================================================
 * SECTION 2: NONCLUSTERED INDEX
 * =========================================================================
 */
CREATE INDEX pnameIndex
ON production.products (product_name);

CREATE INDEX nameIndex
ON sales.customers (first_name, last_name);
GO

/*
 * =========================================================================
 * SECTION 3: UNIQUE INDEX
 * =========================================================================
 */
CREATE UNIQUE INDEX uniqId
ON sales.teams (teamId);
GO

INSERT INTO sales.teams (teamId, teamName) VALUES (3, 'Team C');
GO

SELECT * FROM sales.teams;
GO

/*
 * =========================================================================
 * SECTION 4: DISABLE AND REBUILD
 * =========================================================================
 */
ALTER INDEX nameIndex ON sales.customers DISABLE;

ALTER INDEX ALL ON production.products DISABLE;

ALTER INDEX ALL ON production.products REBUILD;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * Clustered    — one per table; defines row order
 * Nonclustered — many allowed; separate lookup structure
 * UNIQUE       — enforces unique key values
 * ALTER INDEX … DISABLE / REBUILD
 *
 * =============================================================================
 */
