using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Beacons;
using Cllearworks.COH.Models.Beacons;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Beacon API
    /// </summary>
    [RoutePrefix("v1/clients/{clientId}/beacons")]
    public class BeaconController : BaseApiController
    {
        private IBeaconManager _beaconManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="beaconManager"></param>
        public BeaconController(IBeaconManager beaconManager) {
            _beaconManager = beaconManager;
        }

        #region Mobile

        /// <summary>
        /// Mobile - Get all beacon list
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [Route("~/v1/clients/{clientId}/mybeacons")]
        [HttpGet]
        public async Task<IHttpActionResult> GetBeaconsForMobile(int clientId)
        {
            var beacon = await _beaconManager.QueryAsyncForMobile(clientId);
            return Ok(beacon);
        }

        #endregion Mobile

        /// <summary>
        /// Add an beacon
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of the beacon</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddBeacon(int clientId, [FromBody] BeaconModel model)
        {
            var userId = GetUserId();
            var beacon = await _beaconManager.AddAsync(model, clientId, userId);
            return Ok(beacon);
        }

        /// <summary>
        /// Update an beacon
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of the beacon</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateBeacon(int clientId, [FromBody] BeaconModel model)
        {
            var userId = GetUserId();
            var beacon = await _beaconManager.UpdateAsync(model, clientId, userId);
            return Ok(beacon);
        }

        /// <summary>
        /// Get all beacon list
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> GetBeacons(int clientId)
        {
            var userId = GetUserId();
            var beacon = await _beaconManager.QueryAsync(clientId, userId);
            return Ok(beacon);
        }

        /// <summary>
        /// Get beacon details by beacon Id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="beaconId">Id of beacon</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{beaconId:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetBeacon(int clientId, int beaconId)
        {
            var userId = GetUserId();
            var beacon = await _beaconManager.GetAsync(beaconId, clientId, userId);
            return Ok(beacon);
        }

        /// <summary>
        /// Delete beacon
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="beaconId">Id of beacon</param>
        /// <returns></returns>
        [UserAuthorize]
        [Route("{beaconId:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteBeacon(int clientId, int beaconId)
        {
            var userId = GetUserId();
            return Ok(await _beaconManager.DeleteAsync(beaconId, clientId, userId));
        }

        /// <summary>
        /// Get beacontypes
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("~/v1/beacontypes")]
        [HttpGet]
        public IHttpActionResult GetBeaconType()
        {
            var beaconTypes = Enum.GetValues(typeof(BeaconTypes)).Cast<BeaconTypes>().ToList();
            return Ok(beaconTypes);
        }
    }
}
