
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

ALTER TABLE Employee
DROP COLUMN WorkingHours;

ALTER TABLE Employee
DROP COLUMN BreakHours;

DROP TABLE ShiftMaster

-- And also migrate records for new shift

-- Changes on 26-04-2017

ALTER TABLE Employee
ADD ImagePath VARCHAR(100) NULL;

-- Need to update all SP's from local to azure