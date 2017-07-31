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
			DATEDIFF(HOUR, sd.StartTime, sd.EndTime) - sd.BreakHours AS WorkingHours,
			sd.BreakHours AS BreakHours,
			p.Name AS PlaceName,
			d.Name AS DepartmentName,
			sm.Name AS ShiftName,
			sd.StartTime AS ShiftStartTime,
			sd.EndTime AS ShiftEndTime,
			a.Id AS AttendanceId,
			a.AttendanceDate,
			a.CheckInTime,
			a.CheckOutTime,
			a.TotalInTime,
			a.TotalOutTime,
			DATEDIFF(MINUTE, CONVERT(TIME, sd.StartTime), CONVERT(TIME, a.CheckInTime)) AS LateByMinutes
		FROM
			Employee e INNER JOIN
			Attendance a ON e.Id = a.EmployeeId INNER JOIN			
			Place p ON e.PlaceId = p.Id INNER JOIN
			Department d ON e.DepartmentId = d.id INNER JOIN
			ShiftEmployeeHistory seh ON e.Id = seh.EmployeeId INNER JOIN
			Shift sm ON sm.Id = seh.ShiftId INNER JOIN
			ShiftDetails sd ON sm.Id = sd.ShiftId
		WHERE	
			e.ClientId = @clientId
			AND e.Status = 1
			AND (@employeeId = 0 OR e.Id = @employeeId)
			AND (@placeId = 0 OR e.PlaceId = @placeId)
			AND (@departmentId = 0 OR e.DepartmentId = @departmentId)
			AND (@shiftId = 0 OR sm.Id = @shiftId)
			AND ((a.AttendanceDate >= seh.StartDate AND a.AttendanceDate <= seh.EndDate) OR (a.AttendanceDate >= seh.StartDate AND seh.EndDate IS NULL))
			AND (sd.DayOfWeek = (((@@DATEFIRST-1) + DATEPART(WEEKDAY, CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, a.AttendanceDate), '+05:30')))) % 7))
			AND (@startDate IS NULL OR CONVERT(DATE, a.AttendanceDate) >= @startDate)
			AND (@endDate IS NULL OR CONVERT(DATE, a.AttendanceDate) <= @endDate)
			AND (@lateBy IS NULL OR (CONVERT(TIME, a.CheckInTime) > CONVERT(TIME, DATEADD(MINUTE, @lateBy, sd.StartTime))))
		ORDER BY
			e.FirstName

END

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
			Ontime = COUNT(CASE WHEN CONVERT(TIME, a.CheckInTime) <= CONVERT(TIME, sd.StartTime) THEN '' END),
			Late = COUNT(CASE WHEN CONVERT(TIME, a.CheckInTime) > CONVERT(TIME, sd.StartTime) THEN '' END)
		FROM 
			Employee e INNER JOIN
			Attendance a ON e.Id = a.EmployeeId INNER JOIN
			ShiftEmployeeHistory seh ON e.Id = seh.EmployeeId INNER JOIN
			Shift s ON s.Id = seh.ShiftId INNER JOIN
			ShiftDetails sd ON s.Id = sd.ShiftId
		WHERE	
			e.ClientId = @clientId
			AND (@placeId = 0 OR e.PlaceId = @placeId)
			AND e.Status = 1
			AND seh.EndDate IS NULL
			AND CONVERT(DATE, a.AttendanceDate) = CONVERT(DATE, GETUTCDATE())
			AND (sd.DayOfWeek = (((@@DATEFIRST-1) + DATEPART(WEEKDAY, CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, a.AttendanceDate), '+05:30')))) % 7))
		GROUP BY CONVERT(DATE, a.AttendanceDate)
END

ALTER PROCEDURE [dbo].[GetMonthlyReport]
(
	@employeeId INT,
	@startDate	DATETIME,
	@endDate	DATETIME
)
AS
BEGIN
	;WITH Dates AS (
		SELECT @StartDate as DT
		UNION ALL
		SELECT DATEADD(D, 1, DT) FROM Dates WHERE DT < @EndDate
	),
	RecordsWithShift AS (
		SELECT
			DT AS ShiftDate,
			sd.IsWorkingDay,
			DayOfWeek,
			sd.StartTime AS ShiftStartTime,
			sd.EndTime AS ShiftEndTime,
			DATEDIFF(HOUR, sd.StartTime, sd.EndTime) - sd.BreakHours AS WorkingHours,
			sd.BreakHours AS BreakHours
		FROM
			Dates d
			LEFT JOIN ShiftDetails sd ON (sd.DayOfWeek = (((@@DATEFIRST-1) + DATEPART(WEEKDAY, d.DT)) % 7))
			LEFT JOIN Shift s ON sd.ShiftId = s.Id
			LEFT JOIN ShiftEmployeeHistory seh ON s.Id = seh.ShiftId
			LEFT JOIN Employee e ON seh.EmployeeId = e.Id
		WHERE
			e.Id = @employeeId
			AND ((d.DT >= seh.StartDate AND d.DT <= seh.EndDate) OR (d.DT >= seh.StartDate AND seh.EndDate IS NULL))
	),
	RecordsWithAttendances AS (
		SELECT
			ShiftDate,
			IsWorkingDay,
			DayOfWeek,
			ShiftStartTime,
			ShiftEndTime,
			WorkingHours,
			BreakHours,
			a.Id AS AttendanceId,
			CASE WHEN a.Id IS NULL THEN 0 ELSE 1 END AS IsPresent,
			a.AttendanceDate,
			a.CheckInTime,
			a.CheckOutTime,
			a.TotalInTime,
			a.TotalOutTime,
			DATEDIFF(MINUTE, CONVERT(TIME, r.ShiftStartTime), CONVERT(TIME, a.CheckInTime)) AS LateByMinutes
		FROM 
			RecordsWithShift r
			LEFT JOIN Attendance a ON CONVERT(DATE, r.ShiftDate) = CONVERT(DATE, CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, a.AttendanceDate), '+05:30'))) AND a.EmployeeId = @employeeId
		--ORDER BY sr.ShiftDate
	),
	RecordsWithLeaves AS (
		SELECT
			r.*,
			CASE WHEN l.Id IS NULL THEN 0 ELSE 1 END AS IsLeave,
			l.LeaveType AS LeaveType,
			l.Status AS LeaveStatus,
			l.Reason AS LeaveReason
		FROM
			RecordsWithAttendances r
			LEFT JOIN Leave l ON CONVERT(DATE, r.ShiftDate) >= CONVERT(DATE, l.StartDate) AND CONVERT(DATE, r.ShiftDate) <= CONVERT(DATE, l.EndDate) AND l.EmployeeId = @employeeId AND l.Status = 2
	),
	MyHolidays AS (
		SELECT
			h.*
		FROM
			Holiday h
			INNER JOIN HolidayDetails hd ON h.Id = hd.HolidayId
			INNER JOIN Employee e ON e.Id = @employeeId AND (e.ClientId = hd.ToClient OR e.PlaceId = hd.ToPlace OR e.DepartmentId = hd.ToDepartment OR e.Id = hd.ToEmployee)
	),
	--SELECT * FROM MyHolidays
	RecordsWithHolidays AS (
		SELECT
			r.*,
			CASE WHEN h.Id IS NULL THEN 0 ELSE 1 END AS IsHoliday,
			h.Name AS HolidayName
		FROM
			RecordsWithLeaves r
			LEFT JOIN MyHolidays h ON CONVERT(DATE, r.ShiftDate) >= CONVERT(DATE, h.StartDate) AND CONVERT(DATE, r.ShiftDate) <= CONVERT(DATE, h.EndDate)
	)
	SELECT * FROM RecordsWithHolidays ORDER BY ShiftDate
END

/*
EXEC GetMonthlyReport 1
*/

ALTER PROCEDURE [dbo].[GetWeeklyAttendanceByEmployee_ForMobile]
(
	@employeeId INT,
	@minDate DATETIME
)
AS
BEGIN
	SELECT *
	FROM
	(
		SELECT
			ROW_NUMBER() OVER ( ORDER BY a.AttendanceDate DESC) AS RowNum,
			a.Id,
			a.EmployeeId,
			AttendanceDate,
			CheckInTime,
			CheckOutTime,
			TotalInTime,
			TotalOutTime,
			Remarks,
			IsPresent,
			sd.StartTime AS ShiftStartTime,
			sd.EndTime AS ShiftEndTime,
			WorkingHours,
			BreakHours
		FROM 
			Attendance a
			INNER JOIN ShiftEmployeeHistory seh ON a.EmployeeId = seh.EmployeeId 
			INNER JOIN Shift s ON seh.ShiftId = s.Id
			INNER JOIN ShiftDetails sd ON s.Id = sd.ShiftId
		WHERE 
			CONVERT(DATE, a.AttendanceDate) < @minDate
			AND a.EmployeeId = @employeeId
			AND ((a.AttendanceDate >= seh.StartDate AND a.AttendanceDate <= seh.EndDate) OR (a.AttendanceDate >= seh.StartDate AND seh.EndDate IS NULL))
			AND (sd.DayOfWeek = (((@@DATEFIRST-1) + DATEPART(WEEKDAY, CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, a.AttendanceDate), '+05:30')))) % 7))
	) AS AttendanceRecords
	WHERE RowNum <= 7
END

-- EXEC GetWeeklyAttendanceByEmployee_ForMobile 1, '2017-05-01 13:07:50.550'

/*
Purpose		:	Yearly Report of Employee
Author		:	Jigar
In params	:	Employee id and year
Out params	:	no one out params
Remarks		:	
Remote Call	:	EXEC [dbo].[GetYearlyAttendanceByEmployee] 2, 2017
*/

ALTER PROCEDURE [dbo].[GetYearlyAttendanceByEmployee]
(
	@employeeId INT,
	@year INT
)
AS
BEGIN
		SELECT
			DATENAME(month,(AttendanceDate)) AS Month,
			COUNT(*) AS WorkingDays,
			SUM(TotalInTime) AS ActualWorkingHours,
			SUM(DATEDIFF(HOUR, ShiftDetails.StartTime, ShiftDetails.EndTime) - ShiftDetails.BreakHours) AS TotalWorkingHours,
			COUNT(CASE WHEN CONVERT(TIME, Attendance.CheckInTime) > CONVERT(TIME, ShiftDetails.StartTime) THEN '' END) AS TotalLateDays
		FROM Employee
			INNER JOIN Attendance ON Employee.id = Attendance.EmployeeId
			INNER JOIN ShiftEmployeeHistory ON Employee.Id = ShiftEmployeeHistory.EmployeeId 
			INNER JOIN Shift ON ShiftEmployeeHistory.ShiftId = Shift.Id
			INNER JOIN ShiftDetails ON Shift.Id = ShiftDetails.ShiftId
		WHERE 
			YEAR(Attendance.AttendanceDate) = @year
			AND Employee.Id = @employeeId
			AND ((Attendance.AttendanceDate >= ShiftEmployeeHistory.StartDate AND Attendance.AttendanceDate <= ShiftEmployeeHistory.EndDate) OR (Attendance.AttendanceDate >= ShiftEmployeeHistory.StartDate AND ShiftEmployeeHistory.EndDate IS NULL))
			AND (ShiftDetails.DayOfWeek = (((@@DATEFIRST-1) + DATEPART(WEEKDAY, CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, Attendance.AttendanceDate), '+05:30')))) % 7))
		GROUP BY
			DATENAME(month, (AttendanceDate)), DATEPART(m, (AttendanceDate))
		ORDER BY
			DATEPART(m, (AttendanceDate))

END


/*
Purpose		:	Yearly Report of Employee for Mobile
Author		:	Jigar
In params	:	Employee id and year
Out params	:	no one out params
Remarks		:	
Remote Call	:	EXEC [dbo].[GetYearlyAttendanceByEmployee_ForMobile] 2, 2017
*/

ALTER PROCEDURE [dbo].[GetYearlyAttendanceByEmployee_ForMobile]
(
	@employeeId INT,
	@year INT
)
AS
BEGIN
		SELECT
			SUM(DATEDIFF(HOUR, ShiftDetails.StartTime, ShiftDetails.EndTime) - ShiftDetails.BreakHours) AS TotalRequiredWorkingHours,
			COUNT(*) AS TotalPresentDays,
			SUM(TotalInTime) AS TotalInTime,
			SUM(TotalOutTime) AS TotalOutTime
		FROM Employee
			INNER JOIN Attendance ON Employee.id = Attendance.EmployeeId
			INNER JOIN ShiftEmployeeHistory ON Employee.Id = ShiftEmployeeHistory.EmployeeId 
			INNER JOIN Shift ON ShiftEmployeeHistory.ShiftId = Shift.Id
			INNER JOIN ShiftDetails ON Shift.Id = ShiftDetails.ShiftId
		WHERE 
			YEAR(Attendance.AttendanceDate) = @year
			AND Employee.Id = @employeeId
			AND ((Attendance.AttendanceDate >= ShiftEmployeeHistory.StartDate AND Attendance.AttendanceDate <= ShiftEmployeeHistory.EndDate) OR (Attendance.AttendanceDate >= ShiftEmployeeHistory.StartDate AND ShiftEmployeeHistory.EndDate IS NULL))
			AND (ShiftDetails.DayOfWeek = (((@@DATEFIRST-1) + DATEPART(WEEKDAY, CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, Attendance.AttendanceDate), '+05:30')))) % 7))

END