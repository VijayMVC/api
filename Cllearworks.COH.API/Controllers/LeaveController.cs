using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Leaves;
using Cllearworks.COH.Models.Leaves;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Leave API
    /// </summary>
    [RoutePrefix("v1/clients/{clientId}/leaves")]
    public class LeaveController : BaseApiController
    {
        private ILeaveManager _leaveManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveManager"></param>
        public LeaveController(ILeaveManager leaveManager)
        {
            _leaveManager = leaveManager;
        }

        #region Mobile - Without Auth

        /// <summary>
        /// Mobile - Get all leaves of employee
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Leaves")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<LeaveModel>))]
        public async Task<IHttpActionResult> GetLeavesMe(int employeeId)
        {
            var leaves = await _leaveManager.QueryMeAsync(employeeId);
            return Ok(leaves);
        }

        /// <summary>
        /// Mobile - Get leave by id
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Leaves/{leaveId}")]
        [HttpGet]
        [ResponseType(typeof(LeaveModel))]
        public async Task<IHttpActionResult> GetLeaveMe(int employeeId, int leaveId)
        {
            var leave = await _leaveManager.GetMeAsync(leaveId);
            return Ok(leave);
        }

        /// <summary>
        /// Mobile - Add leave of employee
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="model">Model of leave</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Leaves")]
        [HttpPost]
        [ResponseType(typeof(LeaveModel))]
        public async Task<IHttpActionResult> AddLeave(int employeeId, [FromBody] LeaveModel model)
        {
            var leave = await _leaveManager.AddMeAsync(model, employeeId);
            return Ok(leave);
        }

        /// <summary>
        /// Mobile - Update leave of employee
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="model">Model of leave</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Leaves")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateLeave(int employeeId, [FromBody] LeaveModel model)
        {
            var leave = await _leaveManager.UpdateMeAsync(model);
            return Ok(leave);
        }

        /// <summary>
        /// Mobile - Delete leave by id
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Leaves/{leaveId:int}")]
        [HttpDelete]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> DeleteLeave(int employeeId, int leaveId)
        {
            return Ok(await _leaveManager.DeleteMeAsync(leaveId));
        }

        /// <summary>
        /// Mobile - Cancel my approved leave by id
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Leaves/{leaveId:int}/Cancel")]
        [HttpDelete]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> CancelLeave(int employeeId, int leaveId)
        {
            return Ok(await _leaveManager.CancelMeAsync(leaveId, employeeId));
        }

        #endregion Mobile - Without Auth

        #region Mobile - Auth

        /// <summary>
        /// Mobile - Get all leaves of employee
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Leaves")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<LeaveModel>))]
        public async Task<IHttpActionResult> GetLeavesMe()
        {
            var employeeId = GetEmployeeId();
            var leaves = await _leaveManager.QueryMeAsync(employeeId);
            return Ok(leaves);
        }

        /// <summary>
        /// Mobile - Get leave by id
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Leaves/{leaveId}")]
        [HttpGet]
        [ResponseType(typeof(LeaveModel))]
        public async Task<IHttpActionResult> GetLeaveMe(int leaveId)
        {
            var leave = await _leaveManager.GetMeAsync(leaveId);
            return Ok(leave);
        }

        /// <summary>
        /// Mobile - Add leave of employee
        /// </summary>
        /// <param name="model">Model of leave</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Leaves")]
        [HttpPost]
        [ResponseType(typeof(LeaveModel))]
        public async Task<IHttpActionResult> AddLeave([FromBody] LeaveModel model)
        {
            var employeeId = GetEmployeeId();
            var leave = await _leaveManager.AddMeAsync(model, employeeId);
            return Ok(leave);
        }

        /// <summary>
        /// Mobile - Update leave of employee
        /// </summary>
        /// <param name="model">Model of leave</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Leaves")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateLeave([FromBody] LeaveModel model)
        {
            var leave = await _leaveManager.UpdateMeAsync(model);
            return Ok(leave);
        }

        /// <summary>
        /// Mobile - Delete leave by id
        /// </summary>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Leaves/{leaveId:int}")]
        [HttpDelete]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> DeleteLeave(int leaveId)
        {
            return Ok(await _leaveManager.DeleteMeAsync(leaveId));
        }

        /// <summary>
        /// Mobile - Cancel my approved leave by id
        /// </summary>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Leaves/{leaveId:int}/Cancel")]
        [HttpDelete]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> CancelLeave(int leaveId)
        {
            var employeeId = GetEmployeeId();
            return Ok(await _leaveManager.CancelMeAsync(leaveId, employeeId));
        }

        #endregion Mobile - Auth

        /// <summary>
        /// Get all leaves by client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<LeaveModel>))]
        public async Task<IHttpActionResult> GetLeaves(int clientId, int? page = 1, int? pageSize = 10, int? placeId = null, int? departmentId = null, int? status = null)
        {
            var userId = GetUserId();
            var leaves = await _leaveManager.QueryAsync(clientId, userId, page.GetValueOrDefault(), pageSize.GetValueOrDefault(), placeId, departmentId, status);
            return Ok(leaves);
        }

        /// <summary>
        /// Get all leave by employee
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="employeeId">Id of the employee</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("~/v1/clients/{clientId}/employees/{employeeId}/leaves")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<LeaveModel>))]
        public async Task<IHttpActionResult> GetLeavesByEmployee(int clientId, int employeeId)
        {
            var userId = GetUserId();
            var leaves = await _leaveManager.QueryByEmployeeAsync(employeeId, clientId, userId);
            return Ok(leaves);
        }

        /// <summary>
        /// Get leave by id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{leaveId}")]
        [HttpGet]
        [ResponseType(typeof(LeaveModel))]
        public async Task<IHttpActionResult> GetLeave(int clientId, int leaveId)
        {
            var userId = GetUserId();
            var leave = await _leaveManager.GetAsync(leaveId, clientId, userId);
            return Ok(leave);
        }

        /// <summary>
        /// Approve leave
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{leaveId}/Approve")]
        [HttpPost]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> ApproveAsync(int clientId, int leaveId)
        {
            var userId = GetUserId();
            return Ok(await _leaveManager.ApproveAsync(leaveId, clientId, userId));
        }

        /// <summary>
        /// Reject leave
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{leaveId}/Reject")]
        [HttpPost]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> RejectAsync(int clientId, int leaveId)
        {
            var userId = GetUserId();
            return Ok(await _leaveManager.RejectAsync(leaveId, clientId, userId));
        }

        /// <summary>
        /// Cancel leave
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="leaveId">Id of the leave</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{leaveId}/Cancel")]
        [HttpPost]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> CancelAsync(int clientId, int leaveId)
        {
            var userId = GetUserId();
            return Ok(await _leaveManager.CancelAsync(leaveId, clientId, userId));
        }

        /// <summary>
        /// Get leave types
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("~/v1/LeaveTypes")]
        public IHttpActionResult GetLeaveTypes()
        {
            var leaveTypes = Enum.GetValues(typeof(LeaveTypes)).Cast<LeaveTypes>().ToList();
            return Ok(leaveTypes);
        }
    }
}
