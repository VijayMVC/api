using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Cllearworks.COH.API.Controllers.Base
{
    /// <summary>
    /// 
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// Get user id from identity
        /// </summary>
        /// <returns></returns>
        public int GetUserId()
        {
            var userId = "0";
            var identity = HttpContext.Current.GetOwinContext().Authentication.User;
            if (identity != null)
            {
                userId = identity.FindFirst(Constants.UserIdClaimType).Value;
            }

            return Int32.Parse(userId);
        }

        /// <summary>
        /// Get employee id from identity
        /// </summary>
        /// <returns></returns>
        public int GetEmployeeId()
        {
            var employeeId = "1";
            var identity = HttpContext.Current.GetOwinContext().Authentication.User;
            if (identity != null)
            {
                employeeId = identity.FindFirst(Constants.EmployeeIdClaimType).Value;
            }

            return Int32.Parse(employeeId);
        }
    }
}
