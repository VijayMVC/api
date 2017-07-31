using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Holidays;
using Cllearworks.COH.Models.Holidays;
using Cllearworks.COH.Web.Utility.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Holiday Api
    /// </summary>    
    [RoutePrefix("v1/clients/{clientId}/holidays")]
    public class HolidayController : BaseApiController
    {
        private IHolidayManager _holidayManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="holidayManager"></param>
        public HolidayController(IHolidayManager holidayManager)
        {
            _holidayManager = holidayManager;
        }

        #region Mobile - Without Auth

        /// <summary>
        /// Mobile - Get all leaves of employee
        /// </summary>
        /// <param name="employeeId">Id of the employee</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Holidays")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<HolidayModel>))]
        public async Task<IHttpActionResult> GetHolidaysMe(int employeeId)
        {
            var leaves = await _holidayManager.QueryMeAsync(employeeId);
            return Ok(leaves);
        }

        #endregion Mobile - Without Auth

        #region Mobile - Auth

        /// <summary>
        /// Mobile - Get all leaves of employee
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Holidays")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<HolidayModel>))]
        public async Task<IHttpActionResult> GetHolidaysMe()
        {
            var employeeId = GetEmployeeId();
            var leaves = await _holidayManager.QueryMeAsync(employeeId);
            return Ok(leaves);
        }

        #endregion Mobile - Auth

        /// <summary>
        /// Get all holidays
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<HolidayModel>))]
        public async Task<IHttpActionResult> GetHolidays(int clientId)
        {
            var userId = GetUserId();
            var holidays = await _holidayManager.QueryAsync(clientId, userId);
            return Ok(holidays);
        }

        /// <summary>
        /// Get holiday by id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="holidayId">Id of the holiday</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{holidayId:int}")]
        [HttpGet]
        [ResponseType(typeof(HolidayModel))]
        public async Task<IHttpActionResult> GetHoliday(int clientId, int holidayId)
        {
            var userId = GetUserId();
            var holiday = await _holidayManager.GetAsync(holidayId, clientId, userId);
            return Ok(holiday);
        }

        /// <summary>
        /// Add holiday 
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of holiday</param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        [ResponseType(typeof(HolidayModel))]
        [UserAuthorize]
        public async Task<IHttpActionResult> AddHoliday(int clientId, [FromBody] HolidayModel model)
        {

            var userId = GetUserId();
            var holiday = await _holidayManager.AddAsync(model, clientId, userId);
            return Ok(holiday);
        }

        /// <summary>
        /// Update holiday
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of holiday</param>
        /// <returns></returns>
        [Route("")]
        [HttpPut]
        [ResponseType(typeof(HolidayModel))]
        [UserAuthorize]
        public async Task<IHttpActionResult> UpdateHoliday(int clientId, [FromBody] HolidayModel model)
        {
            var userId = GetUserId();
            var holiday = await _holidayManager.UpdateAsync(model, clientId, userId);
            return Ok(holiday);
        }

        /// <summary>
        /// Delete holiday by id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="holidayId">Id of the holiday</param>
        /// <returns></returns>
        [Route("{holidayId:int}")]
        [HttpDelete]
        [ResponseType(typeof(bool))]
        [UserAuthorize]
        public async Task<IHttpActionResult> DeleteHoliday(int clientId, int holidayId)
        {
            var userId = GetUserId();
            return Ok(await _holidayManager.DeleteAsync(holidayId, clientId, userId));
        }

    }
}
