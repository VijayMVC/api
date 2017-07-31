using Cllearworks.COH.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Reports
{
    public interface IReportsRepository
    {
        #region Attendance

        //AttendacebyEmployee
        Task<IEnumerable<AttendanceByEmployeeReportModel>> GetAttendanceByEmployeeAsync(int employeeId, int month, int year);
        //AttendacebyDate
        Task<IEnumerable<AttendanceByDateModel>> GetAttendanceByDateAsync(int clientId, DateTime date);

        Task<IEnumerable<AttendanceByYearlyModel>> GetAttendanceByYearlyAsync(int employeeId, int year);

        Task<IEnumerable<AdvancedReportModel>> GetAdvancedReport(int clientId, int? placeId, int? departmentId, int? shiftId, int? employeeId, DateTime? startDate, DateTime? endDate, int? lateBy);

        #endregion
    }
}
