using Cllearworks.COH.BusinessManager.Applications;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public static class COHOAuthProviderHelpers
    {
        public readonly static ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Called to validate that the context.ClientId is a registered "client_id",
        /// and that the context.RedirectUri a "redirect_uri" registered for that client. 
        /// This only occurs when processing the Authorize endpoint. The application MUST implement this call, and it MUST validate both of those factors before calling context.Validated.
        /// If the context.Validated method is called with a given redirectUri parameter, then IsValidated will only become true if the incoming redirect URI matches the given redirect URI.
        /// If context.Validated is not called the request will not proceed further.
        /// </summary>
        public static Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            //TODO: Need to validate redirect url from Application table
            var redirectUrl = "localhost:8080";
            context.Validated(redirectUrl);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are present on the request.
        /// If the web application accepts Basic authentication credentials, context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. 
        /// If the web application accepts "client_id" and "client_secret" as form encoded POST parameters, context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body. 
        /// If context.Validated is not called the request will not proceed further.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //string clientId;
            //string clientSecret;
            //if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
            //context.TryGetFormCredentials(out clientId, out clientSecret))
            //{
            //    var appManager = new ApplicationManager();
            //    if (appManager.VerifyApplicationSecret(Guid.Parse(clientId), Guid.Parse(clientSecret)))
            //    {
            //        context.Validated();
            //    }
            //    else
            //    {
            //        context.SetError("Invalid client id and client secret");
            //    }
            //}
            context.Validated();
            return Task.FromResult(0);
        }

        public static void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        public static void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }

        public static async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var identity = new ClaimsIdentity(new GenericIdentity(context.UserName, OAuthDefaults.AuthenticationType));
            //context.Validated(identity);

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            COHUserManager _manager = new COHUserManager();

            COHApplicationUser user = await _manager.FindAsync(context.UserName, context.Password);

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

            return;
        }

        public static async Task GrantClientCredetails(OAuthGrantClientCredentialsContext context)
        {
            //var identity = new ClaimsIdentity(new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType));
            //context.Validated(identity);

            var clientGuid = Guid.Parse(context.ClientId);
            var appManager = new ApplicationManager();
            var app = await appManager.GetApplicationByClientId(clientGuid);
            var user = new COHApplicationUser();
            if (app != null)
            {
                user.ClientId = clientGuid.ToString("N");
                user.ApplicationName = app.Name;
                user.ApplicationId = app.Id.ToString();
            }

            var userManager = new COHUserManager();
            var identity = userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType).Result;
            context.Validated(identity);

            return;
        }
        

        public static void CreateAccessToken(AuthenticationTokenCreateContext context)
        {
            //var cache = Connection.GetDatabase();
            //var ticket = context.SerializeTicket();

            //if (context.Token == null)
            //{
            //    var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            //    context.SetToken(token);
            //}

            //object appId;
            //context.OwinContext.Environment.TryGetValue("client_id", out appId);

            //var scopes = new List<string>();
            //cache.Set(context.Token, new AuthTokenModel
            //{
            //    Token = context.Token,
            //    Ticket = ticket,
            //    Expiration = DateTime.UtcNow.AddMinutes(60),
            //    ApplicationId = appId == null ? null : appId.ToString(),
            //    Scopes = scopes,
            //    Type = AuthTokenTypes.AccessToken
            //});
            //cache.KeyExpire(context.Token, TimeSpan.FromDays(1), CommandFlags.FireAndForget);
        }

        public static void ReceiveAccessToken(AuthenticationTokenReceiveContext context)
        {
            //var cache = Connection.GetDatabase();

            //AuthTokenModel authToken = null;
            //try
            //{
            //    authToken = (AuthTokenModel)cache.Get(context.Token);
            //}
            //catch (Exception)
            //{
            //    authToken = (AuthTokenModel)cache.Get(context.Token);
            //}

            //context.DeserializeTicket(authToken.Ticket);
        }

        public static void ExpireAccessToken(AuthenticationTokenReceiveContext context)
        {
            //var cache = Connection.GetDatabase();

            //var authToken = (AuthTokenModel)cache.Get(context.Token);

            //context.DeserializeTicket(authToken.Ticket);

            //context.Ticket.Properties.ExpiresUtc = DateTime.UtcNow;
        }

        public static void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }

        public static void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }
    }
}
