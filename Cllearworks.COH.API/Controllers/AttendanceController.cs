using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Attendances;
using Cllearworks.COH.Models.Attendances;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Attendance API
    /// </summary>
    [RoutePrefix("")]
    public class AttendanceController : BaseApiController
    {
        private IAttendanceManager _attendanceManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attendanceManager"></param>
        public AttendanceController(IAttendanceManager attendanceManager)
        {
            _attendanceManager = attendanceManager;
        }

        #region Mobile - Without Auth

        /// <summary>
        /// Mobile - Today's attandance of the user
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="attendanceDate"> Attandance date is optional paramater, If date is not entered then get today's data otherwise get perticular date data. Date should be (MM/dd/yyyy) format</param>
        /// <returns></returns>        
        [Route("~/v1/Employees/{employeeId}/Attendances/Today")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTodayAttendance(int employeeId, DateTime? attendanceDate = null)
        {
            return Ok(await _attendanceManager.GetAttendanceForTodayAsync(employeeId, attendanceDate));
        }

        /// <summary>
        /// Mobile - Attendance check in
        /// </summary>
        /// <param name="employeeId">Id of employee</param>
        /// <param name="remarks">Remarks</param>
        /// <returns></returns>        
        [Route("~/v1/Employees/{employeeId}/Checkin")]
        [HttpPost]
        public async Task<IHttpActionResult> CheckIn(int employeeId, string remarks = null)
        {
            if (employeeId == 0) throw new ArgumentNullException();
            return Ok(await _attendanceManager.CheckInAsync(employeeId, remarks));
        }

        /// <summary>
        /// Mobile - Attendance check out
        /// </summary>
        /// <param name="employeeId">Id of employee</param>
        /// <param name="remarks">Remarks</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Checkout")]
        [HttpPut]
        public async Task<IHttpActionResult> CheckOut(int employeeId, string remarks = null)
        {
            return Ok(await _attendanceManager.CheckOutAsync(employeeId, remarks));
        }

        /// <summary>
        /// Get Attendance Weekly
        /// </summary>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Attendances/Reports/Weekly")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AttendanceModel>))]
        public async Task<IHttpActionResult> GetAttendanceByWeekly(int employeeId)
        {
            return Ok(await _attendanceManager.GetAttendanceByWeeklyAsync(employeeId));
        }
        /// <summary>
        /// Get Attendance by Monthly
        /// </summary>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Attendances/Reports/Monthly")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AttendanceModel>))]
        public async Task<IHttpActionResult> GetAttendanceByMonthly(int employeeId)
        {
            return Ok(await _attendanceManager.GetAttendanceByMonthlyAsync(employeeId));
        }

        /// <summary>
        /// Get Attendance by Yearly
        /// </summary>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Attendances/Reports/Yearly")]
        [HttpGet]
        [ResponseType(typeof(AttendanceByYearModel))]
        public async Task<IHttpActionResult> GetAttendanceByYearly(int employeeId)
        {
            return Ok(await _attendanceManager.GetAttendanceByYearlyAsync(employeeId));
        }

        #endregion Mobile - Without Auth

        #region Mobile - Auth

        /// <summary>
        /// Mobile - Today's attandance of the user
        /// </summary>
        /// <param name="attendanceDate"> Attandance date is optional paramater, If date is not entered then get today's data otherwise get perticular date data. Date should be (MM/dd/yyyy) format</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Attendances/Today")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTodayAttendance(DateTime? attendanceDate = null)
        {
            var employeeId = GetEmployeeId();
            return Ok(await _attendanceManager.GetAttendanceForTodayAsync(employeeId, attendanceDate));
        }

        /// <summary>
        /// Mobile - Attendance check in
        /// </summary>
        /// <param name="remarks">Remarks</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Checkin")]
        [HttpPost]
        public async Task<IHttpActionResult> CheckIn(string remarks = null)
        {
            var employeeId = GetEmployeeId();
            if (employeeId == 0) throw new ArgumentNullException();
            return Ok(await _attendanceManager.CheckInAsync(employeeId, remarks));
        }

        /// <summary>
        /// Mobile - Attendance check out
        /// </summary>
        /// <param name="remarks">Remarks</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Checkout")]
        [HttpPut]
        public async Task<IHttpActionResult> CheckOut(string remarks = null)
        {
            var employeeId = GetEmployeeId();

            return Ok(await _attendanceManager.CheckOutAsync(employeeId, remarks));
        }

        /// <summary>
        /// Get Attendance Weekly
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Attendances/Reports/Weekly")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AttendanceModel>))]
        public async Task<IHttpActionResult> GetAttendanceByWeekly()
        {
            var employeeId = GetEmployeeId();
            return Ok(await _attendanceManager.GetAttendanceByWeeklyAsync(employeeId));
        }
        /// <summary>
        /// Get Attendance by Monthly
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Attendances/Reports/Monthly")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<AttendanceModel>))]
        public async Task<IHttpActionResult> GetAttendanceByMonthly()
        {
            var employeeId = GetEmployeeId();
            return Ok(await _attendanceManager.GetAttendanceByMonthlyAsync(employeeId));
        }

        /// <summary>
        /// Get Attendance by Yearly
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Attendances/Reports/Yearly")]
        [HttpGet]
        [ResponseType(typeof(AttendanceByYearModel))]
        public async Task<IHttpActionResult> GetAttendanceByYearly()
        {
            var employeeId = GetEmployeeId();
            return Ok(await _attendanceManager.GetAttendanceByYearlyAsync(employeeId));
        }
        #endregion Mobile - Auth
    }
}
