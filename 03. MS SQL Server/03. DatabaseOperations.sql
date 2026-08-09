/*
 * =============================================================================
 * 03. DATABASE OPERATIONS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Creating, listing, switching, and dropping databases on a SQL Server
 *        instance.
 *
 * WHY IT MATTERS:
 *   Databases are the top-level containers for tables, views, and procedures.
 *   You must know how to create a workspace and switch context with USE.
 *
 * WHAT YOU WILL LEARN:
 *   1. CREATE DATABASE and DROP DATABASE
 *   2. USE to set the active database
 *   3. Listing databases from system catalog views
 *
 * =============================================================================
 */

/*
 * =========================================================================
 * SECTION 1: CREATE AND DROP DATABASE
 * =========================================================================
 */
CREATE DATABASE EmployeeDB;
GO

DROP DATABASE EmployeeDB;
GO

/*
 * =========================================================================
 * SECTION 2: LIST DATABASES ON THE SERVER
 * =========================================================================
 */
USE master;
GO

SELECT name
FROM master.sys.databases
ORDER BY name;
GO

/*
 * =========================================================================
 * SECTION 3: CREATE DATABASE AND SWITCH CONTEXT
 * =========================================================================
 */
CREATE DATABASE CompanyDB;
GO

USE CompanyDB;
GO

SELECT DB_NAME() AS CurrentDatabase;          -- confirms active database
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * CREATE DATABASE name;
 * USE name;
 * DROP DATABASE name;
 * SELECT name FROM master.sys.databases;
 *
 * =============================================================================
 */
