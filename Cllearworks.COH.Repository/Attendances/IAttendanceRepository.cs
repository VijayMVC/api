using System;
using System.Linq;
using System.Threading.Tasks;
using Cllearworks.COH.Models.Attendances;
using System.Collections.Generic;

namespace Cllearworks.COH.Repository.Attendances
{
    public interface IAttendanceRepository
    {
        #region Mobile

        Task<Attendance> CheckInAsync(Attendance dataModel);
        Task<Attendance> CheckOutAsync(Attendance dataModel);
        Task<Attendance> GetAttendanceForTodayAsync(int userId, DateTime? attendanceDate = null);


        #region Reports
        Task<IEnumerable<AttendanceModel>> GetAttendanceByWeeklyAsync(int employeeId);
        Task<IQueryable<Attendance>> GetAttendanceByMonthlyAsync(int employeeId);
        Task<AttendanceByYearModel> GetAttendanceByYearlyAsync(int employeeId);
        #endregion

        #endregion Mobile

    }
}
