using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Clients;
using Cllearworks.COH.Models.Clients;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Client API
    /// </summary>
    [UserAuthorize]
    [RoutePrefix("v1/clients")]
    public class ClientController : BaseApiController
    {
        private readonly IClientManager _clientManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientManager"></param>
        public ClientController(IClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var userId = GetUserId();
            return Ok(await _clientManager.QueryAsync(userId));
        }

        /// <summary>
        /// Get client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{clientId}")]
        public async Task<IHttpActionResult> Get(int clientId)
        {
            var userId = GetUserId();
            return Ok(await _clientManager.GetAsync(clientId, userId));
        }

        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="model">Client properties</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] ClientModel model)
        {
            var userId = GetUserId();
            var user = await _clientManager.AddAsync(model, userId);
            return Ok(user);
        }

        /// <summary>
        /// Update client
        /// </summary>
        /// <param name="model">Client properties</param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Put([FromBody] ClientModel model)
        {
            var userId = GetUserId();
            var user = await _clientManager.UpdateAsync(model, userId);
            return Ok(user);
        }

        /// <summary>
        /// Delete client
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{clientId:int}")]
        public async Task<IHttpActionResult> Delete(int clientId)
        {
            var myUserId = GetUserId();
            return Ok(await _clientManager.DeleteAsync(clientId, myUserId));
        }

        /// <summary>
        /// Get Subscription Plans
        /// </summary>
        /// <returns></returns>
        [Route("~/v1/SubscriptionPlans")]
        [HttpGet]
        public IHttpActionResult GetSubscriptionPlans()
        {
            var subscriptionPlans = Enum.GetValues(typeof(SubscriptionPlans)).Cast<SubscriptionPlans>().ToList();
            return Ok(subscriptionPlans);
        }
    }
}