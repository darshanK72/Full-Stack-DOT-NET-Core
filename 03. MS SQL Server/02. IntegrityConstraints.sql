/*
 * =============================================================================
 * 02. INTEGRITY CONSTRAINTS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: PRIMARY KEY, FOREIGN KEY, UNIQUE, CHECK, DEFAULT, NOT NULL,
 *        composite keys, ALTER TABLE constraints, and legacy RULE/DEFAULT objects.
 *
 * WHY IT MATTERS:
 *   Constraints enforce data rules inside the database — duplicate keys, invalid
 *   ages, and orphaned foreign keys are rejected before bad data reaches your app.
 *
 * WHAT YOU WILL LEARN:
 *   1. Column-level constraints
 *   2. Composite primary keys
 *   3. FOREIGN KEY referential integrity
 *   4. ALTER TABLE to add constraints
 *   5. Legacy DEFAULT and RULE objects
 *
 * Prerequisite: run 01. DataTypes.sql first (uses CollegeDB).
 *
 * =============================================================================
 */

USE CollegeDB;
GO

/*
 * =========================================================================
 * SECTION 1: COLUMN-LEVEL CONSTRAINTS
 * =========================================================================
 *
 * Constraint   | Purpose
 * -------------|--------------------------------------------------------
 * PRIMARY KEY  | Uniquely identifies each row; implies NOT NULL
 * UNIQUE       | All values must differ
 * NOT NULL     | Column must have a value on every row
 * DEFAULT      | Value used when INSERT omits the column
 * CHECK        | Boolean expression each row must satisfy
 * -------------------------------------------------------------------------
 */
CREATE TABLE Exam
(
    ExamId              INT PRIMARY KEY,
    ExamName            VARCHAR(20) DEFAULT 'Final Year Exam',
    ExamStartDate       DATETIME DEFAULT GETDATE(),
    ExamEndDate         DATETIME DEFAULT GETDATE() + 20,
    ExamCenterCode      FLOAT NOT NULL,
    ExamUniqueCode      INT UNIQUE,
    ExamCheckConstraint INT CHECK (ExamCheckConstraint BETWEEN 18 AND 40),
    ExamDuration        INT CHECK (ExamDuration >= 2 AND ExamDuration <= 4)
);
GO

DROP TABLE Exam;
GO

/*
 * =========================================================================
 * SECTION 2: COMPOSITE PRIMARY KEY
 * =========================================================================
 */
CREATE TABLE Person
(
    PersonId   INT,
    PersonAge  INT,
    PersonName VARCHAR(50),
    CONSTRAINT pk_Person PRIMARY KEY (PersonId, PersonName)
);
GO

INSERT INTO Person (PersonId, PersonAge, PersonName)
VALUES (102, 40, 'Naresh Khairnar');
GO

ALTER TABLE Person
ALTER COLUMN PersonAge INT NOT NULL;
GO

ALTER TABLE Person
ADD CONSTRAINT PersonAgeCheck CHECK (PersonAge BETWEEN 18 AND 40);
GO

/*
 * =========================================================================
 * SECTION 3: FOREIGN KEY
 * =========================================================================
 */
CREATE TABLE Bank
(
    BankId   INT,
    BankName VARCHAR(50),
    PRIMARY KEY (BankId),
    CONSTRAINT unique_bank_name UNIQUE (BankName)
);
GO

CREATE TABLE Account
(
    AccountNumber     INT PRIMARY KEY,
    AccountHolderName VARCHAR(50),
    BankId            INT,
    CONSTRAINT fk_BankId FOREIGN KEY (BankId) REFERENCES Bank (BankId)
);
GO

INSERT INTO Bank (BankId, BankName) VALUES (101, 'SBI');
INSERT INTO Bank (BankId, BankName) VALUES (102, 'BOI');
INSERT INTO Bank (BankId, BankName) VALUES (103, 'HDFC');
GO

INSERT INTO Account (AccountNumber, AccountHolderName, BankId)
VALUES (1112, 'Darshan', 102);
GO

SELECT * FROM Account;
GO

DROP TABLE Account;
DROP TABLE Bank;
DROP TABLE Person;
GO

/*
 * =========================================================================
 * SECTION 4: LEGACY DEFAULT AND RULE (DEPRECATED)
 * =========================================================================
 */
CREATE DEFAULT default_phone_number AS '01 23457 8910';
GO

CREATE TABLE Station
(
    StationId           INT PRIMARY KEY,
    StationPhoneNumber  VARCHAR(20) NOT NULL
);
GO

EXEC sp_bindefault 'default_phone_number', 'Station.StationPhoneNumber';
GO

INSERT INTO Station (StationId) VALUES (101);
INSERT INTO Station (StationId) VALUES (102);
GO

SELECT * FROM Station;
GO

EXEC sp_unbindefault 'Station.StationPhoneNumber';
GO

CREATE RULE rangeVal AS @rangevalue BETWEEN 40 AND 100;
GO

CREATE TABLE Paper
(
    PaperId   INT PRIMARY KEY,
    PaperName VARCHAR(50) NOT NULL,
    Marks     INT
);
GO

EXEC sp_bindrule 'rangeVal', 'Paper.Marks';
GO

INSERT INTO Paper (PaperId, PaperName, Marks) VALUES (101, 'English', 45);
INSERT INTO Paper (PaperId, PaperName, Marks) VALUES (104, 'History', 87);
GO

SELECT * FROM Paper;
GO

/*
 * =========================================================================
 * SECTION 5: INSPECT COLUMN METADATA
 * =========================================================================
 */
SELECT *
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Paper';
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * PRIMARY KEY   → unique row identity
 * FOREIGN KEY   → REFERENCES parent(key)
 * UNIQUE        → no duplicate values
 * CHECK         → custom boolean rule
 * DEFAULT       → value when column omitted on INSERT
 * NOT NULL      → value required
 *
 * =============================================================================
 */
