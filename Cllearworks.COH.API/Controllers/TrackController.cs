using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Tracks;
using Cllearworks.COH.Models.Tracks;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Track API
    /// </summary>
    [RoutePrefix("")]
    public class TrackController : BaseApiController
    {
        private ITrackManager _trackManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackManager"></param>
        public TrackController(ITrackManager trackManager)
        {
            _trackManager = trackManager;
        }

        #region Mobile - Without Auth

        /// <summary>
        /// Mobile - Get Today's track
        /// </summary>
        /// <param name="employeeId">Id of employee</param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/v1/Employees/{employeeId}/TodayTracks")]
        public async Task<IHttpActionResult> TodayTrack(int employeeId)
        {
            if (employeeId == 0) throw new ArgumentNullException();
            return Ok(await _trackManager.GetAllByDayAsync(employeeId));
        }

        /// <summary>
        /// Mobile - Daily In-Out Track
        /// </summary>
        /// <param name="employeeId">Id of employee</param>
        /// <param name="model">Model of track</param>
        /// <returns></returns>
        [Route("~/v1/Employees/{employeeId}/Tracks")]
        [HttpPost]
        public async Task<IHttpActionResult> DailyInOutTrack(int employeeId, [FromBody] IEnumerable<TrackModel> model)
        {
            if (model == null) throw new ArgumentNullException();

            return Ok(await _trackManager.AddCollectionAsync(model));
        }

        #endregion Mobile - Without Auth

        #region Mobile - Auth

        /// <summary>
        /// Mobile - Get Today's track
        /// </summary>
        /// <returns></returns>
        [EmployeeAuthorize]
        [HttpGet]
        [Route("~/v1/Employees/Me/TodayTracks")]
        public async Task<IHttpActionResult> TodayTrack()
        {
            var employeeId = GetEmployeeId();
            if (employeeId == 0) throw new ArgumentNullException();
            return Ok(await _trackManager.GetAllByDayAsync(employeeId));
        }

        /// <summary>
        /// Mobile - Daily In-Out Track
        /// </summary>
        /// <param name="model">Model of track</param>
        /// <returns></returns>
        [EmployeeAuthorize]
        [Route("~/v1/Employees/Me/Tracks")]
        [HttpPost]
        [ResponseType(typeof(IEnumerable<TrackModel>))]
        public async Task<IHttpActionResult> DailyInOutTrack([FromBody] IEnumerable<TrackModel> model)
        {
            if (model == null) throw new ArgumentNullException();

            return Ok(await _trackManager.AddCollectionAsync(model));
        }

        #endregion Mobile - Auth

        /// <summary>
        /// Get all tracks by attendance id
        /// </summary>
        /// <param name="clientId"></param>        
        /// <param name="attendanceId"></param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("~/v1/Clients/{clientId}/Attendances/{attendanceId}/Tracks")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<TrackDetailModel>))]
        public async Task<IHttpActionResult> GetTracksByAttendanceIdAsync(int clientId, int attendanceId)
        {
            var userId = GetUserId();
            return Ok(await _trackManager.GetTracksByAttendanceIdAsync(clientId, userId, attendanceId));
        }
    }
}
