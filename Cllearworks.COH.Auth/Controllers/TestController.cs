using Cllearworks.COH.Web.Utility.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace Cllearworks.COH.Auth.Controllers
{
    [RoutePrefix("Test")]
    public class TestController : ApiController
    {
        [HttpGet]
        [Route("Auth")]
        [Authorize]
        public IHttpActionResult GetTest()
        {
            return Ok("You are authorize");
        }

        [HttpGet]
        [Route("UserAuth")]
        [UserAuthorize]
        public IHttpActionResult GetTestUserAuth()
        {
            return Ok("Only user auth");
        }

        [Route("NotAuth")]
        [HttpGet]
        public IHttpActionResult GetTestNotAuth()
        {
            var principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

            var claims = from c in principal.Claims
                   select new ViewClaim
                   {
                       Type = c.Type,
                       Value = c.Value
                   };

            return Ok(claims);
        }

        public class ViewClaim
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
