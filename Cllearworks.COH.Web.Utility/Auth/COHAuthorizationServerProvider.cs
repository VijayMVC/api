using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Repository.Applications;
using Cllearworks.COH.Repository.Employees;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public class COHAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
            context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                var appRepository = new ApplicationRepository();                
                if (await appRepository.VerifyApplicationSecretAsync(Guid.Parse(clientId), Guid.Parse(clientSecret)))
                {
                    context.Validated();
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.SetError("invalid_client", "Invalid client id or client secret");
                }
            }
            return;
        }

        //public override async Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        //{
        //    //var identity = new ClaimsIdentity(new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType));
        //    //context.Validated(identity);

        //    var clientGuid = Guid.Parse(context.ClientId);
        //    var appManager = new ApplicationManager();
        //    var app = await appManager.GetApplicationByClientId(clientGuid);
        //    var user = new COHApplicationUser();
        //    if (app != null)
        //    {
        //        user.ClientId = clientGuid.ToString("N");
        //        user.ApplicationName = app.Name;
        //        user.ApplicationId = app.Id.ToString();
        //    }

        //    var userManager = new COHUserManager();
        //    var identity = userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType).Result;
        //    context.Validated(identity);

        //    return;
        //}

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            var clientGuid = Guid.Parse(context.ClientId);
            var appRepository = new ApplicationRepository();
            var app = await appRepository.GetApplicationByClientId(clientGuid);
            COHApplicationUser user = new COHApplicationUser();

            if (app.Scope == "mobile")
            {
                COHEmployeeManager _employeeManager = new COHEmployeeManager();

                user = await _employeeManager.FindAsync(context.UserName, context.Password);

                if (user == null)
                {
                    context.Response.StatusCode = 404;
                    context.SetError("not_registered", "Your are not registered with us.");
                    return;
                }
                else if (user.ApplicationClientId != clientGuid)
                {
                    context.Response.StatusCode = 404;
                    context.SetError("not_registered", "Your are trying to login with different client's app. Please contact to admin.");
                    return;
                }
                else if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    context.Response.StatusCode = 404;
                    context.SetError("not_registered", "Your are not registered with us.");
                    return;
                }
                else if (user.Status == EmployeeStatus.Pending)
                {
                    context.Response.StatusCode = 404;
                    context.SetError("not_approved", "You are registered, but still not approved.");
                    return;
                }
                else if (user.Status != EmployeeStatus.Active)
                {
                    context.Response.StatusCode = 404;
                    context.SetError("not_active", "You are may be inactive or rejected. Please contact to HR");
                    return;
                }
                else if (user.PasswordHash != context.Password)
                {
                    var changeRequestRepository = new ChangeRequestRepository();
                    var changeRequestExist = changeRequestRepository.IsChangeRequestExist(context.UserName, context.Password);
                    if (changeRequestExist)
                    {
                        context.Response.StatusCode = 501;
                        context.SetError("not_approved", "Your device change request still not approved.");
                        return;
                    }
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

                var identity = await _employeeManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);

                context.Validated(identity);
            }
            else
            {
                COHUserManager _manager = new COHUserManager();

                user = await _manager.FindAsync(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

                //var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                //identity.AddClaim(new Claim("sub", context.UserName));
                //identity.AddClaim(new Claim("role", "user"));
                var identity = await _manager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);

                context.Validated(identity);
            }

            return;
        }
    }
}
