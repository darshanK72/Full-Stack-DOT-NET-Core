-- DATATYPES
-- CONSTRAINT
-- Default
-- Rule

CREATE DATABASE CollegeDB;

USE CollegeDB;

-- Alias Datatype
CREATE TYPE email
FROM varchar(30) NOT NULL;

CREATE TABLE Student(

	-- Number Datatypes
	RollNo NUMERIC(5,0), -- Numeric(Precision,Scale)
	Marks DECIMAL(4,2), -- Decimal(Precision,Scale)
	Id int, -- Integer 
	SubjectsCount smallint, -- Small Integer
	StudentBigInt bigint, -- Big Integer
	StudentTinyInt tinyint, -- Tiny Integer
	StudentFloat float, -- Floating Point
	StudentPocketMoney money, -- Money
	StudentSmallMoney smallmoney, --Small Money

	-- String Datatypes
	StudentCharName char(255), -- Char
	StudentVarcharName varchar(255), -- Variable Char
	StudentNCharName nchar(300), -- Nchar
	StudentTextName text, -- Text

	-- Date & Time Datatypes
	StudentDateOfBirth date, -- Date (YYYY-MM-DD)
	StudentTimeOfBirth time, -- Time (HH:MM:SS)
	StudentDateTimeOfBirth datetime, -- DateTime(YYYY-MM-DD HH:MM:SS)


	-- Other Datatypes
	StudentProfileImate binary, -- Binary (Image,Video,Audio,Other)
	StudentImage image, -- Image 
	StudentXML xml,
	StudentGeolocation geometry,
	StudentGeography geography,
	StudentHirchery hierarchyid,
	StudentEmail email

);

DROP TABLE Student;
DROP TYPE email;

SELECT * FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Student';

-- XML Datatype

CREATE TABLE Teacher(
	TeacherData xml
);

DROP TABLE Teacher;

INSERT INTO Teacher(TeacherData) VALUES('
	<teacher id="1">
		<name type="textbox">Darshan Khairnar</name>
		<address type="textbox"> Anand Nager, Soygaon, Malegaon </address>
		<salary type="textbox"> 12000 </salary>
	</teacher>
');


SELECT t.TeacherData.query('/teacher/name[1]/text()') AS TeacherName,
t.TeacherData.query('/teacher/address[1]/text()') AS TeacherAddress,
t.TeacherData.query('/teacher/salary[1]/text()') AS TeacherSalary
FROM Teacher t;

SELECT * FROM Teacher;

-- Geometry & Geograhy

CREATE TABLE Graph(
	GraphId int,
	GraphPoints geometry
);


INSERT INTO Graph(GraphId,GraphPoints) VALUES(1,'point(40 57)');
INSERT INTO Graph(GraphId,GraphPoints) VALUES(2,'linestring(5 9,7 18)');
INSERT INTO Graph(GraphId,GraphPoints) VALUES(3,'polygon((0 0,15 8,3 0,9 6,0 0))');

SELECT * FROM Graph;

CREATE TABLE PolyGraph(
	GraphId int,
	PolyGraphPoints geography
);

INSERT INTO PolyGraph VALUES(1,'polygon((0 0,15 0,9 6,15 10,0 0))');

SELECT * FROM PolyGraph;

-- Identity

CREATE TABLE Subject(
	SubjectId int IDENTITY(1,1),
	SubjectName varchar(255)
);

INSERT INTO Subject(SubjectName) VALUES('English');
INSERT INTO Subject(SubjectName) VALUES('Marathi');
INSERT INTO Subject(SubjectName) VALUES('Hindi');
INSERT INTO Subject(SubjectName) VALUES('History');

SELECT * FROM Subject;

CREATE TABLE Marks(
	Hindi int,
	English int,
	Marathi int,
	History int,
	Science int,
	Average AS (Hindi+English+Marathi+History+Science)/5
);

INSERT INTO Marks(Hindi,English,Marathi,History,Science) VALUES(65,87,89,97,93);

SELECT * FROM Marks;

-- SEQUENCES

CREATE SEQUENCE mySequence
AS INT
START WITH 1
INCREMENT BY 10;

SELECT NEXT VALUE FOR mySequence;

CREATE TABLE Worker(
	WorkerId int,
	WorkerName varchar(50)
);

INSERT INTO Worker(WorkerId,WorkerName) VALUES(NEXT VALUE FOR mySequence,'Darshan');
INSERT INTO Worker(WorkerId,WorkerName) VALUES(NEXT VALUE FOR mySequence,'Aakash');
INSERT INTO Worker(WorkerId,WorkerName) VALUES(NEXT VALUE FOR mySequence,'Prasad');
INSERT INTO Worker(WorkerId,WorkerName) VALUES(NEXT VALUE FOR mySequence,'Krushna');
INSERT INTO Worker(WorkerId,WorkerName) VALUES(NEXT VALUE FOR mySequence,'Abhishek');

SELECT * FROM Worker;

DROP SEQUENCE mySequence;

-- Integraty Constraints

CREATE TABLE Exam(
	ExamId int PRIMARY KEY, -- Primary Key Constraint
	ExamName varchar(20) DEFAULT 'Final Year Exam', -- Default Constraint
	ExamStartDate datetime DEFAULT GETDATE(), -- Default Constraint
	ExamEndDate datetime DEFAULT GETDATE() + 20, -- Default Constraint
	ExamCenterCode float NOT NULL, -- Not Null constraint
	ExamUniqueCode int UNIQUE, -- Unique Constraint
	ExamCheckConstrient int CHECK(ExamCheckConstrient BETWEEN 18 AND 40), -- Check Constraint
	ExamDuration int check (ExamDuration >= 2 AND ExamDuration <= 4), -- Check Constraint

);


CREATE TABLE Person(
	PersonId int,
	PersonAge int,
	PersonName varchar(50),
	CONSTRAINT pk PRIMARY KEY (PersonId,PersonName)
);

USE CollegeDB;

DROP TABLE Person;

INSERT INTO Person(PersonId,PersonAge,PersonName) VALUES(102,40,'Naresh Khairnar');

-- Alter table to add Constraint
ALTER TABLE Person
ALTER COLUMN PersonAge int NOT NULL;

-- Externally Adding Constraint
ALTER TABLE Person
ADD CONSTRAINT PersonAage CHECK(PersonAge BETWEEN 18 AND 40);

CREATE TABLE Bank(
	BankId int,
	BankName varchar(50),
	
	-- Table Level Constraint
	PRIMARY KEY (BankId),
	CONSTRAINT unique_name UNIQUE (BankName)
);

DROP TABLE Bank;

CREATE TABLE Account(
	AccountNumber int PRIMARY KEY,
	AccountHolderName varchar(50),
	BankId int,

	-- Foreign Key Constraint
	CONSTRAINT fk_bankId FOREIGN KEY (BankId)
	REFERENCES Bank(BankId)
);

INSERT INTO Bank(BankId,BankName) VALUES(101,'SBI');
INSERT INTO Bank(BankId,BankName) VALUES(102,'BOI');
INSERT INTO Bank(BankId,BankName) VALUES(103,'HDFC');
INSERT INTO Bank(BankId,BankName) VALUES(104,'ICICI');
INSERT INTO Bank(BankId,BankName) VALUES(105,'AXIS');

INSERT INTO Account(AccountNumber,AccountHolderName,BankId) VALUES(1112,'Darshan',102);
INSERT INTO Account(AccountNumber,AccountHolderName,BankId) VALUES(1113,'Abhishek',101);
INSERT INTO Account(AccountNumber,AccountHolderName,BankId) VALUES(1114,'Aakash',105);
INSERT INTO Account(AccountNumber,AccountHolderName,BankId) VALUES(1115,'Prasad',103);

SELECT * FROM Person;


-- Using Defaults and Rules

CREATE DEFAULT default_phone_number
AS '01 23457 8910';

CREATE TABLE Station(
	StationId int PRIMARY KEY,
	StationPhoneNumber varchar(20) NOT NULL
);


EXEC sp_bindefault default_phone_number,'Station.StationPhoneNumber';

INSERT INTO Station(StationId) VALUES(101);
INSERT INTO Station(StationId) VALUES(102);
INSERT INTO Station(StationId) VALUES(103);
INSERT INTO Station(StationId) VALUES(104);

SELECT * FROM Station;

EXEC sp_unbindefault 'Station.StationPhoneNumber';

INSERT INTO Station(StationId,StationPhoneNumber) VALUES(105,'44-5523-2345');


CREATE RULE rangeVal
AS @rengvalue BETWEEN 40 AND 100;

CREATE TABLE Paper(
	PaperId int PRIMARY KEY,
	PaperName varchar(50) NOT NULL,
	Marks int
);

EXEC sp_bindrule rangeVal, 'Paper.Marks';


INSERT INTO Paper(PaperId,PaperName,Marks) VALUES(101,'English',45);
INSERT INTO Paper(PaperId,PaperName,Marks) VALUES(102,'Marathi',23); -- Cannot be inserted as value is not in range
INSERT INTO Paper(PaperId,PaperName,Marks) VALUES(103,'Hindi',44);
INSERT INTO Paper(PaperId,PaperName,Marks) VALUES(104,'History',87);

SELECT * FROM Paper;