using Cllearworks.COH.Models.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Reports
{
    public interface IReportsManager
    {
        #region Attendance

        //AttendacebyEmployee
        Task<IEnumerable<AttendanceByEmployeeReportModel>> GetAttendanceByEmployeeAsync(int employeeId, int month, int year, int clientId, int userId);
        //AttendacebyDate
        Task<IEnumerable<AttendanceByDateModel>> GetAttendanceByDateAsync(int clientId, int userId, DateTime date);

        Task<IEnumerable<AttendanceByYearlyModel>> GetAttendanceByYearlyAsync(int clientId,int userId, int employeeId, int year);

        Task<IEnumerable<AdvancedReportModel>> GetAdvancedReport(int? placeId, int? departmentId, int? shiftId, int? employeeId, DateTime? startDate, DateTime? endDate, int? lateBy, int clientId, int userId);

        #endregion
    }
}
