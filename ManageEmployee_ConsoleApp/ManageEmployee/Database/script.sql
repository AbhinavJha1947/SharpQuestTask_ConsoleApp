-----------------------------------------------------------
-- 1) CREATE DATABASE
-----------------------------------------------------------
IF DB_ID('EmployeesDB') IS NULL
    CREATE DATABASE EmployeesDB;
GO

USE EmployeesDB;
GO

-----------------------------------------------------------
-- 2) CREATE TABLES
-----------------------------------------------------------
IF OBJECT_ID('dbo.Employee', 'U') IS NOT NULL DROP TABLE dbo.EmployeeSalary;
IF OBJECT_ID('dbo.Employee', 'U') IS NOT NULL DROP TABLE dbo.Employee;

CREATE TABLE dbo.Employee (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    SSN NVARCHAR(20) NOT NULL,
    DOB DATE NOT NULL,
    Address NVARCHAR(300),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Zip NVARCHAR(20),
    Phone NVARCHAR(50),
    JoinDate DATE NOT NULL,
    ExitDate DATE NULL
);

CREATE TABLE dbo.EmployeeSalary (
    SalaryId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Employee(EmployeeId),
    FromDate DATE NOT NULL,
    ToDate DATE NULL,
    Title NVARCHAR(150) NOT NULL,
    Salary DECIMAL(18,2) NOT NULL
);

CREATE INDEX IX_EmployeeSalary_EmployeeId_FromDate 
ON dbo.EmployeeSalary(EmployeeId, FromDate);
GO

-----------------------------------------------------------
-- 3) SUPPORTING LOOKUP TABLES (Names, Titles, Cities, States)
-----------------------------------------------------------

DECLARE @FirstNames TABLE (Name NVARCHAR(50));
INSERT INTO @FirstNames VALUES
('John'),('Mary'),('Robert'),('Patricia'),('Michael'),('Linda'),
('William'),('Elizabeth'),('David'),('Jennifer'),('James'),('Susan'),
('Charles'),('Jessica'),('Thomas'),('Sarah'),('Christopher'),
('Karen'),('Daniel'),('Nancy'),('Matthew'),('Lisa'),('Anthony'),
('Michelle'),('Brian'),('Kimberly'),('George'),('Emily'),
('Edward'),('Amanda'),('Joshua'),('Rebecca');

DECLARE @LastNames TABLE (Name NVARCHAR(50));
INSERT INTO @LastNames VALUES
('Smith'),('Johnson'),('Brown'),('Williams'),('Jones'),('Garcia'),
('Miller'),('Davis'),('Rodriguez'),('Martinez'),('Hernandez'),
('Lopez'),('Gonzalez'),('Wilson'),('Anderson'),('Taylor'),
('Thomas'),('Moore'),('Jackson'),('Martin'),('Lee'),('Perez'),
('Thompson'),('White'),('Harris'),('Sanchez'),('Clark'),
('Ramirez'),('Lewis'),('Robinson'),('Walker'),('Young');

DECLARE @Cities TABLE (City NVARCHAR(100), State NVARCHAR(50));
INSERT INTO @Cities VALUES
('New York','NY'),('Los Angeles','CA'),('Chicago','IL'),('Houston','TX'),
('Phoenix','AZ'),('Philadelphia','PA'),('San Antonio','TX'),
('San Diego','CA'),('Dallas','TX'),('San Jose','CA'),
('Austin','TX'),('Jacksonville','FL'),('San Francisco','CA'),
('Charlotte','NC'),('Seattle','WA');

DECLARE @Titles TABLE (Title NVARCHAR(100), MinSalary INT, MaxSalary INT);
INSERT INTO @Titles VALUES
('Software Developer',60000,110000),
('Senior Software Developer',100000,160000),
('QA Engineer',50000,90000),
('DevOps Engineer',90000,150000),
('Product Manager',90000,170000),
('Business Analyst',60000,100000),
('Data Engineer',90000,160000),
('Data Scientist',90000,170000),
('Tech Lead',120000,190000),
('Frontend Developer',70000,130000),
('Backend Developer',80000,140000),
('Full Stack Developer',80000,140000),
('Support Engineer',45000,85000);

-----------------------------------------------------------
-- 4) INSERT 100 EMPLOYEES
-----------------------------------------------------------

DECLARE @i INT = 1;

WHILE @i <= 100
BEGIN
    DECLARE @First NVARCHAR(50) = (SELECT TOP 1 Name FROM @FirstNames ORDER BY NEWID());
    DECLARE @Last NVARCHAR(50) = (SELECT TOP 1 Name FROM @LastNames ORDER BY NEWID());
    DECLARE @FullName NVARCHAR(200) = @First + ' ' + @Last;

    DECLARE @SSN NVARCHAR(20) = 
        CONCAT(FORMAT(ABS(CHECKSUM(NEWID())) % 900 + 100, '000'), '-',
               FORMAT(ABS(CHECKSUM(NEWID())) % 90 + 10, '00'), '-',
               FORMAT(ABS(CHECKSUM(NEWID())) % 9000 + 1000, '0000'));

    DECLARE @DOB DATE = DATEADD(YEAR, -(22 + ABS(CHECKSUM(NEWID()) % 42)), GETDATE());

    DECLARE @City NVARCHAR(100) = (SELECT TOP 1 City FROM @Cities ORDER BY NEWID());
    DECLARE @State NVARCHAR(50) = (SELECT TOP 1 State FROM @Cities ORDER BY NEWID());

    DECLARE @Address NVARCHAR(300) =
        CONCAT((ABS(CHECKSUM(NEWID())) % 9999 + 1), ' ', 
        (SELECT TOP 1 Name FROM @LastNames ORDER BY NEWID()), ' Street');

    DECLARE @Zip NVARCHAR(20) = FORMAT(ABS(CHECKSUM(NEWID())) % 90000 + 10000, '00000');

    DECLARE @Phone NVARCHAR(50) =
        CONCAT('+1-', FORMAT(ABS(CHECKSUM(NEWID())) % 800 + 200, '000'),
                   '-', FORMAT(ABS(CHECKSUM(NEWID())) % 900 + 100, '000'),
                   '-', FORMAT(ABS(CHECKSUM(NEWID())) % 9000 + 1000, '0000'));

    DECLARE @JoinDate DATE = DATEADD(YEAR, 18 + ABS(CHECKSUM(NEWID()) % 15), @DOB);

    INSERT INTO dbo.Employee (FullName, SSN, DOB, Address, City, State, Zip, Phone, JoinDate, ExitDate)
    VALUES (@FullName, @SSN, @DOB, @Address, @City, @State, @Zip, @Phone, @JoinDate, NULL);

    DECLARE @EmpId INT = SCOPE_IDENTITY();

    -----------------------------------------------------------
    -- 5) INSERT 1 Salary Record (Current)
    -----------------------------------------------------------
    DECLARE @Title NVARCHAR(100) = (SELECT TOP 1 Title FROM @Titles ORDER BY NEWID());
    DECLARE @MinSal INT = (SELECT MinSalary FROM @Titles WHERE Title = @Title);
    DECLARE @MaxSal INT = (SELECT MaxSalary FROM @Titles WHERE Title = @Title);
    DECLARE @Salary DECIMAL(18,2) = @MinSal + (ABS(CHECKSUM(NEWID())) % (@MaxSal - @MinSal));

    INSERT INTO dbo.EmployeeSalary (EmployeeId, FromDate, ToDate, Title, Salary)
    VALUES (@EmpId, @JoinDate, NULL, @Title, @Salary);

    SET @i += 1;
END;

PRINT '✔ 100 Employees + Salary data inserted successfully.';
GO
