using Cllearworks.COH.Models.Attendances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Attendances
{
    public class AttendanceRepository : BaseRepository, IAttendanceRepository
    {
        #region Mobile

        public async Task<Attendance> CheckInAsync(Attendance dataModel)
        {
            return await Task.Run(() =>
            {
                Context.Attendances.Add(dataModel);
                Context.SaveChanges();
                return Context.Attendances.Find(dataModel.Id); ;
            });
        }

        public async Task<Attendance> CheckOutAsync(Attendance dataModel)
        {
            return await Task.Run(() =>
            {
                Context.SaveChanges();
                return dataModel;
            });
        }

        public async Task<Attendance> GetAttendanceForTodayAsync(int employeeId, DateTime? attendanceDate = null)
        {
            return await Task.Run(() =>
            {
                var minDate = default(DateTime);
                var maxDate = default(DateTime);

                if (attendanceDate != null || attendanceDate.HasValue)
                {
                    minDate = new DateTime(attendanceDate.Value.Year, attendanceDate.Value.Month, attendanceDate.Value.Day, 0, 0, 0);
                    maxDate = new DateTime(attendanceDate.Value.Year, attendanceDate.Value.Month, attendanceDate.Value.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);
                }
                else
                {
                    minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                    maxDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);
                }
                var todayAttandance = Context.Attendances.Where(x => x.EmployeeId == employeeId && x.AttendanceDate >= minDate && x.AttendanceDate <= maxDate).FirstOrDefault();
                return todayAttandance;
            });
        }

        public async Task<IEnumerable<AttendanceModel>> GetAttendanceByWeeklyAsync(int employeeId)
        {
            return await Task.Run(() =>
            {
                var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);

                var query = Context.GetWeeklyAttendanceByEmployee_ForMobile(employeeId, minDate).ToList();

                var data = query.Select(a => new AttendanceModel()
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    AttendanceDate = a.AttendanceDate,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    TotalInTime = a.TotalInTime.HasValue ? new TimeSpan(a.TotalInTime.Value).TotalMinutes : 0,
                    TotalOutTime = a.TotalOutTime.HasValue ? new TimeSpan(a.TotalOutTime.Value).TotalMinutes : 0,
                    Remarks = a.Remarks,
                    IsPresent = a.IsPresent,
                    ShiftStartTime = a.ShiftStartTime,
                    ShiftEndTime = a.ShiftEndTime,
                    WorkingHours = a.WorkingHours,
                    BreakHours = a.BreakHours
                }).ToList();

                return data;
            });
        }

        public async Task<IQueryable<Attendance>> GetAttendanceByMonthlyAsync(int employeeId)
        {
            return await Task.Run(() =>
            {
                var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);

                var month = DateTime.UtcNow.Month;
                var data = Context.Attendances.Where(e => e.EmployeeId == employeeId && e.AttendanceDate.Value.Month == month && e.AttendanceDate.Value < minDate);
                return data;
            });
        }

        public async Task<AttendanceByYearModel> GetAttendanceByYearlyAsync(int employeeId)
        {
            return await Task.Run(() =>
            {
                var minDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);

                var year = DateTime.UtcNow.Year;

                var data = Context.GetYearlyAttendanceByEmployee_ForMobile(employeeId, year).ToList();

                var attendanceByYearData = data.Select(e => new AttendanceByYearModel
                {
                    TotalInTime = e.TotalInTime.HasValue ? new TimeSpan(e.TotalInTime.Value).TotalMinutes : 0,
                    TotalOutTime = e.TotalOutTime.HasValue ? new TimeSpan(e.TotalOutTime.Value).TotalMinutes : 0,
                    TotalPresentDays = e.TotalPresentDays.HasValue ? e.TotalPresentDays.Value : 0,
                    TotalRequiredWorkingHours = e.TotalRequiredWorkingHours > 0 ? e.TotalRequiredWorkingHours * 60 : 0

                });

                return attendanceByYearData.FirstOrDefault();
            });
        }
        #endregion Mobile
    }
}
