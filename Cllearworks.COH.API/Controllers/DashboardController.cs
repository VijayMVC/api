using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Dashboard;
using Cllearworks.COH.Models.Dashboard;
using Cllearworks.COH.Utility;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Dashboard API
    /// </summary>
    //[UserAuthorize]
    [RoutePrefix("v1/clients/{clientId}/dashboard")]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardManager _dashboardManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardManager"></param>
        public DashboardController(IDashboardManager dashboardManager)
        {
            _dashboardManager = dashboardManager;
        }

        /// <summary>
        /// Get dashboard general states by client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of the place</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GeneralStats")]
        [ResponseType(typeof(GeneralStatsModel))]
        public async Task<IHttpActionResult> GetGeneralStatsByClient(int clientId, int? placeId)
        {
            //EmailService.SendEmail("ghanshyam@cllearworks.com", "ghanshyam@cllearworks.com", "Testing", "Testing");

            var userId = 3; //GetUserId();
            return Ok(await _dashboardManager.GetGeneralStatsByClient(clientId, userId, placeId));
        }

        /// <summary>
        /// Get attendance trend by client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of the place</param>
        /// <returns></returns>
        [HttpGet]
        [Route("AttendanceTrend")]
        [ResponseType(typeof(AttendanceTrendModel))]
        public async Task<IHttpActionResult> GetAttendanceTrendByClient(int clientId, int? placeId)
        {
            var userId = 3; //GetUserId();
            return Ok(await _dashboardManager.GetAttendanceTrendByClient(clientId, userId, placeId));
        }

        /// <summary>
        /// Get in-time statistics by client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of the place</param>
        /// <returns></returns>
        [HttpGet]
        [Route("InTimeStatistics")]
        [ResponseType(typeof(InTimeStatisticsModel))]
        public async Task<IHttpActionResult> GetInTimeStatisticsByClient(int clientId, int? placeId)
        {
            var userId = 3; //GetUserId();
            return Ok(await _dashboardManager.GetInTimeStatisticsByClient(clientId, userId, placeId));
        }

        /// <summary>
        /// Get attendance statistics by client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of the place</param>
        /// <returns></returns>
        [HttpGet]
        [Route("AttendanceStatistics")]
        [ResponseType(typeof(AttendanceTrendModel))]
        public async Task<IHttpActionResult> GetAttendanceStatisticsByClient(int clientId, int? placeId)
        {
            var userId = 3; //GetUserId();
            return Ok(await _dashboardManager.GetAttendanceStatisticsByClient(clientId, userId, placeId));
        }

        /// <summary>
        /// Get total employees in office time by time - by client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of the place</param>
        /// <returns></returns>
        [HttpGet]
        [Route("InOfficeEmployeesStatistics")]
        [ResponseType(typeof(AttendanceTrendModel))]
        public async Task<IHttpActionResult> GetInOfficeEmployeesByClient(int clientId, int? placeId)
        {
            var userId = 3; //GetUserId();
            return Ok(await _dashboardManager.GetInOfficeEmployeesByClient(clientId, userId, placeId));
        }
    }
}
