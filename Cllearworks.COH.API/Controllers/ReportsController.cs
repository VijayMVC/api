using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Reports;
using Cllearworks.COH.Models.Reports;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// All Reports
    /// </summary>
    [RoutePrefix("v1/clients/{clientId}/reports")]
    public class ReportsController : BaseApiController
    {
        private IReportsManager _reportsManager;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportsManager"></param>
        public ReportsController(IReportsManager reportsManager)
        {
            _reportsManager = reportsManager;
        }

        #region Attendance Reports

        /// <summary>
        /// Get Employee Reports by year and month        
        /// </summary>
        /// <param name="employeeId">Id of employee</param>
        /// <param name="month">Month of year</param>
        /// <param name="year">Year</param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("Attendance/GetAttendanceByEmployee")]
        [HttpGet]
        [ResponseType(typeof(AttendanceByEmployeeReportModel))]
        public async Task<IHttpActionResult> GetEmployeeAttendanceReports(int clientId, int month, int year, int employeeId)
        {
            var userId = GetUserId();
            var attendanceByEmployee = await _reportsManager.GetAttendanceByEmployeeAsync(employeeId, month, year, clientId, userId);
            return Ok(attendanceByEmployee);
        }

        /// <summary>
        /// Get Attendance by date
        /// </summary>
        /// <param name="clientId">Id of client</param>
        /// <param name="date">Date of attendance</param>
        /// <returns></returns>        
        [UserAuthorize]
        [Route("Attendance/GetAttendanceByDate")]
        [HttpGet]
        [ResponseType(typeof(AttendanceByDateModel))]
        public async Task<IHttpActionResult> GetAttendanceByDate(int clientId, DateTime date)
        {
            var userId = GetUserId();
            var attendanceByDate = await _reportsManager.GetAttendanceByDateAsync(clientId, userId, date);
            return Ok(attendanceByDate);
        }

        /// <summary>
        /// Get Employee Yearly Attendance Report
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="employeeId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("Attendance/GetAttendanceByYearly")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AttendanceByYearlyModel>))]
        public async Task<IHttpActionResult> GetAttendanceByYearly(int clientId, int employeeId, int year)
        {
            var userId = GetUserId();
            var attendanceByYearData = await _reportsManager.GetAttendanceByYearlyAsync(clientId, userId, employeeId, year);
            return Ok(attendanceByYearData);
        }

        #endregion

        #region Advanced Report

        /// <summary>
        /// Get advanced report
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of the place</param>
        /// <param name="departmentId">Id of the department</param>
        /// <param name="shiftId">Id of the shift</param>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="startDate">Attendance start date</param>
        /// <param name="endDate">Attendance end date</param>
        /// <param name="lateBy">Late by selected minutes: Options: null, 15, 30, 45, 60</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("Advanced")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AdvancedReportModel>))]
        public async Task<IHttpActionResult> GetAdvancedReport(int clientId, int? placeId = null, int? departmentId = null, int? shiftId = null, int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null, int? lateBy = null)
        {
            var userId = GetUserId();
            var data = await _reportsManager.GetAdvancedReport(placeId, departmentId, shiftId, employeeId, startDate, endDate, lateBy, clientId, userId);
            return Ok(data);
        }

        #endregion Advanced Report
    }
}
