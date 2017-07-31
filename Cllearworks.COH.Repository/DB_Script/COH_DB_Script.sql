
--USE COH
GO

CREATE TABLE Applications (
	Id					INT PRIMARY KEY IDENTITY(1,1),
	Name				VARCHAR(100) NOT NULL,
	ClientId			UNIQUEIDENTIFIER NOT NULL,
	ClientSecret		UNIQUEIDENTIFIER NOT NULL,
	Scope				VARCHAR(20) NULL,
	CreatedOn			DATETIME NOT NULL,
	UpdatedOn			DATETIME NOT NULL
)

INSERT INTO Applications	(Name, ClientId, ClientSecret, Scope, CreatedOn, UpdatedOn)
			VALUES			('Test App', '5491d69d-37dd-4e12-8991-21185c92d414', 'a0f68716-9b45-4629-97fd-8a70db3f9c7f', 'cms', '2017-03-09 13:51:55.753', '2017-03-09 13:51:55.753')
INSERT INTO Applications	(Name, ClientId, ClientSecret, Scope, CreatedOn, UpdatedOn)
			VALUES			('CMS App', '6764d924-adf2-4d4d-a8e3-0a0eff87c8fa', 'c88cf8f0-e4bd-472c-90c3-5a645602cbad', 'cms', '2017-03-09 13:51:55.753', '2017-03-09 13:51:55.753')
INSERT INTO Applications	(Name, ClientId, ClientSecret, Scope, CreatedOn, UpdatedOn)
			VALUES			('Cllearworks Mobile App', '1BADD9C5-7FE9-44C5-96E7-2FFB6E54156C', 'A0239EE4-33AE-4249-8F1B-11392DF0D166', 'mobile', '2017-03-20 07:52:41.110', '2017-03-20 07:52:41.110')

-- SELECT * FROM Applications

CREATE TABLE Client (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	OrganizationName	VARCHAR(250) NOT NULL,
	FirstName			VARCHAR(250) NOT NULL,
	LastName			VARCHAR(250) NULL,
	Email				VARCHAR(250) NOT NULL,
	Address				VARCHAR(250) NOT NULL,
	SubscriptionPlan	INT NULL,
	IsActive			BIT NOT NULL DEFAULT 0,	
	CreatedOn			DATETIME NOT NULL,
	UpdatedOn			DATETIME NOT NULL,
	ApplicationId		INT NOT NULL REFERENCES Applications(Id)
)

INSERT INTO Client	(OrganizationName, FirstName, LastName, Email, Address, SubscriptionPlan, IsActive, CreatedOn, UpdatedOn, ApplicationId)
			VALUES	('Dummy Client', 'Super', 'Admin', 'superadmin@cllearworks.com', 'Office', 1, 1, GETUTCDATE(), GETUTCDATE(), 1)
INSERT INTO Client	(OrganizationName, FirstName, LastName, Email, Address, SubscriptionPlan, IsActive, CreatedOn, UpdatedOn, ApplicationId)
			VALUES	('Cllearworks', 'Gambhir', 'Chauhan', 'gambhir@cllearworks.com', 'Shindhubhavan Road, Ahmedabad', 1, 1, GETUTCDATE(), GETUTCDATE(), 3)

-- SELECT * FROM Client

CREATE TABLE Place (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	Name				VARCHAR(250) NOT NULL,
	Address				VARCHAR(250) NULL,
	IsActive			BIT NOT NULL DEFAULT 1,
	CreatedOn			DATETIME NOT NULL,
	UpdatedOn			DATETIME NOT NULL,
	ClientId			INT NOT NULL REFERENCES Client(Id)
)

INSERT INTO Place	(Name, Address, IsActive, CreatedOn, UpdatedOn, ClientId)
			VALUES	('Shindhubhavan Branch', 'Ahmedabad', 1, GETUTCDATE(), GETUTCDATE(), 2)

-- SELECT * FROM Place

CREATE TABLE Department (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	Name				VARCHAR(250) NOT NULL,
	IsActive			BIT NOT NULL DEFAULT 1,
	CreatedOn			DATETIME NOT NULL,
	UpdatedOn			DATETIME NOT NULL,
	ClientId			INT NOT NULL REFERENCES Client(Id)
)

INSERT INTO Department	(Name, IsActive, CreatedOn, UpdatedOn, ClientId)
			VALUES	('Sales', 1, GETUTCDATE(), GETUTCDATE(), 2)
INSERT INTO Department	(Name, IsActive, CreatedOn, UpdatedOn, ClientId)
			VALUES	('Development', 1, GETUTCDATE(), GETUTCDATE(), 2)

-- SELECT * FROM Department

CREATE TABLE Users (
	Id				INT PRIMARY KEY IDENTITY(1,1),
	FirstName		VARCHAR(250) NOT NULL,
	LastName		VARCHAR(250) NULL,
	Email			VARCHAR(250) NOT NULL,
	PasswordHash	VARCHAR(MAX) NOT NULL,
	Salt			VARCHAR(MAX) NOT NULL,
	IsActive		BIT NOT NULL DEFAULT 1,
	CreatedOn		DATETIME NOT NULL,
	UpdatedOn		DATETIME NOT NULL,
	Role			INT NOT NULL,
	ClientId		INT NOT NULL REFERENCES Client(Id)
)

INSERT INTO Users	(FirstName, LastName, Email, PasswordHash, Salt, IsActive, CreatedOn, UpdatedOn, Role, ClientId)
			VALUES	('Ghanshyam', 'Patel', 'ghanshyam@cllearworks.com', 'DwkRo/UgtmpK4gqvpx1+ppAf4iaQ4LbQu/JDMj7ck10=', 'b9RM7CKTlw540+hRW1qtFQB3jAtjGi1N9hc5T4b5DcA=', 1, GETUTCDATE(), GETUTCDATE(), 1, 1)
INSERT INTO Users	(FirstName, LastName, Email, PasswordHash, Salt, IsActive, CreatedOn, UpdatedOn, Role, ClientId)
			VALUES	('Jigar', 'Khalas', 'jigar@cllearworks.com', 'DwkRo/UgtmpK4gqvpx1+ppAf4iaQ4LbQu/JDMj7ck10=', 'b9RM7CKTlw540+hRW1qtFQB3jAtjGi1N9hc5T4b5DcA=', 1, GETUTCDATE(), GETUTCDATE(), 1, 1)
INSERT INTO Users	(FirstName, LastName, Email, PasswordHash, Salt, IsActive, CreatedOn, UpdatedOn, Role, ClientId)
			VALUES	('Super', 'Admin', 'superadmin@cllearworks.com', 'DwkRo/UgtmpK4gqvpx1+ppAf4iaQ4LbQu/JDMj7ck10=', 'b9RM7CKTlw540+hRW1qtFQB3jAtjGi1N9hc5T4b5DcA=', 1, GETUTCDATE(), GETUTCDATE(), 1, 1)
INSERT INTO Users	(FirstName, LastName, Email, PasswordHash, Salt, IsActive, CreatedOn, UpdatedOn, Role, ClientId)
			VALUES	('Client', 'Admin', 'client@cllearworks.com', 'DwkRo/UgtmpK4gqvpx1+ppAf4iaQ4LbQu/JDMj7ck10=', 'b9RM7CKTlw540+hRW1qtFQB3jAtjGi1N9hc5T4b5DcA=', 1, GETUTCDATE(), GETUTCDATE(), 2, 2)
INSERT INTO Users	(FirstName, LastName, Email, PasswordHash, Salt, IsActive, CreatedOn, UpdatedOn, Role, ClientId)
			VALUES	('HR', 'Admin', 'hr@cllearworks.com', 'DwkRo/UgtmpK4gqvpx1+ppAf4iaQ4LbQu/JDMj7ck10=', 'b9RM7CKTlw540+hRW1qtFQB3jAtjGi1N9hc5T4b5DcA=', 1, GETUTCDATE(), GETUTCDATE(), 3, 2)

-- password 123
-- SELECT * FROM Users

CREATE TABLE Employee (
	Id							INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	DeviceId					VARCHAR(250) NOT NULL,
	GmcId						VARCHAR(250) NULL,
	ApnId						VARCHAR(250) NULL,
	FirstName					VARCHAR(250) NOT NULL,
	LastName					VARCHAR(250) NULL,
	Email						VARCHAR(250) NOT NULL,
	PhoneNumber					VARCHAR(40) NULL,
	WorkingHours				DECIMAL(4, 2) NULL,
	BreakHours					DECIMAL(4, 2) NULL,
	IsActive					BIT NOT NULL DEFAULT 0,	
	FailedLoginAttemptCount		INT NULL,
	LastLoginDate				DATETIME NULL,
	CreatedOn					DATETIME NOT NULL,
	UpdatedOn					DATETIME NOT NULL,
	ClientId					INT NOT NULL REFERENCES Client(Id),
	PlaceId						INT NOT NULL REFERENCES Place(Id),
	DepartmentId				INT NOT NULL REFERENCES Department(Id),
	EmployeeCode				VARCHAR(50) NULL,
	IsApproved					BIT NOT NULL DEFAULT 0,
)

insert into employee 
values('456', '5', '5', 'Jigar', 'Khalas', 'jigar@cllearworks.com', '9988998899', CAST(8 AS DECIMAL(4, 2)), CAST(1 AS DECIMAL(4, 2)), 1, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 2, 1, 1, NULL, 1)
insert into employee 
values('457', '6', '6', 'Gaurang', 'Parikh', 'gaurang@cllearworks.com', '9988998899', CAST(8 AS DECIMAL(4, 2)), CAST(1 AS DECIMAL(4, 2)), 1, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 2, 1, 1, NULL, 1)

-- SELECT * FROM Employee

CREATE TABLE ChangeRequest (
	Id							INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	DeviceId					VARCHAR(250) NOT NULL,
	GmcId						VARCHAR(250) NULL,
	ApnId						VARCHAR(250) NULL,
	Email						VARCHAR(250) NOT NULL,
	IsApproved					BIT NOT NULL DEFAULT 0,
	RequestedDate				DATETIME NOT NULL,
	ApprovedBy					INT NOT NULL REFERENCES Users(Id)
)

CREATE TABLE ShiftMaster (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	Name				VARCHAR(250) NOT NULL,
	StartTime			DATETIME NOT NULL,
	EndTime				DATETIME NOT NULL,
	IsActive			BIT NOT NULL DEFAULT 1,
	CreatedOn			DATETIME NOT NULL,
	UpdatedOn			DATETIME NOT NULL,
	CreatedBy			INT NOT NULL REFERENCES Users(Id),
	ClientId			INT NOT NULL REFERENCES Client(Id)
)

INSERT INTO ShiftMaster (Name, StartTime, EndTime, IsActive, CreatedOn, UpdatedOn, CreatedBy, ClientId)
			VALUES		('Morning shift', '2017-03-23 06:00:00.000', '2017-03-23 15:00:00.000', 1, GETUTCDATE(), GETUTCDATE(), 4, 2)

-- SELECT * FROM ShiftMaster

CREATE TABLE ShiftEmployeeHistory (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	ShiftId				INT NOT NULL REFERENCES ShiftMaster(Id),
	StartDate			DATETIME NOT NULL,
	EndDate				DATETIME NULL,
	EmployeeId			INT NOT NULL REFERENCES Employee(Id)
)

INSERT INTO ShiftEmployeeHistory (ShiftId, StartDate, EndDate, EmployeeId)
			VALUES		(1, '2017-03-23 07:29:06.127', NULL, 1)
INSERT INTO ShiftEmployeeHistory (ShiftId, StartDate, EndDate, EmployeeId)
			VALUES		(1, '2017-03-23 07:29:06.127', NULL, 2)

-- SELECT * FROM ShiftMaster

CREATE TABLE Beacon (
	Id			INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	Name		VARCHAR(250) NOT NULL,
	MacAddress	VARCHAR(500) NULL,
	UUID		VARCHAR(500) NULL,
	Major		INT NULL,
	Minor		INT NULL,
	BeaconType	INT NULL,
	IsActive	BIT NOT NULL DEFAULT 1,	
	CreatedOn			DATETIME NOT NULL,
	UpdatedOn			DATETIME NOT NULL,
	PlaceId		INT NOT NULL REFERENCES Place(Id),
	DepartmentId INT NOT NULL REFERENCES Department(Id)
)

insert into beacon (name, uuid, major, minor,beacontype,isactive,createdOn,UpdatedOn,placeId, departmentid) 
			values ('Beacon 1', '47B1A59A-59CE-4E57-A1F7-65E5A8A96E00', 16507, 55383, 0, 1,getutcdate(),getutcdate(),1, 1)
insert into beacon (name, uuid, major, minor,beacontype,isactive,createdOn,UpdatedOn,placeId, departmentid) 
			values ('Beacon 2', 'B9407F30-F5F8-466E-AFF9-25556B57FE6D', 39989, 36227, 1, 1,getutcdate(),getutcdate(),1, 1)
insert into beacon (name, uuid, major, minor,beacontype,isactive,createdOn,UpdatedOn,placeId, departmentid) 
			values ('Beacon 3', 'B9407F30-F5F8-466E-AFF9-25556B57FE6D', 49076, 49744, 2, 1,getutcdate(),getutcdate(),1, 2)

-- SELECT * FROM Beacon

CREATE TABLE Attendance (
	Id				INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	EmployeeId		INT NOT NULL REFERENCES Employee(Id),
	AttendanceDate	DATETIME NULL,
	CheckInTime		DATETIME NULL,
	CheckOutTime	DATETIME NULL,
	TotalInTime		BIGINT NULL,
	TotalOutTime	BIGINT NULL,
	Remarks			VARCHAR(MAX) NULL,
	IsPresent		BIT NULL
)

-- SELECT * FROM Attendance

CREATE TABLE Track (
	Id				INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	AttendanceId	INT NOT NULL REFERENCES Attendance(Id),
	FromTime		DATETIME NULL,
	ToTime			DATETIME NULL,
	IsIn			BIT NULL,
	IsOut			BIT NULL,
	TrackDuration	BIGINT NULL,
	FromBeacon		INT NOT NULL REFERENCES Beacon(Id),
	Status			VARCHAR(500) NULL
)

-- SELECT * FROM Track

--CREATE TABLE ApplicationTokens (
--	Id INT PRIMARY KEY IDENTITY(1,1),
--	Token VARCHAR(1000) NOT NULL,
--	Expiration DATETIME NOT NULL,
--	FKApplication INT References Applications(Id)
--)

----------------------
select * from Applications
select * from Client
select * from Place
select * from Department
select * from Users
select * from Employee
select * from ChangeRequest
select * from ShiftMaster
select * from ShiftEmployeeHistory
select * from Beacon
select * from attendance
select * from Track

select NEWID()
select GETDATE()
select GETUTCDATE()

--DROP TABLE Track
--DROP TABLE Attendance
--DROP TABLE Beacon
--DROP TABLE ShiftEmployeeHistory
--DROP TABLE ShiftMaster
--DROP TABLE ChangeRequest
--DROP TABLE Employee
--DROP TABLE Users
--DROP TABLE Department
--DROP TABLE Place
--DROP TABLE Client
--DROP TABLE Applications


-- Changes on 28-03-2017 - Done on Azure

ALTER TABLE Employee
ADD ApprovedBy INT NULL REFERENCES Users(Id);

ALTER TABLE Employee
ALTER COLUMN PlaceId INT NULL;

ALTER TABLE Employee
ALTER COLUMN DepartmentId INT NULL;

ALTER TABLE ChangeRequest
ALTER COLUMN ApprovedBy INT NULL;

-- Changes on 31-03-2017 - Done on azure

/*
Purpose		:	Dashboard query for daily attandance count based on client id and place id.
Author		:	Gaurang
In params	:	ClientId
Out params	:	no one out params
Remarks		:	
Remote Call	:	EXEC [dbo].[GetDailyAttendanceCount] 2, 1
*/

ALTER PROCEDURE [dbo].[GetDailyAttendanceCount]
(
	@clientId int,
	@placeId int
)
AS
BEGIN
		SELECT 
			CONVERT (DATE,a.AttendanceDate) AS AttendanceDate,
			TotalEmp = (SELECT COUNT(*) FROM Employee WHERE ClientId = @clientId AND Status = 1),
			Ontime = COUNT(CASE WHEN CONVERT(TIME, a.CheckInTime) <= CONVERT(TIME,DATEADD(MINUTE, +15, sm.StartTime)) THEN '' END),
			Late = COUNT(CASE WHEN CONVERT(TIME, a.CheckInTime) > CONVERT(TIME, DATEADD(MINUTE, +15, sm.StartTime)) THEN '' END)
		FROM 
			Employee e INNER JOIN
			Attendance a ON e.Id = a.EmployeeId INNER JOIN
			ShiftEmployeeHistory seh ON e.Id = seh.EmployeeId INNER JOIN
			ShiftMaster sm ON sm.Id = seh.ShiftId
		WHERE	
			e.ClientId = @clientId
			AND (@placeId = 0 OR e.PlaceId = @placeId)
			AND e.Status = 1
			AND seh.EndDate IS NULL
			AND CONVERT(DATE, a.AttendanceDate) = CONVERT(DATE, GETUTCDATE())
		GROUP BY CONVERT(DATE, a.AttendanceDate)
END

-- Changes on 10-04-2017 - Done on azure

/*
Purpose		:	Advanced report.
Author		:	Ghanshyam
In params	:	ClientId and many
Out params	:	-
Remarks		:	
Remote Call	:	EXEC [dbo].[GetAdvancedReport] 2
*/

ALTER PROCEDURE [dbo].[GetAdvancedReport]
(
	@clientId		INT,
	@placeId		INT = NULL,
	@departmentId	INT = NULL,
	@shiftId		INT = NULL,
	@employeeId		INT = NULL,
	@startDate		DATETIME = NULL,
	@endDate		DATETIME = NULL,
	@lateBy			INT = NULL
)
AS
BEGIN
		SET @placeId = ISNULL(@placeId, 0)
		SET @departmentId = ISNULL(@departmentId, 0)
		SET @employeeId = ISNULL(@employeeId, 0)
		SET @shiftId = ISNULL(@shiftId, 0)

		SELECT
			e.Id AS EmployeeId,
			e.EmployeeCode,
			e.FirstName,
			e.LastName,
			e.Email,
			e.PhoneNumber,
			e.WorkingHours,
			e.BreakHours,
			p.Name AS PlaceName,
			d.Name AS DepartmentName,
			sm.Name AS ShiftName,
			sm.StartTime AS ShiftStartTime,
			sm.EndTime AS ShiftEndTime,
			a.Id AS AttendanceId,
			a.AttendanceDate,
			a.CheckInTime,
			a.CheckOutTime,
			a.TotalInTime,
			a.TotalOutTime,
			DATEDIFF(MINUTE, CONVERT(TIME, sm.StartTime), CONVERT(TIME, a.CheckInTime)) AS LateByMinutes
		FROM
			Employee e INNER JOIN
			Attendance a ON e.Id = a.EmployeeId INNER JOIN
			ShiftEmployeeHistory seh ON e.Id = seh.EmployeeId INNER JOIN
			ShiftMaster sm on sm.Id = seh.ShiftId INNER JOIN
			Place p ON e.PlaceId = p.Id INNER JOIN
			Department d ON e.DepartmentId = d.id
		WHERE	
			e.ClientId = @clientId
			AND e.Status = 1
			AND (@employeeId = 0 OR e.Id = @employeeId)
			AND (@placeId = 0 OR e.PlaceId = @placeId)
			AND (@departmentId = 0 OR e.DepartmentId = @departmentId)
			AND (@shiftId = 0 OR sm.Id = @shiftId)
			AND ((a.AttendanceDate >= seh.StartDate AND a.AttendanceDate <= seh.EndDate) OR (a.AttendanceDate >= seh.StartDate AND seh.EndDate IS NULL))
			AND (@startDate IS NULL OR CONVERT(DATE, a.AttendanceDate) >= @startDate)
			AND (@endDate IS NULL OR CONVERT(DATE, a.AttendanceDate) <= @endDate)
			AND (@lateBy IS NULL OR (CONVERT(TIME, a.CheckInTime) > CONVERT(TIME, DATEADD(MINUTE, @lateBy, sm.StartTime))))

END

-- Changes on 11-04-2017 - Done on azure

/*
Purpose		:	Yearly Report of Employee
Author		:	Jigar
In params	:	Employee id and year
Out params	:	no one out params
Remarks		:	
Remote Call	:	EXEC [dbo].[GetYearlyAttendanceByEmployee] 3, 2017
*/

ALTER PROCEDURE [dbo].[GetYearlyAttendanceByEmployee]
(
	@employeeId INT,
	@year INT
)
AS
BEGIN
	    declare @WorkingHours int;
		set @WorkingHours = (select WorkingHours from Employee where id=@employeeId)

		SELECT
			DATENAME(month,(AttendanceDate)) AS Month,
			COUNT(*) AS WorkingDays,
			SUM(TotalInTime) AS ActualWorkingHours,
			(COUNT(*) * @WorkingHours) AS TotalWorkingHours,
			COUNT(CASE WHEN CONVERT(TIME, Attendance.CheckInTime) > CONVERT(TIME, ShiftMaster.StartTime) THEN '' END) AS TotalLateDays
		FROM Employee
			INNER JOIN Attendance ON Employee.id = Attendance.EmployeeId
			INNER JOIN ShiftEmployeeHistory ON Employee.Id = ShiftEmployeeHistory.EmployeeId 
			INNER JOIN ShiftMaster ON ShiftEmployeeHistory.ShiftId = ShiftMaster.Id
		WHERE 
			YEAR(Attendance.AttendanceDate) = @year
			AND Employee.Id = @employeeId
		GROUP BY
			DATENAME(month, (AttendanceDate))

END

-- Changes on 11-04-2017 - Done on azure

ALTER TABLE Employee
ADD Status INT NOT NULL DEFAULT 1;

ALTER TABLE Employee
DROP COLUMN IsActive;

ALTER TABLE Employee
DROP COLUMN IsApproved;

ALTER TABLE ChangeRequest
ADD Status INT NOT NULL DEFAULT 2;

ALTER TABLE ChangeRequest
DROP COLUMN IsApproved;

-- Changes for Shift, Leave and Holiday - Done

CREATE TABLE Holiday (
	Id				INT PRIMARY KEY IDENTITY(1, 1),
	Name			VARCHAR(250) NOT NULL,
	StartDate		DATETIME NOT NULL,
	EndDate			DATETIME NOT NULL,
	ClientId		INT REFERENCES Client(Id) NOT NULL,
)

CREATE TABLE HolidayDetails (
	Id				INT PRIMARY KEY IDENTITY(1, 1),
	HolidayId		INT REFERENCES Holiday(Id) NOT NULL,
	ToClient		INT REFERENCES Client(Id),
	ToPlace			INT REFERENCES Place(Id),
	ToDepartment	INT REFERENCES Department(Id),
	ToEmployee		INT REFERENCES Employee(Id)
)

CREATE TABLE Leave (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	LeaveType			INT NOT NULL,
	StartDate			DATETIME NOT NULL,
	EndDate				DATETIME NOT NULL,
	Reason				VARCHAR(250) NOT NULL,
	EmployeeId			INT REFERENCES Employee(Id),
	Status				INT NOT NULL,
	ApprovedByEmployee	INT REFERENCES Employee(Id) NULL,
	ApprovedByUser		INT REFERENCES Users(Id) NULL
)

CREATE TABLE Shift (
	Id				INT PRIMARY KEY IDENTITY(1, 1),
	Name			VARCHAR(250) NOT NULL,
	IsActive		BIT NOT NULL DEFAULT 1,
	CreatedOn		DATETIME NOT NULL,
	UpdatedOn		DATETIME NOT NULL,
	CreatedBy		INT REFERENCES Users(Id) NOT NULL,
	ClientId		INT REFERENCES Client(Id) NOT NULL
)

---- 28-04-2017 Pending on Azure
CREATE TABLE ShiftDetails (
	Id				INT PRIMARY KEY IDENTITY(1, 1),
	ShiftId			INT REFERENCES Shift(Id) NOT NULL,
	IsWorkingDay	BIT NOT NULL Default 0,
	DayOfWeek		INT NOT NULL,
	StartTime		DATETIME NULL,
	EndTime			DATETIME NULL,
	WorkingHours	DECIMAL(5, 2) NULL,
	BreakHours		DECIMAL(5, 2) NULL
)

---- 25-04-2017 Pending on Azure

CREATE TABLE ShiftEmployeeHistory (
	Id					INT PRIMARY KEY IDENTITY(1, 1),
	ShiftId				INT NOT NULL REFERENCES Shift(Id),
	StartDate			DATETIME NOT NULL,
	EndDate				DATETIME NULL,
	EmployeeId			INT NOT NULL REFERENCES Employee(Id)
)


