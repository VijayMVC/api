using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Places;
using Cllearworks.COH.Models.Places;
using Cllearworks.COH.Web.Utility.Auth;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Place Api
    /// </summary>
    [UserAuthorize]
    [RoutePrefix("v1/clients/{clientId}/places")]
    public class PlaceController : BaseApiController
    {
        private IPlaceManager _placeManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="placeManager"></param>
        public PlaceController(IPlaceManager placeManager) {
            _placeManager = placeManager;
        }

        /// <summary>
        /// Get all places by client wise
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IHttpActionResult> Getplaces(int clientId)
        {
            var userId = GetUserId();
            var places = await _placeManager.QueryAsync(clientId, userId);
            return Ok(places);
        }

        /// <summary>
        /// Get place details by placeId
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of Place</param>
        /// <returns></returns>
        [Route("{placeId:int}")]
        [HttpGet]
        public async Task<IHttpActionResult> Getplace(int clientId, int placeId)
        {
            var userId = GetUserId();
            var place = await _placeManager.GetAsync(placeId, clientId, userId);
            return Ok(place);
        }

        /// <summary>
        /// Add Place 
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of Place</param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> AddPlace(int clientId, [FromBody] PlaceModel model) {

            var userId = GetUserId();
            var place = await _placeManager.AddAsync(model, clientId, userId);

            return Ok(place);
        }

        /// <summary>
        /// Update Place
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">Model of Place</param>
        /// <returns></returns>
        [Route("")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdatePlace(int clientId, [FromBody] PlaceModel model)
        {
            var userId = GetUserId();
            var place = await _placeManager.UpdateAsync(model, clientId, userId);
            return Ok(place);
        }

        /// <summary>
        /// Delete the place by Id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="placeId">Id of place</param>
        /// <returns></returns>
        [Route("{placeId:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Deleteplace(int clientId, int placeId)
        {
            var userId = GetUserId();
            return Ok(await _placeManager.DeleteAsync(placeId, clientId, userId));
        }

    }
}
