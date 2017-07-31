using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Shifts;
using Cllearworks.COH.Models.Shifts;
using Cllearworks.COH.Web.Utility.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Shift API
    /// </summary>
    [UserAuthorize]
    [RoutePrefix("v1/clients/{clientId}/shifts")]
    public class ShiftController : BaseApiController
    {
        private IShiftManager _shiftManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shiftManager"></param>
        public ShiftController(IShiftManager shiftManager)
        {
            _shiftManager = shiftManager;
        }

        /// <summary>
        /// Add shift
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of the shift</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ShiftModel))]
        public async Task<IHttpActionResult> AddShift(int clientId, [FromBody]ShiftModel model)
        {
            var userId = GetUserId();
            var shift = await _shiftManager.AddAsync(model, clientId, userId);
            return Ok(shift);
        }

        /// <summary>
        /// Get all shifts
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(IEnumerable<ShiftModel>))]
        public async Task<IHttpActionResult> GetShifts(int clientId)
        {
            var userId = GetUserId();
            var shift = await _shiftManager.QueryAsync(clientId, userId);
            return Ok(shift);
        }

        /// <summary>
        /// Get shift detail by shift id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="shiftId">Id of the shift</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{shiftId:int}")]
        [ResponseType(typeof(ShiftModel))]
        public async Task<IHttpActionResult> GetShift(int clientId, int shiftId)
        {
            var userId = GetUserId();
            var shift = await _shiftManager.GetAsync(shiftId, clientId, userId);
            return Ok(shift);
        }

        /// <summary>
        /// Delete shift
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="shiftId">Id of the shift</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{shiftId:int}")]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> DeleteShift(int clientId, int shiftId)
        {
            var userId = GetUserId();
            return Ok(await _shiftManager.DeleteAsync(shiftId, clientId, userId));
        }
    }
}
