/*
 * =============================================================================
 * 01. DATA TYPES IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: Built-in T-SQL data types, user-defined alias types, IDENTITY,
 *        computed columns, sequences, and specialized types (xml, geometry).
 *
 * WHY IT MATTERS:
 *   Column types define what values can be stored and how much space they use.
 *   Choosing the right type prevents overflow, rounding errors, and wasted storage.
 *
 * WHAT YOU WILL LEARN:
 *   1. Numeric, string, and date/time types
 *   2. User-defined alias types (CREATE TYPE)
 *   3. xml, geometry, and geography
 *   4. IDENTITY columns and SEQUENCE objects
 *   5. Computed columns
 *
 * Sample database: CollegeDB (created below).
 *
 * =============================================================================
 */

CREATE DATABASE CollegeDB;
GO

USE CollegeDB;
GO

/*
 * =========================================================================
 * SECTION 1: NUMERIC DATA TYPES
 * =========================================================================
 *
 * Type          | Storage idea              | Typical use
 * --------------|---------------------------|----------------------------------
 * tinyint       | 0–255                     | flags, small counts
 * smallint      | −32,768 to 32,767         | small integers
 * int           | standard 32-bit integer   | IDs, counts
 * bigint        | very large integers       | high-volume keys
 * decimal(p,s)  | exact fixed precision     | money, grades (avoid float rounding)
 * numeric(p,s)  | same as decimal           | financial / measured values
 * float         | approximate floating      | scientific (not money)
 * money         | fixed currency            | currency (prefer decimal in new designs)
 * smallmoney    | smaller currency range    | small amounts
 * -------------------------------------------------------------------------
 */
CREATE TABLE StudentNumericTypes
(
    RollNo              NUMERIC(5, 0),      -- exact number: 5 digits, 0 after decimal
    Marks               DECIMAL(4, 2),      -- e.g. 99.75 — 4 total digits, 2 fractional
    Id                  INT,                -- 32-bit integer — most common whole-number type
    SubjectsCount       SMALLINT,           -- smaller integer range
    StudentBigInt       BIGINT,             -- 64-bit integer
    StudentTinyInt      TINYINT,            -- 0–255
    StudentFloat        FLOAT,              -- approximate — do not use for money
    StudentPocketMoney  MONEY,              -- currency type (legacy; decimal often preferred)
    StudentSmallMoney   SMALLMONEY          -- smaller money range
);
GO

DROP TABLE StudentNumericTypes;
GO

/*
 * =========================================================================
 * SECTION 2: STRING DATA TYPES
 * =========================================================================
 */
CREATE TABLE StudentStringTypes
(
    StudentCharName     CHAR(255),          -- fixed 255 chars (padded)
    StudentVarcharName  VARCHAR(255),       -- up to 255 chars, stores only what is used
    StudentNCharName    NCHAR(300),         -- Unicode fixed length
    StudentTextName     TEXT                -- legacy large string type
);
GO

DROP TABLE StudentStringTypes;
GO

/*
 * =========================================================================
 * SECTION 3: DATE AND TIME DATA TYPES
 * =========================================================================
 */
CREATE TABLE StudentDateTimeTypes
(
    StudentDateOfBirth      DATE,           -- 2000-12-07
    StudentTimeOfBirth      TIME,           -- 14:30:00
    StudentDateTimeOfBirth  DATETIME        -- 2000-12-07 14:30:00
);
GO

DROP TABLE StudentDateTimeTypes;
GO

/*
 * =========================================================================
 * SECTION 4: USER-DEFINED ALIAS TYPE
 * =========================================================================
 */
CREATE TYPE email FROM VARCHAR(30) NOT NULL;
GO

CREATE TABLE StudentWithEmail
(
    StudentId       INT,
    StudentEmail    email                   -- alias type enforces varchar(30) NOT NULL
);
GO

DROP TABLE StudentWithEmail;
DROP TYPE email;
GO

/*
 * =========================================================================
 * SECTION 5: XML DATA TYPE
 * =========================================================================
 */
CREATE TABLE Teacher
(
    TeacherData XML
);
GO

INSERT INTO Teacher (TeacherData)
VALUES ('
    <teacher id="1">
        <name type="textbox">Darshan Khairnar</name>
        <address type="textbox">Anand Nagar, Soygaon, Malegaon</address>
        <salary type="textbox">12000</salary>
    </teacher>
');
GO

SELECT
    t.TeacherData.query('/teacher/name[1]/text()')     AS TeacherName,
    t.TeacherData.query('/teacher/address[1]/text()')  AS TeacherAddress,
    t.TeacherData.query('/teacher/salary[1]/text()')   AS TeacherSalary
FROM Teacher AS t;
GO

DROP TABLE Teacher;
GO

/*
 * =========================================================================
 * SECTION 6: GEOMETRY AND GEOGRAPHY
 * =========================================================================
 */
CREATE TABLE Graph
(
    GraphId      INT,
    GraphPoints  GEOMETRY
);
GO

INSERT INTO Graph (GraphId, GraphPoints) VALUES (1, 'POINT(40 57)');
INSERT INTO Graph (GraphId, GraphPoints) VALUES (2, 'LINESTRING(5 9, 7 18)');
INSERT INTO Graph (GraphId, GraphPoints) VALUES (3, 'POLYGON((0 0, 15 8, 3 0, 9 6, 0 0))');
GO

SELECT * FROM Graph;
GO

CREATE TABLE PolyGraph
(
    GraphId         INT,
    PolyGraphPoints GEOGRAPHY
);
GO

INSERT INTO PolyGraph VALUES (1, 'POLYGON((0 0, 15 0, 9 6, 15 10, 0 0))');
GO

SELECT * FROM PolyGraph;
GO

DROP TABLE Graph;
DROP TABLE PolyGraph;
GO

/*
 * =========================================================================
 * SECTION 7: IDENTITY COLUMN
 * =========================================================================
 */
CREATE TABLE Subject
(
    SubjectId   INT IDENTITY(1, 1),           -- starts at 1, increments by 1
    SubjectName VARCHAR(255)
);
GO

INSERT INTO Subject (SubjectName) VALUES ('English');
INSERT INTO Subject (SubjectName) VALUES ('Marathi');
INSERT INTO Subject (SubjectName) VALUES ('Hindi');
INSERT INTO Subject (SubjectName) VALUES ('History');
GO

SELECT * FROM Subject;
GO

DROP TABLE Subject;
GO

/*
 * =========================================================================
 * SECTION 8: COMPUTED COLUMN
 * =========================================================================
 */
CREATE TABLE Marks
(
    Hindi    INT,
    English  INT,
    Marathi  INT,
    History  INT,
    Science  INT,
    Average  AS (Hindi + English + Marathi + History + Science) / 5
);
GO

INSERT INTO Marks (Hindi, English, Marathi, History, Science)
VALUES (65, 87, 89, 97, 93);
GO

SELECT * FROM Marks;
GO

DROP TABLE Marks;
GO

/*
 * =========================================================================
 * SECTION 9: SEQUENCE OBJECT
 * =========================================================================
 */
CREATE SEQUENCE mySequence
    AS INT
    START WITH 1
    INCREMENT BY 10;
GO

SELECT NEXT VALUE FOR mySequence;
GO

CREATE TABLE Worker
(
    WorkerId   INT,
    WorkerName VARCHAR(50)
);
GO

INSERT INTO Worker (WorkerId, WorkerName) VALUES (NEXT VALUE FOR mySequence, 'Darshan');
INSERT INTO Worker (WorkerId, WorkerName) VALUES (NEXT VALUE FOR mySequence, 'Aakash');
INSERT INTO Worker (WorkerId, WorkerName) VALUES (NEXT VALUE FOR mySequence, 'Prasad');
GO

SELECT * FROM Worker;
GO

DROP TABLE Worker;
DROP SEQUENCE mySequence;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * Exact numbers  → DECIMAL / NUMERIC
 * Whole numbers  → INT, BIGINT, SMALLINT, TINYINT
 * Text           → VARCHAR, CHAR, NVARCHAR
 * Dates          → DATE, TIME, DATETIME2
 * Auto numbers   → IDENTITY or SEQUENCE + NEXT VALUE FOR
 * Derived values → computed column AS (expression)
 *
 * =============================================================================
 */
