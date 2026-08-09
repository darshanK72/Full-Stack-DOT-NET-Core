/*
 * =============================================================================
 * 04. DDL IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Data Definition Language — CREATE / ALTER / DROP TABLE, TRUNCATE,
 *        sp_rename, INSERT/SELECT basics, and schema changes.
 *
 * WHY IT MATTERS:
 *   DDL defines table structure. Applications depend on a correct schema before
 *   any SELECT or INSERT can run reliably.
 *
 * WHAT YOU WILL LEARN:
 *   1. CREATE TABLE with related tables
 *   2. INSERT and SELECT to verify data
 *   3. ALTER TABLE — add, alter, drop columns
 *   4. sp_rename for columns and tables
 *   5. TRUNCATE TABLE vs DROP TABLE
 *
 * Prerequisite: run 03. DatabaseOperations.sql (uses CompanyDB).
 *
 * =============================================================================
 */

USE CompanyDB;
GO

/*
 * =========================================================================
 * SECTION 1: CREATE TABLES
 * =========================================================================
 */
CREATE TABLE Employee
(
    EmployeeId               INT,
    EmployeeName             VARCHAR(50),
    EmployeeAddress          VARCHAR(255),
    EmployeeMobileNumber     VARCHAR(10),
    EmployeeDateOfBirth      DATETIME,
    EmployeeFavChar          CHAR(1),
    EmployeeBaseSalary       MONEY,
    EmployeeProjectId        INT,
    EmployeeProjectManagerId INT,
    EmployeeHrManagerId      INT
);
GO

CREATE TABLE ProjectManager
(
    ManagerId        INT,
    ManagerTeamName  VARCHAR(50),
    ManagerTeamSize  INT,
    ManagerProjectId INT
);
GO

CREATE TABLE HrManager
(
    HrManagerId               INT,
    HrManagerEmployeeId       INT,
    HrManagerSubordinateCount INT
);
GO

/*
 * =========================================================================
 * SECTION 2: INSERT SAMPLE ROWS
 * =========================================================================
 */
INSERT INTO Employee
(
    EmployeeId, EmployeeName, EmployeeAddress, EmployeeMobileNumber,
    EmployeeDateOfBirth, EmployeeFavChar, EmployeeBaseSalary,
    EmployeeProjectId, EmployeeProjectManagerId, EmployeeHrManagerId
)
VALUES (101, 'Darshan Khairnar', 'Nashik', '9834444657', '2000-12-07', 'M', 21000, 11, 301, 401);

INSERT INTO Employee
(
    EmployeeId, EmployeeName, EmployeeAddress, EmployeeMobileNumber,
    EmployeeDateOfBirth, EmployeeFavChar, EmployeeBaseSalary,
    EmployeeProjectId, EmployeeProjectManagerId, EmployeeHrManagerId
)
VALUES (102, 'Aakash Khairnar', 'Nashik', '9834444657', '2001-01-01', 'K', 21000, 11, 301, 401);
GO

/*
 * =========================================================================
 * SECTION 3: SELECT — READ ROWS
 * =========================================================================
 */
SELECT * FROM Employee;
GO

/*
 * =========================================================================
 * SECTION 4: ALTER TABLE — ADD COLUMN
 * =========================================================================
 */
ALTER TABLE ProjectManager
ADD NewAddedColumn CHAR(1);
GO

SELECT column_name, data_type
FROM INFORMATION_SCHEMA.COLUMNS
WHERE table_name = 'ProjectManager';
GO

/*
 * =========================================================================
 * SECTION 5: ALTER TABLE — CHANGE COLUMN TYPE
 * =========================================================================
 */
ALTER TABLE ProjectManager
ALTER COLUMN ManagerId BIGINT;
GO

/*
 * =========================================================================
 * SECTION 6: ALTER TABLE — DROP COLUMN
 * =========================================================================
 */
ALTER TABLE ProjectManager
DROP COLUMN NewAddedColumn;
GO

/*
 * =========================================================================
 * SECTION 7: sp_rename
 * =========================================================================
 */
EXEC sp_rename 'ProjectManager.ManagerId', 'ProjectManagerId', 'COLUMN';
GO

EXEC sp_rename 'ProjectManager', 'Manager';
GO

/*
 * =========================================================================
 * SECTION 8: TRUNCATE TABLE
 * =========================================================================
 */
TRUNCATE TABLE Employee;
GO

SELECT * FROM Employee;
GO

/*
 * =========================================================================
 * SECTION 9: DROP TABLE
 * =========================================================================
 */
DROP TABLE HrManager;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * CREATE TABLE t (col type, …);
 * INSERT INTO t (cols) VALUES (vals);
 * SELECT * FROM t;
 * ALTER TABLE t ADD col type;
 * ALTER TABLE t ALTER COLUMN col newtype;
 * ALTER TABLE t DROP COLUMN col;
 * TRUNCATE TABLE t;
 * DROP TABLE t;
 * EXEC sp_rename 't.old', 'new', 'COLUMN';
 *
 * =============================================================================
 */
