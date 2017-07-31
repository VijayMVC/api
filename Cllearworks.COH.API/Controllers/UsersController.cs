using Cllearworks.COH.API.Controllers.Base;
using Cllearworks.COH.BusinessManager.Users;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Web.Http.Description;

namespace Cllearworks.COH.API.Controllers
{
    /// <summary>
    /// Users API
    /// </summary>
    [UserAuthorize]
    [RoutePrefix("v1/clients/{clientId}/users")]
    public class UsersController : BaseApiController
    {
        private readonly IUsersManager _userManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        public UsersController(IUsersManager userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get(int clientId)
        {
            var userId = GetUserId();
            return Ok(await _userManager.GetUsersAsync(clientId, userId));
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}")]
        public async Task<IHttpActionResult> Get(int clientId, int userId)
        {
            var myUserId = GetUserId();
            return Ok(await _userManager.GetUserByIdAsync(userId, clientId, myUserId));
        }

        /// <summary>
        /// Get me
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/v1/users/me")]
        [ResponseType(typeof(UserMeModel))]
        public async Task<IHttpActionResult> GetMe()
        {
            var userId = GetUserId();
            return Ok(await _userManager.GetMeAsync(userId));
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">User model properties</param>
        /// <param name="password">New password for user</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post(int clientId, [FromBody] UserModel model, string password)
        {
            var userId = GetUserId();
            var user = await _userManager.AddAsync(model, password, clientId, userId);

            return Ok(user);
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="model">User model properties</param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> Put(int clientId, [FromBody] UserModel model)
        {
            var userId = GetUserId();
            var user = await _userManager.UpdateAsync(model, clientId, userId);

            return Ok(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="clientId">Id of the client</param>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{userId:int}")]
        public async Task<IHttpActionResult> Delete(int clientId, int userId)
        {
            var myUserId = GetUserId();
            return Ok(await _userManager.DeleteAsync(userId, clientId, myUserId));
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("~/v1/userroles")]
        public IHttpActionResult GetUserRoles()
        {
            var userRoles = Enum.GetValues(typeof(UserRoles)).Cast<UserRoles>().ToList();
            return Ok(userRoles);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="oldPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/v1/users/me/password")]
        public async Task<IHttpActionResult> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            var userId = GetUserId();
            var user = await _userManager.ChangePasswordAsync(oldPassword, newPassword, userId);

            return Ok(user);
        }

        /// <summary>
        /// Forgot password
        /// </summary>
        /// <param name="userEmail">Email of the user</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("~/v1/users/password/forgot")]
        public async Task<IHttpActionResult> ForgotPasswordAsync(string userEmail)
        {
            var result = await _userManager.ForgotPasswordAsync(userEmail);

            return Ok(true);
        }

        /// <summary>
        /// Send email to user
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{userId:int}/SendEmail")]
        public async Task<IHttpActionResult> SendEmailAsync(int clientId, int userId)
        {
            var myUserId = GetUserId();
            var result = await _userManager.SendEmailAsync(userId, clientId, myUserId);
            return Ok(true);
        }
    }
}
