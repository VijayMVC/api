using Cllearworks.COH.Web.Utility.Auth;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;

namespace Cllearworks.COH.Auth
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the Application Sign In Cookie.
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = "Application",
            //    AuthenticationMode = AuthenticationMode.Passive,
            //    LoginPath = new PathString("/Account/Login"),
            //    LogoutPath = new PathString("/Account/Logout"),
            //    ExpireTimeSpan = TimeSpan.FromMinutes(30),
            //    SlidingExpiration = true,
            //    CookieName = "COHApp"
            //});

            // Enable the External Sign In Cookie.
            //app.SetDefaultSignInAsAuthenticationType("External");
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = "External",
            //    AuthenticationMode = AuthenticationMode.Passive,
            //    CookieName = CookieAuthenticationDefaults.CookiePrefix + "External",
            //    ExpireTimeSpan = TimeSpan.FromMinutes(5),
            //});

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            // Setup Authorization Server
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = new PathString("/OAuth/Authorize"),
                TokenEndpointPath = new PathString("/OAuth/Token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(25),
                ApplicationCanDisplayErrors = true,
                AllowInsecureHttp = true,
                // Authorization server provider which controls the lifecycle of Authorization Server
                Provider = new COHAuthorizationServerProvider(),
                //Provider = new OAuthAuthorizationServerProvider
                //{
                //    OnValidateClientRedirectUri = COHOAuthProviderHelpers.ValidateClientRedirectUri,
                //    OnValidateClientAuthentication = COHOAuthProviderHelpers.ValidateClientAuthentication,
                //    OnGrantResourceOwnerCredentials = COHOAuthProviderHelpers.GrantResourceOwnerCredentials,
                //    OnGrantClientCredentials = COHOAuthProviderHelpers.GrantClientCredetails
                //},

                //AccessTokenProvider = new AuthenticationTokenProvider
                //{
                //    OnCreate = COHOAuthProviderHelpers.CreateAccessToken,
                //    OnReceive = COHOAuthProviderHelpers.ReceiveAccessToken
                //},

                // Authorization code provider which creates and receives the authorization code.
                AuthorizationCodeProvider = new AuthenticationTokenProvider
                {
                    OnCreate = COHOAuthProviderHelpers.CreateAuthenticationCode,
                    OnReceive = COHOAuthProviderHelpers.ReceiveAuthenticationCode,
                },

                // Refresh token provider which creates and receives refresh token.
                //RefreshTokenProvider = new AuthenticationTokenProvider
                //{
                //    OnCreate = COHOAuthProviderHelpers.CreateRefreshToken,
                //    OnReceive = COHOAuthProviderHelpers.ReceiveRefreshToken,
                //}
            });
        }
    }
}