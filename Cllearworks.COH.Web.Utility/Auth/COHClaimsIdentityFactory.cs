using Microsoft.AspNet.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public class COHClaimsIdentityFactory : ClaimsIdentityFactory<COHApplicationUser, string>
    {
        public COHClaimsIdentityFactory()
        {
        }

        public override async Task<ClaimsIdentity> CreateAsync(UserManager<COHApplicationUser, string> manager, COHApplicationUser user, string authenticationType)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (user == null)
                throw new ArgumentNullException("user");

            var id = new ClaimsIdentity(authenticationType, UserNameClaimType, RoleClaimType);
            if (!string.IsNullOrEmpty(user.ClientId))
                id.AddClaim(new Claim(Constants.ClientIdClaimType, user.ClientId, "http://www.w3.org/2001/XMLSchema#string"));
            if (!string.IsNullOrEmpty(user.ApplicationId))
                id.AddClaim(new Claim(Constants.ApplicationIdClaimType, user.ApplicationId, "http://www.w3.org/2001/XMLSchema#string"));
            if (!string.IsNullOrEmpty(user.ApplicationName))
                id.AddClaim(new Claim(Constants.ApplicationNameClaimType, user.ApplicationName, "http://www.w3.org/2001/XMLSchema#string"));
            if (!string.IsNullOrEmpty(user.Id))
                id.AddClaim(new Claim(UserIdClaimType, user.Id, "http://www.w3.org/2001/XMLSchema#string"));
            if (!string.IsNullOrEmpty(user.UserName))
                id.AddClaim(new Claim(UserNameClaimType, user.UserName, "http://www.w3.org/2001/XMLSchema#string"));
            if (!string.IsNullOrEmpty(user.EmployeeId))
                id.AddClaim(new Claim(Constants.EmployeeIdClaimType, user.Id, "http://www.w3.org/2001/XMLSchema#string"));

            id.AddClaim(new Claim(Constants.ApplicationUserTypeClaimType, user.UserType.ToString(), "http://www.w3.org/2001/XMLSchema#string"));

            id.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"));
            //if (manager.SupportsUserRole)
            //{
            //    var roles = await manager.GetRolesAsync(user.Id);
            //    foreach (var str in roles)
            //        id.AddClaim(new Claim(RoleClaimType, str, "http://www.w3.org/2001/XMLSchema#string"));
            //}
            //if (manager.SupportsUserClaim)
            //    id.AddClaims(await manager.GetClaimsAsync(user.Id));
            return id;
        }
    }
}
