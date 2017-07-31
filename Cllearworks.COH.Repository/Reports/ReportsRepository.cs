using Cllearworks.COH.Models.Leaves;
using Cllearworks.COH.Models.Reports;
using Cllearworks.COH.Repository.Holidays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Reports
{
    public class ReportsRepository : BaseRepository, IReportsRepository
    {
        #region Attendance

        //AttendacebyEmployee
        public async Task<IEnumerable<AttendanceByEmployeeReportModel>> GetAttendanceByEmployeeAsync(int employeeId, int month, int year)
        {
            return await Task.Run(() =>
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var data = Context.GetMonthlyReport(employeeId, startDate, endDate).ToList();

                var result = new List<AttendanceByEmployeeReportModel>();
                foreach (var e in data)
                {
                    var model = new AttendanceByEmployeeReportModel();
                    model.ShiftDate = e.ShiftDate.Value;
                    model.IsWorkingDay = e.IsWorkingDay.Value;
                    model.DayOfWeek = (DayOfWeek)e.DayOfWeek;
                    model.ShiftStartDate = e.ShiftStartTime;
                    model.ShiftEndDate = e.ShiftEndTime;
                    model.WorkingHours = e.WorkingHours;
                    model.BreakHours = e.BreakHours;
                    model.AttendanceId = e.AttendanceId;
                    model.IsPresent = e.IsPresent > 0 ? true : false;
                    model.AttendanceDate = e.AttendanceDate;
                    model.CheckInTime = e.CheckInTime;
                    model.CheckOutTime = e.CheckOutTime;
                    model.TotalInTime = e.TotalInTime != null ? new TimeSpan(e.TotalInTime.Value).TotalMinutes : default(double);
                    model.TotalOutTime = e.TotalOutTime != null ? new TimeSpan(e.TotalOutTime.Value).TotalMinutes : default(double);
                    model.LateByMinutes = e.LateByMinutes;
                    model.IsLeave = e.IsLeave > 0 ? true : false;
                    model.LeaveType = e.LeaveType.HasValue ? (LeaveTypes)e.LeaveType.Value : (LeaveTypes?)null;
                    model.LeaveStatus = e.LeaveStatus.HasValue ? (LeaveStatus)e.LeaveStatus.Value : (LeaveStatus?)null;
                    model.LeaveReason = e.LeaveReason;
                    model.IsHoliday = e.IsHoliday > 0 ? true : false;
                    model.HolidayName = e.HolidayName;

                    result.Add(model);
                }

                return result;
            });
        }

        //AttendacebyDate
        public async Task<IEnumerable<AttendanceByDateModel>> GetAttendanceByDateAsync(int clientId, DateTime date)
        {
            return await Task.Run(() =>
            {
                var minDate = date.Date.ToUniversalTime();
                var maxDate = date.Date.AddDays(1).ToUniversalTime();

                var data = (from s in Context.Shifts
                            join sh in Context.ShiftEmployeeHistories on s.Id equals sh.ShiftId
                            join e in Context.Employees on sh.EmployeeId equals e.Id
                            join a in Context.Attendances on e.Id equals a.EmployeeId
                            join p in Context.Places on e.PlaceId equals p.Id
                            join d in Context.Departments on e.DepartmentId equals d.Id
                            where e.ClientId == clientId &&
                            ((a.AttendanceDate >= sh.StartDate && a.AttendanceDate <= sh.EndDate) || (a.AttendanceDate >= sh.StartDate && sh.EndDate == null)) &&
                            (a.AttendanceDate >= minDate && a.AttendanceDate <= maxDate)
                            select new AttendanceByDateModel
                            {
                                EmployeeId = e.Id,
                                EmployeeCode = e.EmployeeCode,
                                FirstName = e.FirstName,
                                LastName = e.LastName,
                                Email = e.Email,
                                Contact = e.PhoneNumber,
                                //WorkingHours = e.WorkingHours,
                                //BreakHours = e.BreakHours,
                                PlaceName = p.Name,
                                DepartmentName = d.Name,
                                ShiftName = s.Name,
                                CheckInTime = a.CheckInTime,
                                CheckOutTime = a.CheckOutTime,
                                TotalInTime = a.TotalInTime,
                                TotalOutTime = a.TotalOutTime,
                                AttendanceId = a.Id,
                                AttendanceDate = a.AttendanceDate
                            }).ToList();

                return data;
            });
        }


        public async Task<IEnumerable<AttendanceByYearlyModel>> GetAttendanceByYearlyAsync(int employeeId, int year)
        {
            return await Task.Run(() =>
            {
                var data = Context.GetYearlyAttendanceByEmployee(employeeId, year).ToList();

                var attendanceByYearData = data.Select(e => new AttendanceByYearlyModel
                {
                    EmployeeId = employeeId,
                    Month = e.Month,
                    WorkingDays = e.WorkingDays.HasValue ? e.WorkingDays.Value : 0,
                    ActualWorkingHours = e.ActualWorkingHours.HasValue ? Math.Round(new TimeSpan(e.ActualWorkingHours.Value).TotalHours) : 0,
                    TotalWorkingHours = e.TotalWorkingHours.HasValue ? (double)e.TotalWorkingHours.Value : 0,
                    TotalLateDays = e.TotalLateDays.HasValue ? e.TotalLateDays.Value : 0
                });

                return attendanceByYearData;
            });
        }

        public async Task<IEnumerable<AdvancedReportModel>> GetAdvancedReport(int clientId, int? placeId, int? departmentId, int? shiftId, int? employeeId, DateTime? startDate, DateTime? endDate, int? lateBy)
        {
            return await Task.Run(() => 
            {
                var query = Context.GetAdvancedReport(clientId, placeId, departmentId, shiftId, employeeId, startDate, endDate, lateBy);

                var data = query.ToList().Select(e => new AdvancedReportModel() {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.PhoneNumber,
                    WorkingHours = e.WorkingHours,
                    BreakHours = e.BreakHours,
                    PlaceName = e.PlaceName,
                    DepartmentName = e.DepartmentName,
                    ShiftName = e.ShiftName,
                    ShiftStartTime = e.ShiftStartTime,
                    ShiftEndTime = e.ShiftEndTime,
                    AttendanceId = e.AttendanceId,
                    AttendanceDate = e.AttendanceDate.Value,
                    CheckInTime = e.CheckInTime.Value,
                    CheckOutTime = e.CheckOutTime,
                    TotalInTime = e.TotalInTime,
                    TotalOutTime = e.TotalOutTime,
                    LateByMinutes = e.LateByMinutes.HasValue ? e.LateByMinutes.Value : 0
                });

                return data;
            });
        }


        #endregion
    }
}
