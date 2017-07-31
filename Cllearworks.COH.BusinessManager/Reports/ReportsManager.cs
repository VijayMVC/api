using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Reports;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Reports
{
    public class ReportsManager : IReportsManager
    {
        private readonly IReportsRepository _reportRepository;
        private readonly IPermissionManager _permissionManager;

        public ReportsManager(IReportsRepository reportRepository, IPermissionManager permissionManager)
        {
            _reportRepository = reportRepository;
            _permissionManager = permissionManager;
        }

        #region Attendance
        //AttendacebyEmployee
        public async Task<IEnumerable<AttendanceByEmployeeReportModel>> GetAttendanceByEmployeeAsync(int employeeId, int month, int year, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHRUserReport))
                throw new Exception("User has not permission to perform this operation");

            return await _reportRepository.GetAttendanceByEmployeeAsync(employeeId, month, year);
        }

        //AttendacebyDate
        public async Task<IEnumerable<AttendanceByDateModel>> GetAttendanceByDateAsync(int clientId, int userId, DateTime date)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHRUserReport))
                throw new Exception("User has not permission to perform this operation");

            var employeeAttendanceByDate = await _reportRepository.GetAttendanceByDateAsync(clientId, date);

            employeeAttendanceByDate.ToList().ForEach((e) =>
            {
                e.TotalInTime = e.TotalInTime.HasValue ? (long)new TimeSpan(e.TotalInTime.Value).TotalMinutes : 0;
                e.TotalOutTime = e.TotalOutTime.HasValue ? (long)new TimeSpan(e.TotalOutTime.Value).TotalMinutes : 0;
            });

            return employeeAttendanceByDate;
        }

        public async Task<IEnumerable<AttendanceByYearlyModel>> GetAttendanceByYearlyAsync(int clientId, int userId, int employeeId, int year)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHRUserReport))
                throw new Exception("User has not permission to perform this operation");

            var attendanceByYearData = (await _reportRepository.GetAttendanceByYearlyAsync(employeeId, year)).ToList();

            attendanceByYearData.ForEach((a) =>
            {
                a.TotalLessWorkingHours = a.TotalWorkingHours > a.ActualWorkingHours ? (a.TotalWorkingHours - a.ActualWorkingHours) : 0;
                a.TotalOverTime = a.ActualWorkingHours > a.TotalWorkingHours ? (a.ActualWorkingHours - a.TotalWorkingHours) : 0;
            });
            return attendanceByYearData;
        }

        public async Task<IEnumerable<AdvancedReportModel>> GetAdvancedReport(int? placeId, int? departmentId, int? shiftId, int? employeeId, DateTime? startDate, DateTime? endDate, int? lateBy, int clientId, int userId)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHRUserReport))
                throw new Exception("User has not permission to perform this operation");

            var data = (await _reportRepository.GetAdvancedReport(clientId, placeId, departmentId, shiftId, employeeId, startDate, endDate, lateBy)).ToList();

            data.ForEach((e) =>
            {
                e.TotalInTime = e.TotalInTime.HasValue ? (long)new TimeSpan(e.TotalInTime.Value).TotalMinutes : 0;
                e.TotalOutTime = e.TotalOutTime.HasValue ? (long)new TimeSpan(e.TotalOutTime.Value).TotalMinutes : 0;
            });

            return data;
        }

        #endregion
    }
}
