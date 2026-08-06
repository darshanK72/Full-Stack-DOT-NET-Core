-- SQL Data Defination Language (DDL)

-- DATABASE OPERATIONS
CREATE DATABASE EmployeeDB;

DROP DATABASE EmployeeDB;

use master;

SELECT name FROM master.sys.databases ORDER BY name;

CREATE DATABASE CompanyDB;

use CompanyDB;

-- CREATE TABLE
CREATE TABLE Employee(
	EmployeeId int,
	EmployeeName varchar(50),
	EmployeeAddress varchar(255),
	EmployeeMobileNumber varchar(10),
	EmployeeDateOfBirth datetime,
	EmployeeFavChar char,
	EmployeeBaseSalary money,
	EmployeeProjectId int,
	EmployeeProjectManagerId int,
	EmployeeHrManagerId int
);


CREATE TABLE Project(
	ProjectId int,
	ProjectName varchar(50),
	ProjectManagerId int,
	ProjectStartDate datetime,
	ProjectClientName char(50),
	ProjectBugdet money,
	ProjectTeamSize int
);

CREATE TABLE ProjectManager(
	ManagerId int,
	ManagerTeamName varchar(50),
	ManagerTeamSize int,
	ManagerProjectId int
);

CREATE TABLE HrManager(
	HrManagerId int,
	HrManagerEmployeeId int,
	HrManagerSubordinateCount int
);

DROP TABLE Employee;

-- INSERT 
INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (101,'Darshan Khairnar','Nashik','9834444657','2000-12-07','M',21000,11,301,401);

INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (101,'Darshan Khairnar','Nashik','9834444657','2000-12-07','M',21000,11,301,401);

INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (101,'Darshan Khairnar','Nashik','9834444657','2000-12-07','M',21000,11,301,401);

INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (102,'Aakash Khairnar','Nashik','9834444657','2001-01-01','K',21000,11,301,401);

INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (103,'Krushna Khairnar','Nashik','9834444657','2000-06-26','P',21000,11,301,401);

INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (104,'Prasad Khairnar','Nashik','9834444657','2004-02-05','A',21000,11,301,401);

INSERT INTO Employee(EmployeeId,EmployeeName,EmployeeAddress,EmployeeMobileNumber,EmployeeDateOfBirth,EmployeeFavChar,EmployeeBaseSalary,EmployeeProjectId,EmployeeProjectManagerId,EmployeeHrManagerId)
VALUES (105,'Abhishek Khairnar','Nashik','9834444657','2003-03-09','S',21000,11,301,401);

-- SELECT
SELECT * FROM Employee;

-- ALTER
ALTER TABLE ProjectManager ADD NewAddexColumn char;

SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE table_name = 'ProjectManager';


ALTER TABLE ProjectManager ALTER COLUMN ManagerId bigint;

SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE table_name = 'ProjectManager';

ALTER TABLE ProjectManager DROP COLUMN NewAddexColumn;

SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE table_name = 'ProjectManager';


-- SP_RENAME
SP_RENAME 'ProjectManager.ManagerId','ProjectManagerId';

SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE table_name = 'ProjectManager';

SP_RENAME 'ProjectManager','Manager';

SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE table_name = 'Manager';

-- TRUNCATE
TRUNCATE TABLE Employee;

SELECT * FROM Employee;

-- DROP TABLE
DROP TABLE HrManager;