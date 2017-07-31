using Cllearworks.COH.Models.Attendances;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Attendances
{
    public interface IAttendanceManager
    {
        #region Mobile

        Task<AttendanceModel> CheckInAsync(int userId, string remarks);
        Task<AttendanceModel> CheckOutAsync(int userId, string remarks);
        Task<AttendanceModel> GetAttendanceForTodayAsync(int employeeId, DateTime? attendanceDate = null);

        #region Reports
        Task<IEnumerable<AttendanceModel>> GetAttendanceByWeeklyAsync(int employeeId);
        Task<IEnumerable<AttendanceModel>> GetAttendanceByMonthlyAsync(int employeeId);
        Task<AttendanceByYearModel> GetAttendanceByYearlyAsync(int employeeId);
        #endregion

        #endregion Mobile
    }
}
